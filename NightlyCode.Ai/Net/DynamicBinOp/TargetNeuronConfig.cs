using NightlyCode.Ai.Net.Configurations;
using NightlyCode.Ai.Net.Operations;

namespace NightlyCode.Ai.Net.DynamicBinOp;

public class TargetNeuronConfig : NeuronConfig {
    public AggregateType Aggregate { get; set; }
    public ActivationFunc Activation { get; set; }
    
    public new TargetNeuronConfig Clone() {
        return new() {
                         OrderNumber = OrderNumber,
                         Index = Index,
                         Aggregate = Aggregate,
                         Activation = Activation
                     };
    }

    public override int GetHashCode() {
        return HashCode.Combine(OrderNumber, Index, Aggregate, Activation);
    }

    /// <inheritdoc />
    public override string ToString() {
        if (Activation == ActivationFunc.None)
            return $"{Index}: {OrderNumber:F2} ({Aggregate})";
        return $"{Index}: {OrderNumber:F2} ({Activation}({Aggregate}))";
    }
}