using ILGPU;
using ILGPU.Runtime;
using Pooshit.Ai.Net;
using Pooshit.Ai.Net.DynamicBO;
using Pooshit.Ai.Neurons;

namespace Pooshit.Ai.IlGpu;

/// <summary>
/// ilgpu version for a dynamic binary operation net
/// </summary>
public class DynamicBONetILGPU : INeuronalNet<DynamicBOConfiguration>, IDisposable {
    readonly Accelerator accelerator;
    readonly MemoryBuffer1D<float, Stride1D.Dense> values;
    readonly ArrayView1D<float, Stride1D.Dense> valueView;

    readonly Dictionary<string, int> named = new();
    readonly DynamicBOConfiguration configuration;

    /// <summary>
    /// creates a new ilgpu version of a dynamic binary operation net
    /// </summary>
    /// <param name="accelerator">accelerator to use</param>
    /// <param name="configuration">net configuration</param>
    public DynamicBONetILGPU(Accelerator accelerator, DynamicBOConfiguration configuration) {
        this.accelerator = accelerator;
        this.configuration = configuration;
        values = accelerator.Allocate1D<float>(configuration.Neurons.Length);
        valueView = values.View;
        foreach (NeuronConfig input in configuration.Neurons) {
            if(!string.IsNullOrEmpty(input.Name))
                named[input.Name] = input.Index;
        }
    }

    /// <inheritdoc />
    public float this[string name] {
        get => valueView[named[name]];
        set => valueView[named[name]] = value;
    }

    public float this[int index] {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public void Compute() {
        throw new NotImplementedException();
    }

    public void SetInputValues(float[] values) {
        if (values.Length != configuration.InputCount)
            throw new ArgumentException("Invalid number of values");
        //Array.Copy(values, neuronValues, values.Length);
    }

    public void Update(DynamicBOConfiguration configuration) {
        throw new NotImplementedException();
    }

    public void Dispose() {
        accelerator?.Dispose();
    }
}