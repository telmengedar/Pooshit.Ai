using NightlyCode.Ai.Extensions;
using NightlyCode.Ai.Net.Operations;

namespace NightlyCode.Ai.Net.DynamicBinOp;

public class BinOpConnection {
    public int Lhs { get; set; }
    
    public int Rhs { get; set; }
    
    public int Target { get; set; }

    public OperationType Operation { get; set; }
    
    public float Weight { get; set; }

    public BinOpConnection Clone() {
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