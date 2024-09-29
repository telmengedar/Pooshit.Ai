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

    public override string ToString() {
        return $"({Lhs} {Operation} {Rhs}) * {Weight} -> {Target}";
    }
}