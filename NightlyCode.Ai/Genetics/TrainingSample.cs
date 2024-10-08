using NightlyCode.Ai.Extensions;

namespace NightlyCode.Ai.Genetics;

/// <summary>
/// training sample for fitness evaluation
/// </summary>
public class TrainingSample {

    /// <summary>
    /// creates a new <see cref="TrainingSample"/>
    /// </summary>
    public TrainingSample() { }

    /// <summary>
    /// creates a new <see cref="TrainingSample"/>
    /// </summary>
    /// <param name="inputs">input values</param>
    /// <param name="outputs">expected output values</param>
    public TrainingSample(Dictionary<string, float> inputs, Dictionary<string, float> outputs) {
        Inputs = inputs;
        Outputs = outputs;
    }

    /// <summary>
    /// creates a new <see cref="TrainingSample"/>
    /// </summary>
    /// <param name="inputs">input values</param>
    /// <param name="outputs">expected output values</param>
    public TrainingSample(float[] inputs, Dictionary<string, float> outputs) {
        InputArray = inputs;
        Outputs = outputs;
    }

    /// <summary>
    /// creates a new <see cref="TrainingSample"/>
    /// </summary>
    /// <param name="inputs">input values</param>
    /// <param name="outputs">expected output values</param>
    public TrainingSample(dynamic inputs, dynamic outputs) {
        Inputs = DynamicExtensions.ToDictionary<float>(inputs);
        Outputs = DynamicExtensions.ToDictionary<float>(outputs);
    }

    /// <summary>
    /// creates a new <see cref="TrainingSample"/>
    /// </summary>
    /// <param name="inputs">input values</param>
    /// <param name="outputs">expected output values</param>
    public TrainingSample(float[] inputs, dynamic outputs) {
        InputArray = inputs;
        Outputs = DynamicExtensions.ToDictionary<float>(outputs);
    }

    /// <summary>
    /// inputs for training sample
    /// </summary>
    public Dictionary<string, float> Inputs { get; set; }

    /// <summary>
    /// unnamed input values
    /// </summary>
    public float[] InputArray { get; set; }
    
    /// <summary>
    /// expected output of training sample
    /// </summary>
    public Dictionary<string, float> Outputs { get; set; }
}