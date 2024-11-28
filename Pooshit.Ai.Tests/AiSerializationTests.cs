using System.IO.Compression;
using Pooshit.Ai.Genetics;
using Pooshit.Ai.Net;
using Pooshit.Ai.Net.DynamicBO;
using Pooshit.Ai.Net.DynamicFF;
using Pooshit.Ai.Neurons;
using Pooshit.Ai.Serialization;
using Pooshit.Json;

namespace NightlyCode.Ai.Tests;

[TestFixture, Parallelizable]
public class AiSerializationTests {

    float[] NameArray(string name) {
        float[] values = new float[20];
        for (int i = 0; i < values.Length; ++i)
            values[i] = i < name.Length ? (byte)name[i] : 0.0f;

        return values;
    }

    [Test, Parallelizable]
    public void SerializeAndDeserializeBO() {
        Population<DynamicBOConfiguration> population = new(100, rng => new(20, ["x", "y", "z"], rng));

        EvolutionSetup<DynamicBOConfiguration> setup = new() {
                                                                 Evaluator = new SamplesEvaluator<DynamicBOConfiguration, DynamicBONet>([
                                                                                                                                            new(NameArray("Spitzenplatz"), new { y = 3 }),
                                                                                                                                            new(NameArray("Kräuterbeet"), new { y = 5 }),
                                                                                                                                            new(NameArray("Muschelstrand"), new { y = 3 }),
                                                                                                                                            new(NameArray("Oldenburg"), new { y = 3 }),
                                                                                                                                            new(NameArray("Fernsehgarten"), new { y = 4 }),
                                                                                                                                            new(NameArray("Ananasrhabarbersalat"), new { y = 8 }),
                                                                                                                                            new(NameArray("Interkontinental"), new { y = 6 }),
                                                                                                                                            new(NameArray("Sonnenallee"), new { y = 5 }),
                                                                                                                                            new(NameArray("Gemüsebrühe"), new { y = 5 }),
                                                                                                                                            new(NameArray("Ölplattform"), new { y = 3 }),
                                                                                                                                            new(NameArray("Mandelbrot"), new { y = 3 }),
                                                                                                                                            new(NameArray("Sonderrecht"), new { y = 3 }),
                                                                                                                                            new(NameArray("Molekularbiologe"), new { y = 8 }),
                                                                                                                                            new(NameArray("Wetteransage"), new { y = 5 }),
                                                                                                                                            new(NameArray("Ananas"), new { y = 3 }),
                                                                                                                                            new(NameArray("Schonungslos"), new { y = 3 }),
                                                                                                                                            new(NameArray("Sauerkraut"), new { y = 5 }),
                                                                                                                                            new(NameArray("Loeffelbiscuit"), new { y = 6 }),
                                                                                                                                            new(NameArray("Pfeiffenblaeser"), new { y = 6 }),
                                                                                                                                            new(NameArray("Affenbrotbaum"), new { y = 5 }),
                                                                                                                                            new(NameArray("Illustrationsbuch"), new { y = 6 }),
                                                                                                                                            new(NameArray("Offenbarung"), new { y = 4 }),
                                                                                                                                            new(NameArray("Hausverkauf"), new { y = 5 }),
                                                                                                                                            new(NameArray("Lampenkranz"), new { y = 3 }),
                                                                                                                                            new(NameArray("Kostenstelle"), new { y = 4 }),
                                                                                                                                            new(NameArray("Oberfachverkaeufer"), new { y = 8 })
                                                                                                                                        ]),
                                                                 Runs = 100,
                                                                 TargetFitness = 0.01f,
                                                                 AfterRun = (index, fitness) => {
                                                                                if ((index & 511) == 0)
                                                                                    Console.WriteLine("{0}: {1}", index, fitness);
                                                                            },
                                                                 Threads = 2
                                                             };
        population.Train(setup);
        
        foreach(PopulationEntry<DynamicBOConfiguration> entry in population.Entries)
        foreach (NeuronConfig input in entry.Chromosome.Neurons.Take(entry.Chromosome.InputCount)) {
            input.Activation = 0;
            input.Aggregate = 0;
        }
        
        string original = Json.WriteString(population.Entries);
        MemoryStream outputMemory = new();
        BrotliStream output=new(outputMemory, CompressionLevel.Optimal);
        AiSerialization.Serialize(population, output);

        Population<DynamicBOConfiguration> deserializedPopulation = new(AiSerialization.Deserialize<DynamicBOConfiguration>(new BrotliStream(new MemoryStream(outputMemory.ToArray()), CompressionMode.Decompress)).ToArray(), null);
        string deserializedJson = Json.WriteString(deserializedPopulation.Entries);

        
        Assert.That(deserializedJson, Is.EqualTo(original));
    }
    
    [Test, Parallelizable]
    public void SerializeAndDeserializeFF() {
        Population<DynamicFFConfiguration> population = new(100, rng => new(20, ["x", "y", "z"], rng));

        EvolutionSetup<DynamicFFConfiguration> setup = new() {
                                                                 Evaluator = new SamplesEvaluator<DynamicFFConfiguration, DynamicFFNet>([
                                                                                                                                            new(NameArray("Spitzenplatz"), new { y = 3 }),
                                                                                                                                            new(NameArray("Kräuterbeet"), new { y = 5 }),
                                                                                                                                            new(NameArray("Muschelstrand"), new { y = 3 }),
                                                                                                                                            new(NameArray("Oldenburg"), new { y = 3 }),
                                                                                                                                            new(NameArray("Fernsehgarten"), new { y = 4 }),
                                                                                                                                            new(NameArray("Ananasrhabarbersalat"), new { y = 8 }),
                                                                                                                                            new(NameArray("Interkontinental"), new { y = 6 }),
                                                                                                                                            new(NameArray("Sonnenallee"), new { y = 5 }),
                                                                                                                                            new(NameArray("Gemüsebrühe"), new { y = 5 }),
                                                                                                                                            new(NameArray("Ölplattform"), new { y = 3 }),
                                                                                                                                            new(NameArray("Mandelbrot"), new { y = 3 }),
                                                                                                                                            new(NameArray("Sonderrecht"), new { y = 3 }),
                                                                                                                                            new(NameArray("Molekularbiologe"), new { y = 8 }),
                                                                                                                                            new(NameArray("Wetteransage"), new { y = 5 }),
                                                                                                                                            new(NameArray("Ananas"), new { y = 3 }),
                                                                                                                                            new(NameArray("Schonungslos"), new { y = 3 }),
                                                                                                                                            new(NameArray("Sauerkraut"), new { y = 5 }),
                                                                                                                                            new(NameArray("Loeffelbiscuit"), new { y = 6 }),
                                                                                                                                            new(NameArray("Pfeiffenblaeser"), new { y = 6 }),
                                                                                                                                            new(NameArray("Affenbrotbaum"), new { y = 5 }),
                                                                                                                                            new(NameArray("Illustrationsbuch"), new { y = 6 }),
                                                                                                                                            new(NameArray("Offenbarung"), new { y = 4 }),
                                                                                                                                            new(NameArray("Hausverkauf"), new { y = 5 }),
                                                                                                                                            new(NameArray("Lampenkranz"), new { y = 3 }),
                                                                                                                                            new(NameArray("Kostenstelle"), new { y = 4 }),
                                                                                                                                            new(NameArray("Oberfachverkaeufer"), new { y = 8 })
                                                                                                                                        ]),
                                                                 Runs = 100,
                                                                 TargetFitness = 0.01f,
                                                                 AfterRun = (index, fitness) => {
                                                                                if ((index & 511) == 0)
                                                                                    Console.WriteLine("{0}: {1}", index, fitness);
                                                                            },
                                                                 Threads = 2
                                                             };
        population.Train(setup);
        
        foreach(PopulationEntry<DynamicFFConfiguration> entry in population.Entries)
        foreach (NeuronConfig input in entry.Chromosome.Neurons.Take(entry.Chromosome.InputCount)) {
            input.Activation = 0;
            input.Aggregate = 0;
        }
        
        string original = Json.WriteString(population.Entries);
        MemoryStream output = new();
        AiSerialization.Serialize(population, output);

        Population<DynamicFFConfiguration> deserializedPopulation = new(AiSerialization.Deserialize<DynamicFFConfiguration>(new MemoryStream(output.ToArray())).ToArray(), null);
        string deserializedJson = Json.WriteString(deserializedPopulation.Entries);

        Assert.That(deserializedJson, Is.EqualTo(original));
    }

}