namespace Vorcyc.Mathematics.Framework;


public enum ComputingBackend
{
    //ManagedCode,
    //HeterogeneousCPU,
    //HeterogeneousGPU,
    //HeterogeneousFPGA,
    //HeterogeneousASIC,

    CPU,
    GPU,
    FPGA,
    ASIC,
    TPU,
    NPU,
}


public enum ComputingMethodPriority
{
    Sequential_Normal,
    Sequential_SIMD,
    Parallel,
}