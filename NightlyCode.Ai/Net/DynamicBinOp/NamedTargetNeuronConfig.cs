using NightlyCode.Ai.Net.Operations;

namespace NightlyCode.Ai.Net.DynamicBinOp;

public class NamedTargetNeuronConfig : TargetNeuronConfig {
    public string Name { get; set; }

    public new NamedTargetNeuronConfig Clone() {
        return new() {
                         Name = Name,
                         Index = Index,
                         OrderNumber = OrderNumber,
                         Activation = Activation,
                         Aggregate = Aggregate
                     };
    }

    public override int GetHashCode() {
        return HashCode.Combine(Name, Index, OrderNumber, Activation, Aggregate);
    }

    public override string ToString() {
        if (Activation == ActivationFunc.None)
            return $"{Index}: {Name} ({Aggregate})";
        return $"{Index}: {Name} ({Activation}({Aggregate}))";
    }
}