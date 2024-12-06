namespace Pooshit.Ai.Net.Evaluation;

/// <summary>
/// function used to evaluate chromosome fitness
/// </summary>
public enum EvaluationFunc {
	
	/// <summary>
	/// absolute distance to target value
	/// </summary>
	Distance,
	
	/// <summary>
	/// relative distance to target value
	/// </summary>
	DistancePercent
}