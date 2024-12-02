namespace Pooshit.Ai.Neurons;

/// <summary>
/// specification for a neuron
/// </summary>
public class NeuronSpec {
	
	/// <summary>
	/// creates a new <see cref="NeuronSpec"/>
	/// </summary>
	public NeuronSpec() { }

	/// <summary>
	/// creates a new <see cref="NeuronSpec"/>
	/// </summary>
	/// <param name="name">name of neuron</param>
	/// <param name="generator">generator function</param>
	public NeuronSpec(string name, string generator=null) {
		Name = name;
		Generator = generator;
	}

	/// <summary>
	/// name of neuron
	/// </summary>
	public string Name { get; set; }
	
	/// <summary>
	/// generator function
	/// </summary>
	public string Generator { get; set; }

	/// <summary>
	/// generates a neuronspec from a string
	/// </summary>
	/// <param name="name">name of neuron</param>
	/// <returns><see cref="NeuronSpec"/></returns>
	public static implicit operator NeuronSpec(string name) => new(name);
}