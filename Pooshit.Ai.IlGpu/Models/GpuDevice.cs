namespace Pooshit.Ai.IlGpu.Models;

/// <summary>
/// supported accelerator device
/// </summary>
public enum GpuDevice {
    
    /// <summary>
    /// cpu emulation
    /// </summary>
    Cpu,
    
    /// <summary>
    /// cuda
    /// </summary>
    Cuda,
    
    /// <summary>
    /// open cl
    /// </summary>
    OpenCl
}