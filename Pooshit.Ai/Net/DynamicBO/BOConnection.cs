using Pooshit.Ai.Extensions;
using Pooshit.Ai.Net.Operations;

namespace Pooshit.Ai.Net.DynamicBO;

public class BOConnection {
    public int Lhs { get; set; }
    
    public int Rhs { get; set; }
    
    public int Target { get; set; }

    public OperationType Operation { get; set; }
    
    public float Weight { get; set; }

    public BOConnection Clone() {
        return new() {
                         Lhs = Lhs,
                         Rhs = Rhs,
                         Target = Target,
                         Operation = Operation,
                         Weight = Weight
                     };
    }

    public override int GetHashCode() {
        return HashCode.Combine(Lhs, Rhs, Target, Operation, Weight);
    }

    public int StructureHash => HashCode.Combine(Lhs, Rhs, Target, Operation);
    
    public override string ToString() {
        if(Rhs==-1)
            return $"[{Lhs}] * {Weight:F2} -> [{Target}]";
        return $"([{Lhs}] {Operation.ToDisplay()} [{Rhs}]) * {Weight} -> [{Target}]";
    }
}