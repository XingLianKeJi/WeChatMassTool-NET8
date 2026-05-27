using WeChatMassTool.Config;

namespace WeChatMassTool.Utils;

/// <summary>
/// 随机延迟工具，模拟人类操作的不规则时间间隔
/// </summary>
public static class HumanSimulator
{
    [ThreadStatic]
    private static Random? _rng;

    private static Random Rng => _rng ??= new Random(Guid.NewGuid().GetHashCode());

    /// <summary>
    /// 均匀分布随机延迟
    /// </summary>
    public static void RandomDelay(int minMs, int maxMs)
    {
        Thread.Sleep(Rng.Next(minMs, maxMs + 1));
    }

    /// <summary>
    /// 正态分布随机延迟，截断到 [minMs, maxMs]
    /// </summary>
    public static void RandomDelayGaussian(int meanMs, int stdDevMs, int minMs, int maxMs)
    {
        var value = (int)NextGaussian(meanMs, stdDevMs);
        value = Math.Max(minMs, Math.Min(maxMs, value));
        Thread.Sleep(value);
    }

    /// <summary>
    /// 随机键盘事件间隔
    /// </summary>
    public static void RandomKeyPause()
    {
        Thread.Sleep(Rng.Next(HumanSimConfig.KeyPauseMin, HumanSimConfig.KeyPauseMax + 1));
    }

    private static double NextGaussian(double mean, double stdDev)
    {
        var u1 = 1.0 - Rng.NextDouble();
        var u2 = 1.0 - Rng.NextDouble();
        var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        return mean + stdDev * randStdNormal;
    }
}
