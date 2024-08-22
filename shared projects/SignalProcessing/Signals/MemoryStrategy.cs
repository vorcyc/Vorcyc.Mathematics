namespace Vorcyc.Mathematics.SignalProcessing.Signals;

/// <summary>
/// 包的内存使用策略
/// </summary>
public enum MemoryStrategy
{
    /// <summary>
    /// 立即模式：用时再分配内存，并用依赖的包来计算。
    /// 这样做可以减少一个包的内存使用，但会增加计算量降低运行速度。
    /// 若使用此策略则相关数据只能在栈中使用！！？？？可行吗？够不够？
    /// </summary>
    Immediate,

    /// <summary>
    /// 预先将两个包都分配在堆里，提速，但是占用内存.
    /// </summary>
    Pre_Allocated,
}
