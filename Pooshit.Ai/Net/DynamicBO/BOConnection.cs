using Pooshit.Ai.Extensions;
using Pooshit.Ai.Net.Operations;

namespace Pooshit.Ai.Net.DynamicBO;

/// <summary>
/// binary operation connection
/// </summary>
public class BOConnection {
    
    /// <summary>
    /// left hand side
    /// </summary>
    public int Lhs { get; set; }
    
    /// <summary>
    /// right hand side
    /// </summary>
    public int Rhs { get; set; }
    
    /// <summary>
    /// target neuron
    /// </summary>
    public int Target { get; set; }

    /// <summary>
    /// operation type
    /// </summary>
    public OperationType Operation { get; set; }
    
    /// <summary>
    /// weight of connection
    /// </summary>
    public float Weight { get; set; }

    /// <summary>
    /// clones this connection
    /// </summary>
    /// <returns></returns>
    public BOConnection Clone() {
        return new() {
                         Lhs = Lhs,
                         Rhs = Rhs,
                         Target = Target,
                         Operation = Operation,
                         Weight = Weight
                     };
    }

    /// <inheritdoc />
    public override int GetHashCode() {
        return HashCode.Combine(Lhs, Rhs, Target, Operation, Weight);
    }

    /// <summary>
    /// generates hash of structure
    /// </summary>
    public int StructureHash => HashCode.Combine(Lhs, Rhs, Target, Operation);

    /// <inheritdoc />
    public override string ToString() {
        if(Rhs==-1)
            return $"[{Lhs}] * {Weight:F2} -> [{Target}]";
        return $"([{Lhs}] {Operation.ToDisplay()} [{Rhs}]) * {Weight} -> [{Target}]";
    }
}