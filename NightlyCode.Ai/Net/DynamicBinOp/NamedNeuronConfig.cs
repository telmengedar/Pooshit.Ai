namespace NightlyCode.Ai.Net.DynamicBinOp;

public class NamedNeuronConfig : NeuronConfig {
    public string Name { get; set; }
    
    public new NamedNeuronConfig Clone() {
        return new() {
                         Name = Name,
                         OrderNumber = OrderNumber,
                         Index = Index
                     };
    }

    public override int GetHashCode() {
        return HashCode.Combine(Name, OrderNumber, Index);
    }

    public override string ToString() {
        return $"{Index}: {Name}";
    }
}