using NightlyCode.Ai.Net.Operations;

namespace NightlyCode.Ai.Net.DynamicBinOp;

public struct BinOpNeuron {
    public float Value { get; set; }
    public float OrderNumber { get; set; }
    public AggregateType Aggregate { get; set; }
    public ActivationFunc Activation { get; set; }
}