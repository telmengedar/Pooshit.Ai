namespace NightlyCode.Ai.Net.Operations;

/// <summary>
/// operation in neuronal net
/// </summary>
public class Operation {
    
    /// <summary>
    /// type of operation
    /// </summary>
    public OperationType Type { get; set; }
    
    /// <summary>
    /// operand used in certain operations
    /// </summary>
    public double Weight { get; set; }

    /// <summary>
    /// operation is broken and has to mutate
    /// </summary>
    public bool IsBroken { get; set; }
    
    /// <summary>
    /// clones this operation
    /// </summary>
    /// <returns>cloned operation</returns>
    public Operation Clone() {
        return new Operation {
                                 Type = Type,
                                 Weight = Weight,
                                 IsBroken = IsBroken
                             };
    }

    bool Equals(Operation other) => Type == other.Type && Weight.Equals(other.Weight) && IsBroken == other.IsBroken;

    public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Operation)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine((int)Type, Weight, IsBroken);

    public override string ToString() {
        return $"{(IsBroken?"!!! - ":"")}{Type}({Weight})";
    }
}