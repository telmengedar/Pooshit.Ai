using ILGPU;
using ILGPU.Runtime.Cuda;
using ILGPU.Runtime.OpenCL;
using Pooshit.Ai.IlGpu.Models;
using Pooshit.Ai.Net;
using Pooshit.Ai.Net.DynamicBO;

namespace Pooshit.Ai.IlGpu;

/// <inheritdoc />
public class DynamicBOProvider : INeuronalNetProvider<DynamicBOConfiguration>, IDisposable {
    readonly GpuDevice gpuDevice = GpuDevice.Cpu;
    readonly Context context = Context.CreateDefault();

    public DynamicBOProvider() {
        if (context.GetCudaDevices().Count > 0)
            gpuDevice = GpuDevice.Cuda;
        else if (context.GetCLDevices().Count > 0)
            gpuDevice = GpuDevice.OpenCl;
    }

    /// <inheritdoc />
    public async Task<INeuronalNet<DynamicBOConfiguration>> Get(DynamicBOConfiguration configuration) {
        switch (gpuDevice) {
            case GpuDevice.Cpu:
                return new DynamicBONet(configuration);
            default:
                throw new NotSupportedException();
        }
    }

    /// <inheritdoc />
    void IDisposable.Dispose() {
        context?.Dispose();
    }
}