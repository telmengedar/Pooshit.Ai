namespace NightlyCode.Ai.Net.Operations;

/// <summary>
/// operation in a binary operation net
/// </summary>
public class NeuronalOperation {
    
    /// <summary>
    /// operation to apply to input neurons
    /// </summary>
    public OperationType Operation { get; set; }
    
    /// <summary>
    /// weight value
    /// </summary>
    public float Weight { get; set; }
    
    /// <summary>
    /// left hand side neuron
    /// </summary>
    public NeuronIndex Lhs { get; set; }
    
    /// <summary>
    /// right hand side neuron
    /// </summary>
    public NeuronIndex Rhs { get; set; }
    
    /// <summary>
    /// output neuron
    /// </summary>
    public NeuronIndex Output { get; set; }

    /// <summary>
    /// clones the operation
    /// </summary>
    /// <returns>cloned operation</returns>
    public NeuronalOperation Clone() {
        return new NeuronalOperation {
                                         Operation = Operation,
                                         Weight = Weight,
                                         Lhs = Lhs,
                                         Rhs = Rhs,
                                         Output = Output
                                     };
    }

    protected bool Equals(NeuronalOperation other) => Operation == other.Operation && Weight.Equals(other.Weight) && Equals(Lhs, other.Lhs) && Equals(Rhs, other.Rhs) && Equals(Output, other.Output);

    public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((NeuronalOperation)obj);
    }

    public override int GetHashCode() => HashCode.Combine((int)Operation, Weight, Lhs, Rhs, Output);
}