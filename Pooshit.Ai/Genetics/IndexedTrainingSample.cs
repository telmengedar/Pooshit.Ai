using Pooshit.Ai.Neurons;

namespace Pooshit.Ai.Genetics;

/// <summary>
/// training sample
/// </summary>
public class IndexedTrainingSample {

	/// <summary>
	/// indexed input values
	/// </summary>
	public NeuronValue[] Inputs { get; set; }
	
	/// <summary>
	/// unnamed input values
	/// </summary>
	public float[] InputArray { get; set; }

	/// <summary>
	/// expected outputs
	/// </summary>
	public NeuronValue[] Outputs { get; set; }
}