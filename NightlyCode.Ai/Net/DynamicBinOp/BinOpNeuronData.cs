using NightlyCode.Ai.Net.Configurations;
using NightlyCode.Ai.Net.Operations;

namespace NightlyCode.Ai.Net.DynamicBinOp;

public class BinOpNeuronData {
    public float OrderNumber { get; set; }

    public int Index { get; set; }
    
    public AggregateType Aggregate { get; set; }
    public ActivationFunc Activation { get; set; }
    
    public void Randomize(CrossSetup setup) {
        Aggregate = setup.NextAggregate();
        Activation = setup.NextFunc();
    }

    public BinOpNeuronData Clone() {
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
}