namespace Pooshit.Ai.Net.Operations;

/// <summary>
/// function used to transform value for a neuron
/// </summary>
public enum ActivationFunc {

    /// <summary>
    /// value is returned unchanged
    /// </summary>
    None,

    /// <summary>
    /// 0 for negative values, 1 otherwise
    /// </summary>
    BinaryStep,

    /// <summary>
    /// fast/algebraic sigmoid 0.5 * (x / (1 + |x|)) + 0.5 — not the logistic function 1 / (1 + e^-x); saturates far more slowly and does not approach its asymptotes at realistic magnitudes
    /// </summary>
    Sigmoid,

    /// <summary>
    /// sine of the value
    /// </summary>
    Sin,

    /// <summary>
    /// hyperbolic tangent of the value
    /// </summary>
    Tanh,

    /// <summary>
    /// value if positive, 0 otherwise (rectified linear unit)
    /// </summary>
    ReLU,

    /// <summary>
    /// value if positive, 0.1 times the value otherwise
    /// </summary>
    LeakyReLU,

    /// <summary>
    /// reciprocal of the value (1 / x)
    /// </summary>
    Reciprocal,

    /// <summary>
    /// x times the fast/algebraic sigmoid of x — not the published Swish x * (1 / (1 + e^-x))
    /// </summary>
    Swish,

    /// <summary>
    /// square root of the value, computed as the reciprocal of a fast approximate inverse square root
    /// </summary>
    Sqrt,

    /// <summary>
    /// square of the value
    /// </summary>
    Pow2,

    /// <summary>
    /// value rounded down to the nearest integer
    /// </summary>
    Floor,

    /// <summary>
    /// value rounded up to the nearest integer
    /// </summary>
    Ceiling
}