namespace NightlyCode.Ai.Net.Operations;

/// <summary>
/// function used to transform value for a neuron
/// </summary>
public enum ActivationFunc {
    None,
    BinaryStep,
    Sigmoid,
    Sin,
    Tanh,
    ReLU,
    LeakyReLU,
    Reciprocal,
    Swish,
    Sqrt,
    Pow2,
    Floor,
    Ceiling
}