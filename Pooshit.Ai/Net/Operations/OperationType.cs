namespace Pooshit.Ai.Net.Operations;

/// <summary>
/// type of neuronal operation
/// </summary>
public enum OperationType {

    /// <summary>
    /// product of lhs and rhs
    /// </summary>
    Multiply,

    /// <summary>
    /// sum of lhs and rhs
    /// </summary>
    Add,

    /// <summary>
    /// quotient of lhs and rhs; a non-finite result (e.g. division by zero) is replaced with 0
    /// </summary>
    Div,

    /// <summary>
    /// difference of lhs and rhs (lhs minus rhs)
    /// </summary>
    Sub,

    /// <summary>
    /// lhs raised to the power of |rhs|, via a fast bit-hack approximation — the sign of rhs is discarded
    /// </summary>
    Pow,

    /// <summary>
    /// rhs raised to the power of |lhs|, via the same fast approximation as Pow with base and exponent swapped — the sign of lhs is discarded
    /// </summary>
    InvPow,

    /// <summary>
    /// smaller of lhs and rhs
    /// </summary>
    Min,

    /// <summary>
    /// larger of lhs and rhs
    /// </summary>
    Max
}