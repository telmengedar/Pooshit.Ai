using Pooshit.Ai.Genetics;
using Pooshit.Ai.Net.DynamicBO;
using Pooshit.Ai.Net.DynamicFF;

namespace NightlyCode.Ai.Tests.Benchmarks;

/// <summary>
/// the fixed problem set the benchmark measures - lifted verbatim from existing
/// <see cref="CalculatorTests"/> demos so no new training data is authored (design #9072 §15 Q1,
/// confirmed by Toni). <c>BinOp.MultiplyMinus</c> and <c>FeedForward.MultiplyMinus</c> share the
/// same 21 samples across the two chromosome families deliberately - the pairing is what
/// separates "which family is better at this" from "which problem is harder"
/// </summary>
public static class BenchmarkProblems {

    static TrainingSample[] MultiplyMinusSamples() => [
        new(new { x = 5, y = 2, z = 7 }, new { result = 3 }),
        new(new { x = 3, y = 3, z = 3 }, new { result = 6 }),
        new(new { x = 10, y = 10, z = 2 }, new { result = 98 }),
        new(new { x = 5, y = 5, z = 1 }, new { result = 24 }),
        new(new { x = 1, y = 40, z = 9 }, new { result = 31 }),
        new(new { x = 6, y = 10, z = 10 }, new { result = 50 }),
        new(new { x = 7, y = 8, z = 6 }, new { result = 50 }),
        new(new { x = 11, y = 8, z = 6 }, new { result = 82 }),
        new(new { x = 2, y = 70, z = 12 }, new { result = 128 }),
        new(new { x = 12, y = 12, z = 4 }, new { result = 140 }),
        new(new { x = 9, y = 12, z = 19 }, new { result = 89 }),
        new(new { x = 1, y = 2, z = 3 }, new { result = -1 }),
        new(new { x = 8, y = 3, z = 8 }, new { result = 16 }),
        new(new { x = 2, y = 34, z = 9 }, new { result = 59 }),
        new(new { x = 8, y = 66, z = 3 }, new { result = 525 }),
        new(new { x = 20, y = 6, z = 333 }, new { result = -213 }),
        new(new { x = 4, y = 60, z = 399 }, new { result = -159 }),
        new(new { x = 7, y = 18, z = 170 }, new { result = -49 }),
        new(new { x = -3, y = 7, z = 20 }, new { result = -41 }),
        new(new { x = -3, y = 8, z = 20 }, new { result = -44 }),
        new(new { x = -3, y = -8, z = 20 }, new { result = 4 }),
    ];

    static float[] GenderNameVector(string name) {
        name = name.ToLower();
        float[] values = new float[20];
        for (int i = 0; i < values.Length; ++i)
            values[i] = i < name.Length ? (byte)name[i] : 0.0f;
        return values;
    }

    static TrainingSample[] GenderSamples() => [
        new(GenderNameVector("Matthias"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Ina"), new { male = 0, female = 1, @object = 0 }),
        new(GenderNameVector("Monika"), new { male = 0, female = 1, @object = 0 }),
        new(GenderNameVector("Heinz"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Ali"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Mohammed"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Jesus"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Theresa"), new { male = 0, female = 1, @object = 0 }),
        new(GenderNameVector("Sandra"), new { male = 0, female = 1, @object = 0 }),
        new(GenderNameVector("Brunhilde"), new { male = 0, female = 1, @object = 0 }),
        new(GenderNameVector("Siegfried"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Gangolf"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Rolf"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Sieglinde"), new { male = 0, female = 1, @object = 0 }),
        new(GenderNameVector("Tina"), new { male = 0, female = 1, @object = 0 }),
        new(GenderNameVector("Matthilda"), new { male = 0, female = 1, @object = 0 }),
        new(GenderNameVector("Hilda"), new { male = 0, female = 1, @object = 0 }),
        new(GenderNameVector("Friedrich"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Gisela"), new { male = 0, female = 1, @object = 0 }),
        new(GenderNameVector("Tom"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Lisa"), new { male = 0, female = 1, @object = 0 }),
        new(GenderNameVector("Cheryl"), new { male = 0, female = 1, @object = 0 }),
        new(GenderNameVector("Leo"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Martin"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Selene"), new { male = 0, female = 1, @object = 0 }),
        new(GenderNameVector("Nathan"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Christopher"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Christian"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Kristin"), new { male = 0, female = 1, @object = 0 }),
        new(GenderNameVector("Lucy"), new { male = 0, female = 1, @object = 0 }),
        new(GenderNameVector("Cloud"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Kina"), new { male = 0, female = 1, @object = 0 }),
        new(GenderNameVector("Timon"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Monika"), new { male = 0, female = 1, @object = 0 }),
        new(GenderNameVector("Peter"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Oliver"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Idriss"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Katharina"), new { male = 0, female = 1, @object = 0 }),
        new(GenderNameVector("Olga"), new { male = 0, female = 1, @object = 0 }),
        new(GenderNameVector("Susanne"), new { male = 0, female = 1, @object = 0 }),
        new(GenderNameVector("Susi"), new { male = 0, female = 1, @object = 0 }),
        new(GenderNameVector("Horst"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Karl"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Mandy"), new { male = 0, female = 1, @object = 0 }),
        new(GenderNameVector("Jörg"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Irene"), new { male = 0, female = 1, @object = 0 }),
        new(GenderNameVector("Marco"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Theodor"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Paul"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Julia"), new { male = 0, female = 1, @object = 0 }),
        new(GenderNameVector("Felix"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Charlotte"), new { male = 0, female = 1, @object = 0 }),
        new(GenderNameVector("Brad"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Konstanze"), new { male = 0, female = 1, @object = 0 }),
        new(GenderNameVector("Sebastian"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Angela"), new { male = 0, female = 1, @object = 0 }),
        new(GenderNameVector("Eberhart"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Jeanne"), new { male = 0, female = 1, @object = 0 }),
        new(GenderNameVector("Anja"), new { male = 0, female = 1, @object = 0 }),
        new(GenderNameVector("Dennis"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Ronald"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Sindy"), new { male = 0, female = 1, @object = 0 }),
        new(GenderNameVector("Juliane"), new { male = 0, female = 1, @object = 0 }),
        new(GenderNameVector("Lilith"), new { male = 0, female = 1, @object = 0 }),
        new(GenderNameVector("Marcel"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Klaus"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Ben"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Bill"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Wesley"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Beverly"), new { male = 0, female = 1, @object = 0 }),
        new(GenderNameVector("Maurice"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Ryan"), new { male = 1, female = 0, @object = 0 }),
        new(GenderNameVector("Daniel"), new { male = 1, female = 0, @object = 0 }),
    ];

    /// <summary>
    /// the three benchmark problems, in the fixed order they are reported
    /// </summary>
    public static readonly BenchmarkProblem[] All = [
        new BenchmarkProblem<DynamicBOConfiguration, DynamicBONet>(
            "BinOp.MultiplyMinus",
            populationSize: 100,
            generator: rng => new(["x", "y", "z"], ["result"], rng),
            samples: MultiplyMinusSamples,
            runs: 500,
            rivalism: 5,
            targetFitness: float.Epsilon),
        new BenchmarkProblem<DynamicFFConfiguration, DynamicFFNet>(
            "FeedForward.MultiplyMinus",
            populationSize: 100,
            generator: rng => new(["x", "y", "z"], ["result"], rng),
            samples: MultiplyMinusSamples,
            runs: 500,
            rivalism: 5,
            targetFitness: float.Epsilon),
        new BenchmarkProblem<DynamicFFConfiguration, DynamicFFNet>(
            "FeedForward.Gender",
            populationSize: 100,
            generator: rng => new(20, ["male", "female", "object"], rng),
            samples: GenderSamples,
            runs: 500,
            rivalism: 5,
            targetFitness: 0.01f),
    ];
}
