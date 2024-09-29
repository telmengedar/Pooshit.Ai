using NightlyCode.Ai.Net.Configurations;
using NightlyCode.Ai.Net.Operations;

namespace NightlyCode.Ai.Net.DynamicBinOp;

public class NamedBinOpNeuronData : BinOpNeuronData {
    public string Name { get; set; }

    public new NamedBinOpNeuronData Clone() {
        return new() {
                         Name = Name,
                         Index = Index,
                         OrderNumber = OrderNumber,
                         Activation = Activation,
                         Aggregate = Aggregate
                     };
    }
}