using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.Win32;

namespace WeChatMassTool.Utils;

public static class WeChatDbManager
{
    #region Win32 API

    [DllImport("kernel32.dll")]
    private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

    #endregion

    private static readonly string[] CoreDllNames = { "Weixin.dll", "WeChatWin.dll", "wechatwin.dll" };
    private static string? _manualDataDir;

    public static void SetManualDataDir(string? path) => _manualDataDir = path;

    #region 数据目录查找

    public static string? FindWeChatDataDir()
    {
        if (!string.IsNullOrEmpty(_manualDataDir) && Directory.Exists(_manualDataDir))
        {
            var wxidDir = FindWxidDir(_manualDataDir);
            if (wxidDir != null) return wxidDir;
            if (Directory.Exists(Path.Combine(_manualDataDir, "db_storage")) ||
                Directory.Exists(Path.Combine(_manualDataDir, "Msg")))
                return _manualDataDir;
        }

        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Tencent\WeChat");
            if (key != null)
            {
                var fileSavePath = key.GetValue("FileSavePath") as string;
                if (!string.IsNullOrEmpty(fileSavePath) && Directory.Exists(fileSavePath))
                {
                    var dir = SearchWxidInRoot(fileSavePath);
                    if (dir != null) return dir;
                }
            }
        }
        catch { }

        var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var dir2 = SearchWxidInRoot(docs);
        if (dir2 != null) return dir2;

        return null;
    }

    private static string? SearchWxidInRoot(string root)
    {
        foreach (var dirName in new[] { "xwechat_files", "WeChat Files" })
        {
            var path = Path.Combine(root, dirName);
            if (!Directory.Exists(path)) continue;
            var wxidDir = FindWxidDir(path);
            if (wxidDir != null) return wxidDir;
        }
        return FindWxidDir(root);
    }

    private static string? FindWxidDir(string basePath)
    {
        try
        {
            foreach (var dir in Directory.GetDirectories(basePath))
            {
                var dirName = Path.GetFileName(dir);
                if (dirName == "All Users" || dirName == "Applet" || dirName == "Backup") continue;
                if (Directory.Exists(Path.Combine(dir, "db_storage"))) return dir;
                if (Directory.Exists(Path.Combine(dir, "Msg"))) return dir;
                if (dirName.StartsWith("wxid_")) return dir;
            }
        }
        catch { }
        return null;
    }

    public static string? GetContactDbPath(string dataDir)
    {
        var newPath = Path.Combine(dataDir, "db_storage", "contact", "contact.db");
        if (File.Exists(newPath)) return newPath;
        var oldPath = Path.Combine(dataDir, "Msg", "MicroMsg.db");
        if (File.Exists(oldPath)) return oldPath;
        return null;
    }

    public static bool IsDbEncrypted(string dbPath)
    {
        try
        {
            using var fs = new FileStream(dbPath, FileMode.Open, FileAccess.Read);
            var header = new byte[16];
            fs.Read(header, 0, 16);
            var sqliteHeader = Encoding.ASCII.GetBytes("SQLite format 3\0");
            for (int i = 0; i < 16; i++)
                if (header[i] != sqliteHeader[i]) return true;
            return false;
        }
        catch { }
        return true;
    }

    #endregion

    #region 密钥搜索

    /// <summary>
    /// 查找包含核心 DLL 的微信主进程
    /// </summary>
    private static Process? FindMainWeChatProcess()
    {
        foreach (var process in Process.GetProcessesByName("Weixin"))
        {
            try
            {
                process.Refresh();
                foreach (ProcessModule module in process.Modules)
                {
                    foreach (var dllName in CoreDllNames)
                    {
                        if (module.ModuleName?.Equals(dllName, StringComparison.OrdinalIgnoreCase) == true)
                            return process;
                    }
                }
            }
            catch { }
        }
        return null;
    }

    private static (IntPtr baseAddr, int size) FindCoreModule(Process process)
    {
        try
        {
            process.Refresh();
            foreach (ProcessModule module in process.Modules)
            {
                foreach (var dllName in CoreDllNames)
                {
                    if (module.ModuleName?.Equals(dllName, StringComparison.OrdinalIgnoreCase) == true)
                        return (module.BaseAddress, (int)module.ModuleMemorySize);
                }
            }
        }
        catch { }
        return (IntPtr.Zero, 0);
    }

    /// <summary>
    /// 解析 PE 头，查找 .data 段的偏移和大小
    /// </summary>
    private static (int offset, int size) FindDataSectionOffset(byte[] buffer)
    {
        try
        {
            if (buffer.Length < 64 || buffer[0] != 'M' || buffer[1] != 'Z')
                return (0, 0);

            int peOffset = BitConverter.ToInt32(buffer, 0x3C);
            if (peOffset <= 0 || peOffset + 24 > buffer.Length)
                return (0, 0);

            if (buffer[peOffset] != 'P' || buffer[peOffset + 1] != 'E')
                return (0, 0);

            int numberOfSections = BitConverter.ToInt16(buffer, peOffset + 6);
            int sizeOfOptionalHeader = BitConverter.ToInt16(buffer, peOffset + 20);
            int sectionStart = peOffset + 24 + sizeOfOptionalHeader;

            for (int i = 0; i < numberOfSections; i++)
            {
                int off = sectionStart + i * 40;
                if (off + 40 > buffer.Length) break;

                var name = Encoding.ASCII.GetString(buffer, off, 8).TrimEnd('\0');
                var virtualSize = BitConverter.ToInt32(buffer, off + 8);
                var virtualAddress = BitConverter.ToInt32(buffer, off + 12);

                if (name == ".data" && virtualAddress > 0 && virtualSize > 0)
                    return (virtualAddress, Math.Min(virtualSize, buffer.Length - virtualAddress));
            }
        }
        catch { }
        return (0, 0);
    }

    private static int FindPatternOffset(byte[] buffer, byte[] pattern)
    {
        for (int i = 0; i <= buffer.Length - pattern.Length; i++)
        {
            bool found = true;
            for (int j = 0; j < pattern.Length; j++)
            {
                if (buffer[i + j] != pattern[j]) { found = false; break; }
            }
            if (found) return i;
        }
        return -1;
    }

    private static bool IsValidKey(byte[] key)
    {
        if (key.Length != 32) return false;
        var seen = new HashSet<byte>();
        foreach (var b in key) seen.Add(b);
        return seen.Count >= 8 && !key.All(b => b == 0);
    }

    #endregion

    #region 数据库读取

    private static SqliteConnection OpenDatabase(string dbPath)
    {
        if (!IsDbEncrypted(dbPath))
            return OpenPlainDatabase(dbPath);

        var process = FindMainWeChatProcess();
        if (process == null)
            throw new Exception("未找到微信进程，请确认微信正在运行");

        var (moduleBase, moduleSize) = FindCoreModule(process);
        if (moduleBase == IntPtr.Zero)
            throw new Exception("未找到微信核心模块");

        var buffer = new byte[moduleSize];
        if (!ReadProcessMemory(process.Handle, moduleBase, buffer, buffer.Length, out _))
            throw new Exception("无法读取微信进程内存，请以管理员身份运行");

        // 逐步尝试不同策略，找到能用的密钥就立即返回
        SqliteConnection? conn;

        // 策略1: sqlcipher 字符串附近搜索（快速，约3万次迭代）
        conn = TryStrategyNearSqlcipher(process, buffer, dbPath);
        if (conn != null) return conn;

        // 策略2: 特征码附近搜索（快速，约数百次迭代）
        conn = TryStrategyNearFeatureCode(process, buffer, dbPath);
        if (conn != null) return conn;

        // 策略3: .data 段搜索（中等速度，1-5MB 范围）
        conn = TryStrategyDataSection(process, buffer, dbPath);
        if (conn != null) return conn;

        throw new Exception("数据库解密失败：无法找到正确的密钥。\n" +
            "请尝试：1) 以管理员身份运行  2) 确认微信正在运行  3) 按 Ctrl+D 运行诊断");
    }

    /// <summary>
    /// 策略1: 在 sqlcipher 字符串附近搜索指针
    /// </summary>
    private static SqliteConnection? TryStrategyNearSqlcipher(Process process, byte[] buffer, string dbPath)
    {
        var searchBytes = Encoding.ASCII.GetBytes("sqlcipher");
        var offset = FindPatternOffset(buffer, searchBytes);
        if (offset < 0) return null;

        var start = Math.Max(0, offset - 65536);
        var end = Math.Min(buffer.Length - 8, offset + 65536);
        var seen = new HashSet<string>();

        for (int i = start; i < end; i += 4)
        {
            if (i + 8 > buffer.Length) break;
            var ptrValue = BitConverter.ToInt64(buffer, i);
            if (ptrValue < 0x10000 || ptrValue > 0x7FFFFFFFFFFF) continue;

            var keyBuffer = new byte[32];
            if (!ReadProcessMemory(process.Handle, (IntPtr)ptrValue, keyBuffer, 32, out int bytesRead) || bytesRead != 32)
                continue;

            if (!IsValidKey(keyBuffer)) continue;

            var hex = BitConverter.ToString(keyBuffer);
            if (seen.Contains(hex)) continue;
            seen.Add(hex);

            var conn = TryOpenWithKey(dbPath, keyBuffer);
            if (conn != null) return conn;
        }
        return null;
    }

    /// <summary>
    /// 策略2: 在特征码位置附近搜索指针
    /// </summary>
    private static SqliteConnection? TryStrategyNearFeatureCode(Process process, byte[] buffer, string dbPath)
    {
        var seen = new HashSet<string>();

        foreach (var (pattern, offset) in new (byte[], int)[]
        {
            (new byte[] { 0xF7, 0x43, 0x3B, 0xB8 }, 0x08),
            (new byte[] { 0x48, 0x8B, 0x05 }, 0x03),
        })
        {
            var found = FindPatternOffset(buffer, pattern);
            if (found < 0) continue;

            var start = Math.Max(0, found + offset - 512);
            var end = Math.Min(buffer.Length - 8, found + offset + 512);

            for (int i = start; i < end; i += 4)
            {
                if (i + 8 > buffer.Length) break;
                var ptrValue = BitConverter.ToInt64(buffer, i);
                if (ptrValue < 0x10000 || ptrValue > 0x7FFFFFFFFFFF) continue;

                var keyBuffer = new byte[32];
                if (!ReadProcessMemory(process.Handle, (IntPtr)ptrValue, keyBuffer, 32, out int bytesRead) || bytesRead != 32)
                    continue;

                if (!IsValidKey(keyBuffer)) continue;

                var hex = BitConverter.ToString(keyBuffer);
                if (seen.Contains(hex)) continue;
                seen.Add(hex);

                var conn = TryOpenWithKey(dbPath, keyBuffer);
                if (conn != null) return conn;
            }
        }
        return null;
    }

    /// <summary>
    /// 策略3: 在 .data 段搜索密钥指针和直接密钥数据
    /// </summary>
    private static SqliteConnection? TryStrategyDataSection(Process process, byte[] buffer, string dbPath)
    {
        var (dataOffset, dataSize) = FindDataSectionOffset(buffer);
        if (dataOffset <= 0 || dataSize <= 0) return null;

        var seen = new HashSet<string>();

        // 3a: 搜索 .data 段中的指针（指向堆上的密钥）
        var end = Math.Min(buffer.Length - 8, dataOffset + dataSize);
        for (int i = dataOffset; i < end; i += 4)
        {
            if (i + 8 > buffer.Length) break;
            var ptrValue = BitConverter.ToInt64(buffer, i);
            if (ptrValue < 0x10000 || ptrValue > 0x7FFFFFFFFFFF) continue;

            var keyBuffer = new byte[32];
            if (!ReadProcessMemory(process.Handle, (IntPtr)ptrValue, keyBuffer, 32, out int bytesRead) || bytesRead != 32)
                continue;

            if (!IsValidKey(keyBuffer)) continue;

            var hex = BitConverter.ToString(keyBuffer);
            if (seen.Contains(hex)) continue;
            seen.Add(hex);

            var conn = TryOpenWithKey(dbPath, keyBuffer);
            if (conn != null) return conn;
        }

        // 3b: 在 .data 段直接搜索 32 字节高熵序列（密钥可能直接存储在此）
        for (int i = dataOffset; i < dataOffset + dataSize - 32; i += 8)
        {
            if (i + 32 > buffer.Length) break;

            var keyBuffer = new byte[32];
            Array.Copy(buffer, i, keyBuffer, 0, 32);

            if (!IsValidKey(keyBuffer)) continue;

            var hex = BitConverter.ToString(keyBuffer);
            if (seen.Contains(hex)) continue;
            seen.Add(hex);

            var conn = TryOpenWithKey(dbPath, keyBuffer);
            if (conn != null) return conn;
        }

        return null;
    }

    private static SqliteConnection OpenPlainDatabase(string dbPath)
    {
        var tempDbPath = CopyDbToTemp(dbPath);
        var connection = new SqliteConnection($"Data Source={tempDbPath};Mode=ReadOnly;");
        connection.Open();
        return connection;
    }

    /// <summary>
    /// 用指定密钥尝试打开数据库，支持多种 sqlcipher 参数组合
    /// </summary>
    private static SqliteConnection? TryOpenWithKey(string dbPath, byte[] key)
    {
        var hexKey = BitConverter.ToString(key).Replace("-", "");

        var cipherConfigs = new (string name, Action<SqliteConnection> setup)[]
        {
            ("sqlcipher4", conn =>
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"PRAGMA key = \"x'{hexKey}'\"; PRAGMA cipher_compatibility = 4;";
                cmd.ExecuteNonQuery();
            }),
            ("sqlcipher3", conn =>
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"PRAGMA key = \"x'{hexKey}'\"; PRAGMA cipher_compatibility = 3;";
                cmd.ExecuteNonQuery();
            }),
            ("custom_64000", conn =>
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"PRAGMA key = \"x'{hexKey}'\"; " +
                    "PRAGMA kdf_iter = 64000; PRAGMA cipher_page_size = 4096; " +
                    "PRAGMA cipher_hmac_algorithm = HMAC_SHA512; " +
                    "PRAGMA cipher_kdf_algorithm = PBKDF2_HMAC_SHA512;";
                cmd.ExecuteNonQuery();
            }),
            ("custom_256000", conn =>
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"PRAGMA key = \"x'{hexKey}'\"; " +
                    "PRAGMA kdf_iter = 256000; PRAGMA cipher_page_size = 4096; " +
                    "PRAGMA cipher_hmac_algorithm = HMAC_SHA512; " +
                    "PRAGMA cipher_kdf_algorithm = PBKDF2_HMAC_SHA512;";
                cmd.ExecuteNonQuery();
            }),
        };

        foreach (var (name, setup) in cipherConfigs)
        {
            var tempDbPath = CopyDbToTemp(dbPath);
            try
            {
                var connection = new SqliteConnection($"Data Source={tempDbPath};Mode=ReadOnly;");
                connection.Open();
                setup(connection);

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT count(*) FROM sqlite_master;";
                    cmd.ExecuteScalar();
                }

                return connection;
            }
            catch
            {
                try
                {
                    if (File.Exists(tempDbPath))
                    {
                        foreach (var f in Directory.GetFiles(Path.GetDirectoryName(tempDbPath)!, "WeChatMassTool_*"))
                        {
                            try { File.Delete(f); } catch { }
                        }
                    }
                }
                catch { }
            }
        }

        return null;
    }

    private static string CopyDbToTemp(string dbPath)
    {
        var tempDbPath = Path.Combine(Path.GetTempPath(), $"WeChatMassTool_{Guid.NewGuid():N}.db");
        File.Copy(dbPath, tempDbPath, true);

        foreach (var ext in new[] { "-wal", "-shm" })
        {
            var src = dbPath + ext;
            var dst = tempDbPath + ext;
            if (File.Exists(src))
            {
                try { File.Copy(src, dst, true); } catch { }
            }
        }

        return tempDbPath;
    }

    private static void CleanupTempDb(SqliteConnection connection)
    {
        try
        {
            var dbPath = connection.DataSource;
            connection.Close();
            if (!string.IsNullOrEmpty(dbPath) && dbPath.Contains("WeChatMassTool_"))
            {
                foreach (var f in Directory.GetFiles(Path.GetDirectoryName(dbPath)!, "WeChatMassTool_*"))
                {
                    try { File.Delete(f); } catch { }
                }
            }
        }
        catch { }
    }

    public static List<string> GetContacts(string dbPath)
    {
        if (!File.Exists(dbPath))
            throw new FileNotFoundException($"数据库文件不存在: {dbPath}");

        SqliteConnection? connection = null;
        try
        {
            connection = OpenDatabase(dbPath);
            var tables = GetTableNames(connection);
            return TryQueryContacts(connection, tables);
        }
        finally
        {
            if (connection != null) CleanupTempDb(connection);
        }
    }

    private static List<string> GetTableNames(SqliteConnection connection)
    {
        var tables = new List<string>();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            tables.Add(reader.GetString(0));
        return tables;
    }

    private static List<string> TryQueryContacts(SqliteConnection connection, List<string> tables)
    {
        var contacts = new List<string>();

        var tableColumns = new Dictionary<string, List<string>>();
        foreach (var table in tables)
        {
            var cols = new List<string>();
            try
            {
                using var cmd = connection.CreateCommand();
                cmd.CommandText = $"PRAGMA table_info([{table}]);";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    cols.Add(reader.GetString(1).ToLower());
            }
            catch { }
            tableColumns[table] = cols;
        }

        foreach (var (table, cols) in tableColumns)
        {
            bool hasUsername = cols.Any(c => c.Contains("username") || c.Contains("wxid") || c == "id");
            bool hasNickname = cols.Any(c => c.Contains("nickname") || c.Contains("nick") || c.Contains("name"));

            if (!hasUsername || !hasNickname) continue;

            try
            {
                var nickCol = cols.First(c => c.Contains("nickname") || c.Contains("nick") || c.Contains("name"));
                var remarkCol = cols.FirstOrDefault(c => c.Contains("remark"));
                var usernameCol = cols.First(c => c.Contains("username") || c.Contains("wxid") || c == "id");

                using var cmd = connection.CreateCommand();
                cmd.CommandText = $"SELECT [{nickCol}], {(remarkCol != null ? $"[{remarkCol}]" : "NULL")} FROM [{table}] WHERE [{nickCol}] IS NOT NULL AND [{nickCol}] != ''";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var nick = reader.GetString(0);
                    var remark = reader.IsDBNull(1) ? "" : (reader.GetString(1) ?? "");
                    if (!string.IsNullOrEmpty(nick))
                        contacts.Add(string.IsNullOrEmpty(remark) ? nick : remark);
                }

                if (contacts.Count > 0) return contacts;
            }
            catch { }
        }

        return contacts;
    }

    public static Dictionary<string, string> GetChatRoomList(string dbPath)
    {
        var rooms = new Dictionary<string, string>();

        if (!File.Exists(dbPath))
            throw new FileNotFoundException($"数据库文件不存在: {dbPath}");

        SqliteConnection? connection = null;
        try
        {
            connection = OpenDatabase(dbPath);
            var tables = GetTableNames(connection);

            foreach (var table in tables)
            {
                try
                {
                    var cols = new List<string>();
                    using var cmdCols = connection.CreateCommand();
                    cmdCols.CommandText = $"PRAGMA table_info([{table}]);";
                    using var colsReader = cmdCols.ExecuteReader();
                    while (colsReader.Read())
                        cols.Add(colsReader.GetString(1).ToLower());

                    bool hasUsername = cols.Any(c => c.Contains("username") || c.Contains("wxid") || c == "id");
                    bool hasNickname = cols.Any(c => c.Contains("nickname") || c.Contains("nick") || c.Contains("name"));
                    if (!hasUsername || !hasNickname) continue;

                    var usernameCol = cols.First(c => c.Contains("username") || c.Contains("wxid") || c == "id");
                    var nickCol = cols.First(c => c.Contains("nickname") || c.Contains("nick") || c.Contains("name"));

                    using var cmd = connection.CreateCommand();
                    cmd.CommandText = $"SELECT [{nickCol}], [{usernameCol}] FROM [{table}] WHERE [{usernameCol}] LIKE '%@chatroom' AND [{nickCol}] IS NOT NULL AND [{nickCol}] != ''";
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                        rooms[reader.GetString(0)] = reader.GetString(1);

                    if (rooms.Count > 0) return rooms;
                }
                catch { }
            }
        }
        finally
        {
            if (connection != null) CleanupTempDb(connection);
        }

        return rooms;
    }

    public static List<string> GetChatRoomMembers(string dbPath, string? chatRoomName = null)
    {
        var members = new List<string>();

        if (!File.Exists(dbPath))
            throw new FileNotFoundException($"数据库文件不存在: {dbPath}");

        SqliteConnection? connection = null;
        try
        {
            connection = OpenDatabase(dbPath);

            if (!string.IsNullOrEmpty(chatRoomName))
            {
                var tables = GetTableNames(connection);
                foreach (var table in tables)
                {
                    try
                    {
                        var cols = new List<string>();
                        using var cmdCols = connection.CreateCommand();
                        cmdCols.CommandText = $"PRAGMA table_info([{table}]);";
                        using var colsReader = cmdCols.ExecuteReader();
                        while (colsReader.Read())
                            cols.Add(colsReader.GetString(1));

                        var colsLower = cols.Select(c => c.ToLower()).ToList();
                        bool hasRoomData = colsLower.Any(c => c.Contains("roomdata") || c.Contains("member") || c.Contains("roomlist"));
                        bool hasChatRoomId = colsLower.Any(c => c.Contains("chatroom") || c.Contains("username") || c.Contains("id"));

                        if (!hasRoomData || !hasChatRoomId) continue;

                        var idCol = cols[colsLower.FindIndex(c => c.Contains("chatroom") || c.Contains("username") || c == "id")];
                        var memberCol = cols[colsLower.FindIndex(c => c.Contains("roomdata") || c.Contains("member") || c.Contains("roomlist"))];

                        using var cmd = connection.CreateCommand();
                        cmd.CommandText = $"SELECT [{memberCol}] FROM [{table}] WHERE [{idCol}] = @roomId";
                        cmd.Parameters.AddWithValue("@roomId", chatRoomName);
                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            var memberData = result.ToString() ?? "";
                            foreach (var wxid in memberData.Split(new[] { ',', ';', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                var name = GetContactNameFromConnection(connection, wxid.Trim());
                                if (!string.IsNullOrEmpty(name))
                                    members.Add(name);
                            }
                        }
                    }
                    catch { }
                }
            }
            else
            {
                var rooms = GetChatRoomList(dbPath);
                members.AddRange(rooms.Keys);
            }
        }
        finally
        {
            if (connection != null) CleanupTempDb(connection);
        }

        return members;
    }

    private static string? GetContactNameFromConnection(SqliteConnection connection, string wxid)
    {
        try
        {
            var tables = GetTableNames(connection);
            foreach (var table in tables)
            {
                try
                {
                    var cols = new List<string>();
                    using var cmdCols = connection.CreateCommand();
                    cmdCols.CommandText = $"PRAGMA table_info([{table}]);";
                    using var colsReader = cmdCols.ExecuteReader();
                    while (colsReader.Read())
                        cols.Add(colsReader.GetString(1));

                    var colsLower = cols.Select(c => c.ToLower()).ToList();
                    bool hasUsername = colsLower.Any(c => c.Contains("username") || c.Contains("wxid") || c == "id");
                    bool hasNickname = colsLower.Any(c => c.Contains("nickname") || c.Contains("nick") || c.Contains("name"));
                    if (!hasUsername || !hasNickname) continue;

                    var usernameCol = cols[colsLower.FindIndex(c => c.Contains("username") || c.Contains("wxid") || c == "id")];
                    var nickCol = cols[colsLower.FindIndex(c => c.Contains("nickname") || c.Contains("nick") || c.Contains("name"))];
                    var remarkCol = cols.FirstOrDefault(c => c.ToLower().Contains("remark"));

                    using var cmd = connection.CreateCommand();
                    cmd.CommandText = remarkCol != null
                        ? $"SELECT COALESCE(NULLIF([{remarkCol}], ''), [{nickCol}]) FROM [{table}] WHERE [{usernameCol}] = @wxid"
                        : $"SELECT [{nickCol}] FROM [{table}] WHERE [{usernameCol}] = @wxid";
                    cmd.Parameters.AddWithValue("@wxid", wxid);

                    var result = cmd.ExecuteScalar();
                    return result as string;
                }
                catch { }
            }
        }
        catch { }
        return null;
    }

    #endregion
}
