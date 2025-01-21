using System.Collections.Concurrent;
using Pooshit.Ai.Genetics;
using Pooshit.Ai.Net;
using Pooshit.Ai.Net.DynamicBO;
using Pooshit.Ai.Net.DynamicFF;
using Pooshit.Ai.Net.Evaluation;
using Pooshit.Ai.Neurons;
using Pooshit.Json;
using Pooshit.Scripting;
using Pooshit.Scripting.Expressions;
using Pooshit.Scripting.Parser;

namespace NightlyCode.Ai.Tests;

[TestFixture, Parallelizable]
public class CalculatorTests {

    public IScriptParser CreateParser() {
        ScriptParser parser = new();
        parser.Extensions.AddExtensions(typeof(Math));
        return parser;
    }
    
    /*[Test, Parallelizable]
    public void SquareRootNeuronalOperation() {
        NeuronalOperationNetConfiguration configuration = new(["x"], ["result"], 3, 3);
        NeuronalOperationNet net = new(configuration);

        float Test(NeuronalOperationNetConfiguration config, float input, float expected) {
            net.Update(config);
            net.Input["x"] = input;
            net.Compute();
            float result = net.Output["result"];
            return Math.Abs(expected - result);
        }

        Population<NeuronalOperationNetConfiguration> population = new(100, _ => new(["x"], ["result"], 3, 3));
        EvolutionSetup<NeuronalOperationNetConfiguration> setup = new() {
                                                                            TrainingSet = [
                                                                                              config => Test(config, 1, 1), config => Test(config, 4, 2), config => Test(config, 9, 3),
                                                                                              config => Test(config, 16, 4), config => Test(config, 25, 5), config => Test(config, 36, 6),
                                                                                              config => Test(config, 49, 7), config => Test(config, 64, 8), config => Test(config, 81, 9),
                                                                                              config => Test(config, 100, 10), config=>Test(config, 121, 11), config=>Test(config, 144, 12)
                                                                                          ],
                                                                            Runs = 1000,
                                                                            //Threads = Environment.ProcessorCount
                                                                            /*Acceptable = config => {
                                                                                             net.UpdateWeights(config.Operations);
                                                                                             net.Input["x"] = 16;
                                                                                             net.Compute();
                                                                                             double value = net.Output["result"];
                                                                                             return false;
                                                                                         }
                                                                        };
        PopulationEntry<NeuronalOperationNetConfiguration> result=population.Train(setup);
        Console.WriteLine($"Fitness: {result.Fitness:F2}");
        net.Update(result.Chromosome);
        for (int i = 1; i < 20; ++i) {
            net.Input["x"] = i;
            net.Compute();  
            Console.WriteLine($"sqrt({i})={net.Output["result"]}");
        }
        net.Input["x"] = 422933.0f;
        net.Compute();
        Console.WriteLine($"sqrt(422933)={net.Output["result"]}");

        //Console.WriteLine(Json.Json.WriteString(result.Item1.ExportDictionary()));
    }*/

    /*[Test, Parallelizable]
    public void SquareRootFeedForward() {
        FeedForwardConfiguration configuration = new(["x"], ["result"], 3, 3);
        FeedForwardNet net = new(configuration);

        float Test(FeedForwardConfiguration config, float input, float expected) {
            net.Update(config);
            net.Input["x"] = input;
            net.Compute();
            float result = net.Output["result"];
            return Math.Abs(expected - result);
        }

        Population<FeedForwardConfiguration> population = new(100, _ => new(["x"], ["result"], 3, 3));
        EvolutionSetup<FeedForwardConfiguration> setup = new() {
                                                                   TrainingSet = [
                                                                                     config => Test(config, 1, 1), config => Test(config, 4, 2), config => Test(config, 9, 3),
                                                                                     config => Test(config, 16, 4), config => Test(config, 25, 5), config => Test(config, 36, 6),
                                                                                     config => Test(config, 49, 7), config => Test(config, 64, 8), config => Test(config, 81, 9),
                                                                                     config => Test(config, 100, 10), config=>Test(config, 121, 11), config=>Test(config, 144, 12)
                                                                                 ],
                                                                   Runs = 1000,
                                                                   //Threads = Environment.ProcessorCount
                                                                   /*Acceptable = config => {
                                                                                    net.UpdateWeights(config.Operations);
                                                                                    net.Input["x"] = 16;
                                                                                    net.Compute();
                                                                                    double value = net.Output["result"];
                                                                                    return false;
                                                                                }
                                                               };
        PopulationEntry<FeedForwardConfiguration> result=population.Train(setup);
        Console.WriteLine($"Fitness: {result.Fitness:F2}");
        net.Update(result.Chromosome);
        for (int i = 1; i < 20; ++i) {
            net.Input["x"] = i;
            net.Compute();  
            Console.WriteLine($"sqrt({i})={net.Output["result"]}");
        }
        net.Input["x"] = 422933.0f;
        net.Compute();
        Console.WriteLine($"sqrt(422933)={net.Output["result"]}");
        Console.WriteLine(Json.Json.WriteString(result.Chromosome));
        //Console.WriteLine(Json.Json.WriteString(result.Item1.ExportDictionary()));
    }*/
    
    /*[Test, Parallelizable]
    public void MultiplyMinusNeuronalOperation() {
        NeuronalOperationNetConfiguration configuration = new(["x", "y", "z"], ["result"], 3, 3);
        NeuronalOperationNet net = new(configuration);

        float Test(NeuronalOperationNetConfiguration config, float x, float y, float z, float expected) {
            net.Update(config);
            net.Input["x"] = x;
            net.Input["y"] = y;
            net.Input["z"] = z;
            net.Compute();
            float result = net.Output["result"];
            return Math.Abs(expected - result);
        }

        Population<NeuronalOperationNetConfiguration> population = new(100, _ => new(["x", "y", "z"], ["result"], 3, 3));
        EvolutionSetup<NeuronalOperationNetConfiguration> setup = new() {
                                                                            TrainingSet = [
                                                                                              config => Test(config, 5, 2, 7, 3),
                                                                                              config => Test(config, 3, 3, 3, 6),
                                                                                              config => Test(config, 10, 10, 2, 98),
                                                                                              config => Test(config, 5, 5, 1, 24),
                                                                                              config => Test(config, 1, 40, 9, 31),
                                                                                              config => Test(config, 6, 10, 10, 50),
                                                                                              config => Test(config, 7, 8, 6, 50),
                                                                                              config => Test(config, 11, 8, 6, 82),
                                                                                              config => Test(config, 2, 70, 12, 128),
                                                                                              config => Test(config, 12, 12, 4, 140),
                                                                                              config => Test(config, 9, 12, 19, 89),
                                                                                              config => Test(config, 1, 2, 3, -1),
                                                                                              config => Test(config, 8, 3, 8, 16),
                                                                                              config => Test(config, 2, 34, 9, 59),
                                                                                              config => Test(config, 8, 66, 3, 525),
                                                                                              config => Test(config, 20, 6, 333, -213),
                                                                                              config => Test(config, 4, 60, 399, -159),
                                                                                              config => Test(config, 7, 18, 170, -49),
                                                                                              config => Test(config, -3, 7, 20, -41),
                                                                                              config => Test(config, -3, 8, 20, -44),
                                                                                              config => Test(config, -3, -8, 20, 4)
                                                                                          ],
                                                                            Runs = 5000,
                                                                            //Threads = Environment.ProcessorCount
                                                                            /*Acceptable = config => {
                                                                                             net.UpdateWeights(config.Operations);
                                                                                             net.Input["x"] = 16;
                                                                                             net.Compute();
                                                                                             double value = net.Output["result"];
                                                                                             return false;
                                                                                         }
                                                                        };
        PopulationEntry<NeuronalOperationNetConfiguration> result=population.Train(setup);
        Console.WriteLine($"Fitness: {result.Fitness:F2}");
        net.Update(result.Chromosome);
        net.Input["x"] = -3;
        net.Input["y"] = 7;
        net.Input["z"] = 20;
        net.Compute();
        Console.WriteLine($"f(-3,7,20)={net.Output["result"]}");

        //Console.WriteLine(Json.Json.WriteString(result.Item1.ExportDictionary()));
    }*/
    
    /*[Test, Parallelizable]
    public void MultiplyMinusFeedForward() {
        FeedForwardConfiguration configuration = new(["x", "y", "z"], ["result"], 3, 3, new() {
                                                                                                  OperationTypes = new()
                                                                                              });
        FeedForwardNet net = new(configuration);

        float Test(FeedForwardConfiguration config, float x, float y, float z, float expected) {
            net.Update(config);
            net.Input["x"] = x;
            net.Input["y"] = y;
            net.Input["z"] = z;
            net.Compute();
            float result = net.Output["result"];
            return Math.Abs(expected - result);
        }

        Population<FeedForwardConfiguration> population = new(100, _ => new(["x", "y", "z"], ["result"], 3, 3));
        EvolutionSetup<FeedForwardConfiguration> setup = new() {
                                                                   TrainingSet = [
                                                                                     config => Test(config, 5, 2, 7, 3),
                                                                                     config => Test(config, 3, 3, 3, 6),
                                                                                     config => Test(config, 10, 10, 2, 98),
                                                                                     config => Test(config, 5, 5, 1, 24),
                                                                                     config => Test(config, 1, 40, 9, 31),
                                                                                     config => Test(config, 6, 10, 10, 50),
                                                                                     config => Test(config, 7, 8, 6, 50),
                                                                                     config => Test(config, 11, 8, 6, 82),
                                                                                     config => Test(config, 2, 70, 12, 128),
                                                                                     config => Test(config, 12, 12, 4, 140),
                                                                                     config => Test(config, 9, 12, 19, 89),
                                                                                     config => Test(config, 1, 2, 3, -1),
                                                                                     config => Test(config, 8, 3, 8, 16),
                                                                                     config => Test(config, 2, 34, 9, 59),
                                                                                     config => Test(config, 8, 66, 3, 525),
                                                                                     config => Test(config, 20, 6, 333, -213),
                                                                                     config => Test(config, 4, 60, 399, -159),
                                                                                     config => Test(config, 7, 18, 170, -49),
                                                                                     config => Test(config, -3, 7, 20, -41),
                                                                                     config => Test(config, -3, 8, 20, -44),
                                                                                     config => Test(config, -3, -8, 20, 4)
                                                                                 ],
                                                                   Runs = 5000,
                                                               };
        PopulationEntry<FeedForwardConfiguration> result=population.Train(setup);
        Console.WriteLine($"Fitness: {result.Fitness:F2}");
        net.Update(result.Chromosome);
        net.Input["x"] = -3;
        net.Input["y"] = 7;
        net.Input["z"] = 20;
        net.Compute();
        Console.WriteLine($"f(-3,7,20)={net.Output["result"]}");

        //Console.WriteLine(Json.Json.WriteString(result.Item1.ExportDictionary()));
    }*/
    
    [Test, Parallelizable]
    public void MultiplyMinusDynamicBinOp() {
        Population<DynamicBOConfiguration> population = new(100, rng => new(["x", "y", "z"], ["result"], rng));
        EvolutionSetup<DynamicBOConfiguration> setup = new() {
            Evaluator = new SamplesEvaluator<DynamicBOConfiguration, DynamicBONet>([
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
            ]),
            Runs = 500,
            Rivalism = 5,
            AfterRun = (index, fitness) => {
                if ((index & 511) == 0)
                    Console.WriteLine("{0}: {1}", index, fitness);
            },
            Threads = 2
        };
        PopulationEntry<DynamicBOConfiguration> result=population.Train(setup);
        Console.WriteLine($"Fitness with {setup.Threads} threads: {result.Fitness:F2}");

        DynamicBONet net = new(result.Chromosome) {
            ["x"] = 5,
            ["y"] = 13,
            ["z"] = 44
        };

        net.Compute();
        Console.WriteLine($"f(5,13,44)={net["result"]}");

        Console.WriteLine(result.Chromosome);
    }
    
    [Test, Parallelizable]
    public void ExcellenceDynamicBinOp() {
        Dictionary<string, object> samples = Json.Read<Dictionary<string, object>>(File.ReadAllText("Data/excellence_samples.json"));
        
        Population<DynamicBOConfiguration> population = new(100, rng => new(new NeuronSpec[]{"visits", "appclicks", "applications", "profit", new("vcr", "net[\"appclicks\"]/net[\"visits\"].max(1.0f).max(net[\"appclicks\"])") , new("ccr", "net[\"applications\"]/net[\"appclicks\"].max(1.0f).max(net[\"applications\"])"), new("ppa", "net[\"profit\"]/net[\"applications\"].max(1.0f)")}, ["excellence"], rng));
        TrainingSample[] trainingSamples = JPath.Select<object[]>(samples, "samples")
                                                .Select(s => new TrainingSample(new {
                                                    visits = JPath.Select<float>(s, "inputs/visits"),
                                                    appclicks = JPath.Select<float>(s, "inputs/appclicks"),
                                                    applications = JPath.Select<float>(s, "inputs/applications"),
                                                    profit = JPath.Select<float>(s, "inputs/profit")
                                                }, new {
                                                    excellence = JPath.Select<float>(s, "outputs/excellence")
                                                })).ToArray();

        //Console.WriteLine(Json.WriteString(trainingSamples, JsonOptions.RestApi));

        IScriptParser parser = CreateParser();
        Tuple<int, Func<DynamicBONet, float>>[] scripts = null;

        IEnumerable<Tuple<int, Func<DynamicBONet, float>>> ScriptBuilder(DynamicBOConfiguration config) {
            foreach (NeuronConfig neuronConfig in config.Neurons.Take(config.InputCount).Where(n => !string.IsNullOrEmpty(n.Generator)))
                yield return new(neuronConfig.Index, parser.ParseDelegate<Func<DynamicBONet, float>>(neuronConfig.Generator, new LambdaParameter("net", typeof(DynamicBONet))));
        }

        EvolutionSetup<DynamicBOConfiguration> setup = new()
        {
            Evaluator = new SamplesEvaluator<DynamicBOConfiguration, DynamicBONet>(trainingSamples) {
                InputGenerator = (net, config) => {
                    scripts ??= ScriptBuilder(config).ToArray();
                    foreach ((int neuron, Func<DynamicBONet, float> generator) in scripts)
                        net[neuron] = generator(net);
                }
            },
            Runs = 500,
            Rivalism = 5,
            AfterRun = (index, fitness) =>
            {
                if ((index & 511) == 0)
                    Console.WriteLine("{0}: {1}", index, fitness);
            },
            Threads = Environment.ProcessorCount
        };

        PopulationEntry<DynamicBOConfiguration> result = population.Train(setup);
        Console.WriteLine($"Fitness: {result.Fitness:F2}"); 
        DynamicBONet net = new(result.Chromosome) {
            ["visits"] = 144,
            ["appclicks"] = 4,
            ["applications"] = 1,
            ["profit"] = 5
        };

        net.Compute();
        Console.WriteLine($"Excellence {net["excellence"]}");

        Console.WriteLine(result.Chromosome);
    }
    
    [Test, Parallelizable]
    public void SequenceDynamicBinOp() {
        Population<DynamicBOConfiguration> population = new(100, rng => new(new[]{"x"}, ["y"], rng));
        EvolutionSetup<DynamicBOConfiguration> setup = new()
        {
            Evaluator = new SamplesEvaluator<DynamicBOConfiguration, DynamicBONet>([
                new(new { x = 1 }, new { y = 2 }),
                new(new { x = 2 }, new { y = 6 }),
                new(new { x = 3 }, new { y = 12 }),
                new(new { x = 4 }, new { y = 20 }),
                new(new { x = 5 }, new { y = 30 }),
                new(new { x = 6 }, new { y = 42 }),
                new(new { x = 7 }, new { y = 56 }),
                new(new { x = 8 }, new { y = 72 }),
                new(new { x = 9 }, new { y = 90 }),
                new(new { x = 10 }, new { y = 110 }),
            ]),
            Runs = 500,
            Rivalism = 5,
            AfterRun = (index, fitness) =>
            {
                if ((index & 511) == 0)
                    Console.WriteLine("{0}: {1}", index, fitness);
            },
            Threads = 2
        };
        PopulationEntry<DynamicBOConfiguration> result=population.Train(setup);
        Console.WriteLine($"Fitness: {result.Fitness:F2}");
        DynamicBONet net = new(result.Chromosome)
        {
            ["x"] = 20
        };

        net.Compute();
        Console.WriteLine($"{net["y"]}");

        Console.WriteLine(result.Chromosome);
        Console.WriteLine(Json.WriteString(result.Chromosome));
    }

    [Test, Parallelizable]
    public void PredictExcellenceMonth() {
        Population<DynamicBOConfiguration> population = new(100, rng => new(12, ["excellence"], rng));
        EvolutionSetup<DynamicBOConfiguration> setup = new()
        {
            Evaluator = new SamplesEvaluator<DynamicBOConfiguration, DynamicBONet>([
                new([-0.70377576f, -1.0478438f, -0.44907594f, -1.2090507f, -2.3576488f, -2.741547f, -4.995035f, 1.510059f, -0.94160247f, -0.6352442f, -0.86448616f, 4.0936418f], new { excellence = 0.03903519f }),
                new([-1.0478438f, -0.44907594f, -1.2090507f, -2.3576488f, -2.741547f, -4.995035f, 1.510059f, -0.94160247f, -0.6352442f, -0.86448616f, 4.0936418f, 0.03903519f], new { excellence = 0.97822535f }),
                new([-0.44907594f, -1.2090507f, -2.3576488f, -2.741547f, -4.995035f, 1.510059f, -0.94160247f, -0.6352442f, -0.86448616f, 4.0936418f, 0.03903519f, 0.97822535f], new { excellence = -0.0025440229f }),
                new([-1.2090507f, -2.3576488f, -2.741547f, -4.995035f, 1.510059f, -0.94160247f, -0.6352442f, -0.86448616f, 4.0936418f, 0.03903519f, 0.97822535f, -0.0025440229f], new { excellence = 1.5184832f }),
                new([-2.3576488f, -2.741547f, -4.995035f, 1.510059f, -0.94160247f, -0.6352442f, -0.86448616f, 4.0936418f, 0.03903519f, 0.97822535f, -0.0025440229f, 1.5184832f], new { excellence = 1.1404086f }),
                new([-2.741547f, -4.995035f, 1.510059f, -0.94160247f, -0.6352442f, -0.86448616f, 4.0936418f, 0.03903519f, 0.97822535f, -0.0025440229f, 1.5184832f, 1.1404086f], new { excellence = 1.1211936f }),
                new([-4.995035f, 1.510059f, -0.94160247f, -0.6352442f, -0.86448616f, 4.0936418f, 0.03903519f, 0.97822535f, -0.0025440229f, 1.5184832f, 1.1404086f, 1.1211936f], new { excellence = 1.1998706f }),
                new([1.510059f, -0.94160247f, -0.6352442f, -0.86448616f, 4.0936418f, 0.03903519f, 0.97822535f, -0.0025440229f, 1.5184832f, 1.1404086f, 1.1211936f, 1.1998706f], new { excellence = 1.1294295f }),
                new([-0.94160247f, -0.6352442f, -0.86448616f, 4.0936418f, 0.03903519f, 0.97822535f, -0.0025440229f, 1.5184832f, 1.1404086f, 1.1211936f, 1.1998706f, 1.1294295f], new { excellence = 2.6707275f }),
                new([-0.6352442f, -0.86448616f, 4.0936418f, 0.03903519f, 0.97822535f, -0.0025440229f, 1.5184832f, 1.1404086f, 1.1211936f, 1.1998706f, 1.1294295f, 2.6707275f], new { excellence = 3.9283998f }),
                new([-0.86448616f, 4.0936418f, 0.03903519f, 0.97822535f, -0.0025440229f, 1.5184832f, 1.1404086f, 1.1211936f, 1.1998706f, 1.1294295f, 2.6707275f, 3.9283998f], new { excellence = 0.6945998f }),
                new([4.0936418f, 0.03903519f, 0.97822535f, -0.0025440229f, 1.5184832f, 1.1404086f, 1.1211936f, 1.1998706f, 1.1294295f, 2.6707275f, 3.9283998f, 0.6945998f], new { excellence = 2.5514934f }),
                new([0.03903519f, 0.97822535f, -0.0025440229f, 1.5184832f, 1.1404086f, 1.1211936f, 1.1998706f, 1.1294295f, 2.6707275f, 3.9283998f, 0.6945998f, 2.5514934f], new { excellence = 2.4648762f }),
                new([0.97822535f, -0.0025440229f, 1.5184832f, 1.1404086f, 1.1211936f, 1.1998706f, 1.1294295f, 2.6707275f, 3.9283998f, 0.6945998f, 2.5514934f, 2.4648762f], new { excellence = 5.7177305f }),
                new([-0.0025440229f, 1.5184832f, 1.1404086f, 1.1211936f, 1.1998706f, 1.1294295f, 2.6707275f, 3.9283998f, 0.6945998f, 2.5514934f, 2.4648762f, 5.7177305f], new { excellence = 2.4198537f }),
                new([1.5184832f, 1.1404086f, 1.1211936f, 1.1998706f, 1.1294295f, 2.6707275f, 3.9283998f, 0.6945998f, 2.5514934f, 2.4648762f, 5.7177305f, 2.4198537f], new { excellence = 1.1340914f }),
                new([1.1404086f, 1.1211936f, 1.1998706f, 1.1294295f, 2.6707275f, 3.9283998f, 0.6945998f, 2.5514934f, 2.4648762f, 5.7177305f, 2.4198537f, 1.1340914f], new { excellence = 1.5012846f }),
                new([1.1211936f, 1.1998706f, 1.1294295f, 2.6707275f, 3.9283998f, 0.6945998f, 2.5514934f, 2.4648762f, 5.7177305f, 2.4198537f, 1.1340914f, 1.5012846f], new { excellence = 2.3335252f }),
                new([1.1998706f, 1.1294295f, 2.6707275f, 3.9283998f, 0.6945998f, 2.5514934f, 2.4648762f, 5.7177305f, 2.4198537f, 1.1340914f, 1.5012846f, 2.3335252f], new { excellence = 2.2889094f }),
                new([1.1294295f, 2.6707275f, 3.9283998f, 0.6945998f, 2.5514934f, 2.4648762f, 5.7177305f, 2.4198537f, 1.1340914f, 1.5012846f, 2.3335252f, 2.2889094f], new { excellence = 2.2817314f }),
                new([2.6707275f, 3.9283998f, 0.6945998f, 2.5514934f, 2.4648762f, 5.7177305f, 2.4198537f, 1.1340914f, 1.5012846f, 2.3335252f, 2.2889094f, 2.2817314f], new { excellence = 1.420998f }),
                new([3.9283998f, 0.6945998f, 2.5514934f, 2.4648762f, 5.7177305f, 2.4198537f, 1.1340914f, 1.5012846f, 2.3335252f, 2.2889094f, 2.2817314f, 1.420998f], new { excellence = 1.2097629f }),
                new([0.6945998f, 2.5514934f, 2.4648762f, 5.7177305f, 2.4198537f, 1.1340914f, 1.5012846f, 2.3335252f, 2.2889094f, 2.2817314f, 1.420998f, 1.2097629f], new { excellence = 2.051351f }),
                new([2.5514934f, 2.4648762f, 5.7177305f, 2.4198537f, 1.1340914f, 1.5012846f, 2.3335252f, 2.2889094f, 2.2817314f, 1.420998f, 1.2097629f, 2.051351f], new { excellence = 1.8794844f }),
                new([2.4648762f, 5.7177305f, 2.4198537f, 1.1340914f, 1.5012846f, 2.3335252f, 2.2889094f, 2.2817314f, 1.420998f, 1.2097629f, 2.051351f, 1.8794844f], new { excellence = 2.2029688f }),
                new([5.7177305f, 2.4198537f, 1.1340914f, 1.5012846f, 2.3335252f, 2.2889094f, 2.2817314f, 1.420998f, 1.2097629f, 2.051351f, 1.8794844f, 2.2029688f], new { excellence = 2.4636724f }),
                new([2.4198537f, 1.1340914f, 1.5012846f, 2.3335252f, 2.2889094f, 2.2817314f, 1.420998f, 1.2097629f, 2.051351f, 1.8794844f, 2.2029688f, 2.4636724f], new { excellence = 2.0925627f }),
                new([1.1340914f, 1.5012846f, 2.3335252f, 2.2889094f, 2.2817314f, 1.420998f, 1.2097629f, 2.051351f, 1.8794844f, 2.2029688f, 2.4636724f, 2.0925627f], new { excellence = 1.0691693f }),
                new([1.5012846f, 2.3335252f, 2.2889094f, 2.2817314f, 1.420998f, 1.2097629f, 2.051351f, 1.8794844f, 2.2029688f, 2.4636724f, 2.0925627f, 1.0691693f], new { excellence = 4.2139206f }),
                new([2.3335252f, 2.2889094f, 2.2817314f, 1.420998f, 1.2097629f, 2.051351f, 1.8794844f, 2.2029688f, 2.4636724f, 2.0925627f, 1.0691693f, 4.2139206f], new { excellence = 2.9854026f }),
                new([2.2889094f, 2.2817314f, 1.420998f, 1.2097629f, 2.051351f, 1.8794844f, 2.2029688f, 2.4636724f, 2.0925627f, 1.0691693f, 4.2139206f, 2.9854026f], new { excellence = 1.4749887f }),
                new([2.2817314f, 1.420998f, 1.2097629f, 2.051351f, 1.8794844f, 2.2029688f, 2.4636724f, 2.0925627f, 1.0691693f, 4.2139206f, 2.9854026f, 1.4749887f], new { excellence = 1.3614614f }),
                new([1.420998f, 1.2097629f, 2.051351f, 1.8794844f, 2.2029688f, 2.4636724f, 2.0925627f, 1.0691693f, 4.2139206f, 2.9854026f, 1.4749887f, 1.3614614f], new { excellence = 2.91232f }),
                new([1.2097629f, 2.051351f, 1.8794844f, 2.2029688f, 2.4636724f, 2.0925627f, 1.0691693f, 4.2139206f, 2.9854026f, 1.4749887f, 1.3614614f, 2.91232f], new { excellence = 1.9441458f }),
                new([2.051351f, 1.8794844f, 2.2029688f, 2.4636724f, 2.0925627f, 1.0691693f, 4.2139206f, 2.9854026f, 1.4749887f, 1.3614614f, 2.91232f, 1.9441458f], new { excellence = 2.702325f }),
                new([1.8794844f, 2.2029688f, 2.4636724f, 2.0925627f, 1.0691693f, 4.2139206f, 2.9854026f, 1.4749887f, 1.3614614f, 2.91232f, 1.9441458f, 2.702325f], new { excellence = 2.4657698f }),
            ]),
            Runs = 5000,
            AfterRun = (index, fitness) =>
            {
                if ((index & 511) == 0)
                    Console.WriteLine("{0}: {1}", index, fitness);
            },
            Threads = 2
        };
        PopulationEntry<DynamicBOConfiguration> result = population.Train(setup);
        Console.WriteLine($"Fitness: {result.Fitness:F2}");
        DynamicBONet net = new(result.Chromosome);
        net.SetInputValues([2.2029688f, 2.4636724f, 2.0925627f, 1.0691693f, 4.2139206f, 2.9854026f, 1.4749887f, 1.3614614f, 2.91232f, 1.9441458f, 2.702325f, 2.4657698f]);
        net.Compute();
        Console.WriteLine($"{net["excellence"]}");

        Console.WriteLine(result.Chromosome);
        Console.WriteLine(Json.WriteString(result.Chromosome));
    }

    [Test, Parallelizable]
    public void PredictTagExcellenceMonth() {
        Population<DynamicBOConfiguration> population = new(100, rng => new(13, ["excellence"], rng));
        EvolutionSetup<DynamicBOConfiguration> setup = new()
        {
            Evaluator = new SamplesEvaluator<DynamicBOConfiguration, DynamicBONet>([
                new([-2.550804f, -2.947061f, -3.1435318f, -4.0194483f, -4.286634f, -4.995035f, -4.995035f, -1.3816023f, -0.30745876f, -2.5069754f, -2.7969193f, 2.5990822f, -0.8763627f], new { excellence = 0.544427f }),
                new([-2.947061f, -3.1435318f, -4.0194483f, -4.286634f, -4.995035f, -4.995035f, -1.3816023f, -0.30745876f, -2.5069754f, -2.7969193f, 2.5990822f, -0.8763627f, 0.544427f], new { excellence = 10.636712f }),
                new([-3.1435318f, -4.0194483f, -4.286634f, -4.995035f, -4.995035f, -1.3816023f, -0.30745876f, -2.5069754f, -2.7969193f, 2.5990822f, -0.8763627f, 0.544427f, 10.636712f], new { excellence = 2.1540642f }),
                new([-4.0194483f, -4.286634f, -4.995035f, -4.995035f, -1.3816023f, -0.30745876f, -2.5069754f, -2.7969193f, 2.5990822f, -0.8763627f, 0.544427f, 10.636712f, 2.1540642f], new { excellence = 2.5721974f }),
                new([-4.286634f, -4.995035f, -4.995035f, -1.3816023f, -0.30745876f, -2.5069754f, -2.7969193f, 2.5990822f, -0.8763627f, 0.544427f, 10.636712f, 2.1540642f, 2.5721974f], new { excellence = 1.8246433f }),
                new([-4.995035f, -4.995035f, -1.3816023f, -0.30745876f, -2.5069754f, -2.7969193f, 2.5990822f, -0.8763627f, 0.544427f, 10.636712f, 2.1540642f, 2.5721974f, 1.8246433f], new { excellence = 1.0785362f }),
                new([-4.995035f, -1.3816023f, -0.30745876f, -2.5069754f, -2.7969193f, 2.5990822f, -0.8763627f, 0.544427f, 10.636712f, 2.1540642f, 2.5721974f, 1.8246433f, 1.0785362f], new { excellence = 1.3644333f }),
                new([-1.3816023f, -0.30745876f, -2.5069754f, -2.7969193f, 2.5990822f, -0.8763627f, 0.544427f, 10.636712f, 2.1540642f, 2.5721974f, 1.8246433f, 1.0785362f, 1.3644333f], new { excellence = 2.3525946f }),
                new([-0.30745876f, -2.5069754f, -2.7969193f, 2.5990822f, -0.8763627f, 0.544427f, 10.636712f, 2.1540642f, 2.5721974f, 1.8246433f, 1.0785362f, 1.3644333f, 2.3525946f], new { excellence = 1.3881122f }),
                new([-2.5069754f, -2.7969193f, 2.5990822f, -0.8763627f, 0.544427f, 10.636712f, 2.1540642f, 2.5721974f, 1.8246433f, 1.0785362f, 1.3644333f, 2.3525946f, 1.3881122f], new { excellence = 1.1955884f }),
                new([-2.7969193f, 2.5990822f, -0.8763627f, 0.544427f, 10.636712f, 2.1540642f, 2.5721974f, 1.8246433f, 1.0785362f, 1.3644333f, 2.3525946f, 1.3881122f, 1.1955884f], new { excellence = 2.2566073f }),
                new([2.5990822f, -0.8763627f, 0.544427f, 10.636712f, 2.1540642f, 2.5721974f, 1.8246433f, 1.0785362f, 1.3644333f, 2.3525946f, 1.3881122f, 1.1955884f, 2.2566073f], new { excellence = 1.8456446f }),
                new([-0.8763627f, 0.544427f, 10.636712f, 2.1540642f, 2.5721974f, 1.8246433f, 1.0785362f, 1.3644333f, 2.3525946f, 1.3881122f, 1.1955884f, 2.2566073f, 1.8456446f], new { excellence = 1.3207263f }),
                new([0.544427f, 10.636712f, 2.1540642f, 2.5721974f, 1.8246433f, 1.0785362f, 1.3644333f, 2.3525946f, 1.3881122f, 1.1955884f, 2.2566073f, 1.8456446f, 1.3207263f], new { excellence = 1.353026f }),
                new([10.636712f, 2.1540642f, 2.5721974f, 1.8246433f, 1.0785362f, 1.3644333f, 2.3525946f, 1.3881122f, 1.1955884f, 2.2566073f, 1.8456446f, 1.3207263f, 1.353026f], new { excellence = 1.3906026f }),
                new([2.1540642f, 2.5721974f, 1.8246433f, 1.0785362f, 1.3644333f, 2.3525946f, 1.3881122f, 1.1955884f, 2.2566073f, 1.8456446f, 1.3207263f, 1.353026f, 1.3906026f], new { excellence = 1.0099341f }),
                new([2.5721974f, 1.8246433f, 1.0785362f, 1.3644333f, 2.3525946f, 1.3881122f, 1.1955884f, 2.2566073f, 1.8456446f, 1.3207263f, 1.353026f, 1.3906026f, 1.0099341f], new { excellence = 1.719673f }),
                new([1.8246433f, 1.0785362f, 1.3644333f, 2.3525946f, 1.3881122f, 1.1955884f, 2.2566073f, 1.8456446f, 1.3207263f, 1.353026f, 1.3906026f, 1.0099341f, 1.719673f], new { excellence = 2.0159636f }),
                new([1.0785362f, 1.3644333f, 2.3525946f, 1.3881122f, 1.1955884f, 2.2566073f, 1.8456446f, 1.3207263f, 1.353026f, 1.3906026f, 1.0099341f, 1.719673f, 2.0159636f], new { excellence = 1.5613949f }),
                new([1.3644333f, 2.3525946f, 1.3881122f, 1.1955884f, 2.2566073f, 1.8456446f, 1.3207263f, 1.353026f, 1.3906026f, 1.0099341f, 1.719673f, 2.0159636f, 1.5613949f], new { excellence = 1.6830198f }),
                new([2.3525946f, 1.3881122f, 1.1955884f, 2.2566073f, 1.8456446f, 1.3207263f, 1.353026f, 1.3906026f, 1.0099341f, 1.719673f, 2.0159636f, 1.5613949f, 1.6830198f], new { excellence = 1.3998297f }),
                new([1.3881122f, 1.1955884f, 2.2566073f, 1.8456446f, 1.3207263f, 1.353026f, 1.3906026f, 1.0099341f, 1.719673f, 2.0159636f, 1.5613949f, 1.6830198f, 1.3998297f], new { excellence = 0.8084549f }),
                new([1.1955884f, 2.2566073f, 1.8456446f, 1.3207263f, 1.353026f, 1.3906026f, 1.0099341f, 1.719673f, 2.0159636f, 1.5613949f, 1.6830198f, 1.3998297f, 0.8084549f], new { excellence = 0.8410222f }),
                new([2.2566073f, 1.8456446f, 1.3207263f, 1.353026f, 1.3906026f, 1.0099341f, 1.719673f, 2.0159636f, 1.5613949f, 1.6830198f, 1.3998297f, 0.8084549f, 0.8410222f], new { excellence = 1.0605899f }),
                new([1.8456446f, 1.3207263f, 1.353026f, 1.3906026f, 1.0099341f, 1.719673f, 2.0159636f, 1.5613949f, 1.6830198f, 1.3998297f, 0.8084549f, 0.8410222f, 1.0605899f], new { excellence = 1.5191834f }),
                new([1.3207263f, 1.353026f, 1.3906026f, 1.0099341f, 1.719673f, 2.0159636f, 1.5613949f, 1.6830198f, 1.3998297f, 0.8084549f, 0.8410222f, 1.0605899f, 1.5191834f], new { excellence = 2.448923f }),
                new([1.353026f, 1.3906026f, 1.0099341f, 1.719673f, 2.0159636f, 1.5613949f, 1.6830198f, 1.3998297f, 0.8084549f, 0.8410222f, 1.0605899f, 1.5191834f, 2.448923f], new { excellence = 1.8765916f }),
                new([1.3906026f, 1.0099341f, 1.719673f, 2.0159636f, 1.5613949f, 1.6830198f, 1.3998297f, 0.8084549f, 0.8410222f, 1.0605899f, 1.5191834f, 2.448923f, 1.8765916f], new { excellence = 2.7118263f }),
                new([1.0099341f, 1.719673f, 2.0159636f, 1.5613949f, 1.6830198f, 1.3998297f, 0.8084549f, 0.8410222f, 1.0605899f, 1.5191834f, 2.448923f, 1.8765916f, 2.7118263f], new { excellence = 1.7764813f }),
                new([1.719673f, 2.0159636f, 1.5613949f, 1.6830198f, 1.3998297f, 0.8084549f, 0.8410222f, 1.0605899f, 1.5191834f, 2.448923f, 1.8765916f, 2.7118263f, 1.7764813f], new { excellence = 0.94527507f }),
                new([2.0159636f, 1.5613949f, 1.6830198f, 1.3998297f, 0.8084549f, 0.8410222f, 1.0605899f, 1.5191834f, 2.448923f, 1.8765916f, 2.7118263f, 1.7764813f, 0.94527507f], new { excellence = 0.888548f }),
                new([1.5613949f, 1.6830198f, 1.3998297f, 0.8084549f, 0.8410222f, 1.0605899f, 1.5191834f, 2.448923f, 1.8765916f, 2.7118263f, 1.7764813f, 0.94527507f, 0.888548f], new { excellence = 2.6978912f }),
                new([1.6830198f, 1.3998297f, 0.8084549f, 0.8410222f, 1.0605899f, 1.5191834f, 2.448923f, 1.8765916f, 2.7118263f, 1.7764813f, 0.94527507f, 0.888548f, 2.6978912f], new { excellence = 1.8902876f }),
                new([1.3998297f, 0.8084549f, 0.8410222f, 1.0605899f, 1.5191834f, 2.448923f, 1.8765916f, 2.7118263f, 1.7764813f, 0.94527507f, 0.888548f, 2.6978912f, 1.8902876f], new { excellence = 2.5727115f }),
                new([0.8084549f, 0.8410222f, 1.0605899f, 1.5191834f, 2.448923f, 1.8765916f, 2.7118263f, 1.7764813f, 0.94527507f, 0.888548f, 2.6978912f, 1.8902876f, 2.5727115f], new { excellence = 2.2042959f })
            ]),
            Runs = 5000,
            AfterRun = (index, fitness) =>
            {
                if ((index & 511) == 0)
                    Console.WriteLine("{0}: {1}", index, fitness);
            },
            Threads = 2
        };
        PopulationEntry<DynamicBOConfiguration> result = population.Train(setup);
        Console.WriteLine($"Fitness: {result.Fitness:F2}");
        DynamicBONet net = new(result.Chromosome);
        net.SetInputValues([0.8410222f, 1.0605899f, 1.5191834f, 2.448923f, 1.8765916f, 2.7118263f, 1.7764813f, 0.94527507f, 0.888548f, 2.6978912f, 1.8902876f, 2.5727115f, 2.2042959f]);
        net.Compute();
        Console.WriteLine($"{net["excellence"]}");

        Console.WriteLine(result.Chromosome);
        Console.WriteLine(Json.WriteString(result.Chromosome));
    }

    [Test, Parallelizable]
    public void PredictProfitBinOp() {
        ConcurrentStack<DynamicBONet> netStack = new();
        
        Population<DynamicBOConfiguration> population = new(100, rng => new(new []{"year","month","budget"}, ["profit"], rng));
        EvolutionSetup<DynamicBOConfiguration> setup = new() {
            Evaluator = new SamplesEvaluator<DynamicBOConfiguration, DynamicBONet>([
                new(new { year = 4, month = 5, budget = 57615.586360627 }, new { profit = 23575.7377141578 }),
                new(new { year = 4, month = 6, budget = 60395.3696294009 }, new { profit = 19690.454525992 }),
                new(new { year = 4, month = 7, budget = 58837.037529195 }, new { profit = 20603.8622427126 }),
                new(new { year = 4, month = 8, budget = 57075.7820036814 }, new { profit = 18805.7086431028 }),
                new(new { year = 4, month = 9, budget = 66252.5692546513 }, new { profit = 29562.2027050053 }),
                new(new { year = 4, month = 10, budget = 68492.1329769176 }, new { profit = 32080.2357595928 }),
            ]),
            Runs = 5000,
            AfterRun = (index, fitness) => {
                if ((index & 511) == 0)
                    Console.WriteLine("{0}: {1}", index, fitness);
            },
            Threads = 2
        };
        PopulationEntry<DynamicBOConfiguration> result=population.Train(setup);
        Console.WriteLine($"Fitness: {result.Fitness:F2}");
        if (!netStack.TryPop(out DynamicBONet net))
            net = new(result.Chromosome);
        else net.Update(result.Chromosome);

        net["year"] = 4;
        net["month"] = 11;
        net["budget"] = 75040.2678569351f;
        net.Compute();
        Console.WriteLine($"{net["profit"]}");

        Console.WriteLine(result.Chromosome);
    }

    [Test, Parallelizable]
    public void SequenceDynamicFF() {
        Population<DynamicFFConfiguration> population = new(100, rng => new(new[]{"x"}, ["y"], rng));
        EvolutionSetup<DynamicFFConfiguration> setup = new() {
            Evaluator = new SamplesEvaluator<DynamicFFConfiguration, DynamicFFNet>([
                new(new { x = 1 }, new { y = 2 }),
                new(new { x = 2 }, new { y = 6 }),
                new(new { x = 3 }, new { y = 12 }),
                new(new { x = 4 }, new { y = 20 }),
                new(new { x = 5 }, new { y = 30 }),
                new(new { x = 6 }, new { y = 42 }),
                new(new { x = 7 }, new { y = 56 }),
                new(new { x = 8 }, new { y = 72 }),
                new(new { x = 9 }, new { y = 90 }),
                new(new { x = 10 }, new { y = 110 }),
            ]),
            Runs = 5000,
            AfterRun = (index, fitness) => {
                if ((index & 511) == 0)
                    Console.WriteLine("{0}: {1}", index, fitness);
            },
            Threads = 2
        };
        PopulationEntry<DynamicFFConfiguration> result=population.Train(setup);
        Console.WriteLine($"Fitness: {result.Fitness:F2}");

        DynamicFFNet net = new(result.Chromosome) {
                                                      ["x"] = 20
                                                  };
        net.Compute();
        Console.WriteLine($"{net["y"]}");

        Console.WriteLine(result.Chromosome);
    }

    [Test, Parallelizable]
    public void SequenceDynamicBinOpTwice() {
        Population<DynamicBOConfiguration> population = new(100, rng => new(["x"], ["y"], rng));
        EvolutionSetup<DynamicBOConfiguration> setup = new() {
            Evaluator = new SamplesEvaluator<DynamicBOConfiguration, DynamicBONet>([
                new(new { x = 1 }, new { y = 2 }),
                new(new { x = 2 }, new { y = 6 }),
                new(new { x = 3 }, new { y = 12 }),
                new(new { x = 4 }, new { y = 20 }),
                new(new { x = 5 }, new { y = 30 }),
                new(new { x = 6 }, new { y = 42 }),
                new(new { x = 7 }, new { y = 56 }),
                new(new { x = 8 }, new { y = 72 }),
                new(new { x = 9 }, new { y = 90 }),
                new(new { x = 10 }, new { y = 110 }),
            ]),
            Runs = 5000,
            TargetFitness = 0.01f,
            AfterRun = (index, fitness) => {
                if ((index & 511) == 0)
                    Console.WriteLine("{0}: {1}", index, fitness);
            },
            Threads = 2
        };
        PopulationEntry<DynamicBOConfiguration> result=population.Train(setup);
        Console.WriteLine($"Fitness: {result.Fitness:F2}");

        DynamicBONet resultNet = new(result.Chromosome) {
            ["x"] = 20
        };

        resultNet.Compute();
        Console.WriteLine($"{resultNet["y"]}");

        Console.WriteLine(result.Chromosome);
        
        result=population.Train(setup);
        Console.WriteLine($"Fitness: {result.Fitness:F2}");
        resultNet = new(result.Chromosome) {
            ["x"] = 20
        };

        resultNet.Compute();
        Console.WriteLine($"{resultNet["y"]}");

        Console.WriteLine(result.Chromosome);
    }
    
    [Test, Parallelizable]
    public void CountVowels() {

        float[] NameArray(string name) {
            float[] values = new float[20];
            for(int i=0;i<values.Length;++i)
                values[i] = i < name.Length ? (byte)name[i] : 0.0f;

            return values;
        }
        
        Population<DynamicBOConfiguration> population = new(100, rng => new(20, ["y"], rng));
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
                new(NameArray("Oberfachverkaeufer"), new { y = 8 }),
                new(NameArray("Semmelbroesel"), new { y = 5 })
            ]),
            Runs = 5000,
            TargetFitness = 0.01f,
            AfterRun = (index, fitness) => {
                if ((index & 511) == 0)
                    Console.WriteLine("{0}: {1}", index, fitness);
            },
            Threads = 2
        };
        PopulationEntry<DynamicBOConfiguration> result=population.Train(setup);
        Console.WriteLine($"Fitness: {result.Fitness:F2}");
        
        DynamicBONet resultNet = new(result.Chromosome);

        
        resultNet.SetInputValues(NameArray("Konfettikanone"));
        resultNet.Compute();
        Console.WriteLine($"{resultNet["y"]}");

        Console.WriteLine(result.Chromosome);
    }
    
    [Test, Parallelizable]
    public void CountVowelsFF() {

        float[] NameArray(string name) {
            name = name.ToLower();
            float[] values = new float[20];
            for(int i=0;i<values.Length;++i)
                values[i] = i < name.Length ? (byte)name[i] : 0.0f;

            return values;
        }
        
        Population<DynamicFFConfiguration> population = new(100, rng => new(20, ["y"], rng));
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
            Runs = 5000,
            TargetFitness = 0.01f,
            AfterRun = (index, fitness) => {
                if ((index & 511) == 0)
                    Console.WriteLine("{0}: {1}", index, fitness);
            },
            Threads = 2
        };
        PopulationEntry<DynamicFFConfiguration> result=population.Train(setup);
        Console.WriteLine($"Fitness: {result.Fitness:F2}");
        
        DynamicFFNet resultNet = new(result.Chromosome);

        
        resultNet.SetInputValues(NameArray("Semmelbroesel"));
        resultNet.Compute();
        Console.WriteLine($"{resultNet["y"]}");

        Console.WriteLine(result.Chromosome);
    }
    
    [Test, Parallelizable]
    public void DetermineGender() {

        float[] NameArray(string name) {
            name = name.ToLower();
            float[] values = new float[20];
            for(int i=0;i<values.Length;++i)
                values[i] = i < name.Length ? (byte)name[i] : 0.0f;

            return values;
        }
        
        Population<DynamicFFConfiguration> population = new(100, rng => new(20, ["male", "female", "object"], rng));
        EvolutionSetup<DynamicFFConfiguration> setup = new() {
            Evaluator = new SamplesEvaluator<DynamicFFConfiguration, DynamicFFNet>([
                new(NameArray("Matthias"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Ina"), new { male = 0, female=1, @object=0 }),
                new(NameArray("Monika"), new { male = 0, female=1, @object=0 }),
                new(NameArray("Heinz"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Ali"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Mohammed"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Jesus"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Theresa"), new { male = 0, female=1, @object=0 }),
                new(NameArray("Sandra"), new { male = 0, female=1, @object=0 }),
                new(NameArray("Brunhilde"), new { male = 0, female=1, @object=0 }),
                new(NameArray("Siegfried"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Gangolf"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Rolf"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Sieglinde"), new { male = 0, female=1, @object=0 }),
                new(NameArray("Tina"), new { male = 0, female=1, @object=0 }),
                new(NameArray("Matthilda"), new { male = 0, female=1, @object=0 }),
                new(NameArray("Hilda"), new { male = 0, female=1, @object=0 }),
                new(NameArray("Friedrich"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Gisela"), new { male = 0, female=1, @object=0 }),
                new(NameArray("Tom"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Lisa"), new { male = 0, female=1, @object=0 }),
                new(NameArray("Cheryl"), new { male = 0, female=1, @object=0 }),
                new(NameArray("Leo"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Martin"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Selene"), new { male = 0, female=1, @object=0 }),
                new(NameArray("Nathan"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Christopher"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Christian"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Kristin"), new { male = 0, female=1, @object=0 }),
                new(NameArray("Lucy"), new { male = 0, female=1, @object=0 }),
                new(NameArray("Cloud"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Kina"), new { male = 0, female=1, @object=0 }),
                new(NameArray("Timon"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Monika"), new { male = 0, female=1, @object=0 }),
                new(NameArray("Peter"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Oliver"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Idriss"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Katharina"), new { male = 0, female=1, @object=0 }),
                new(NameArray("Olga"), new { male = 0, female=1, @object=0 }),
                new(NameArray("Susanne"), new { male = 0, female=1, @object=0 }),
                new(NameArray("Susi"), new { male = 0, female=1, @object=0 }),
                new(NameArray("Horst"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Karl"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Mandy"), new { male = 0, female=1, @object=0 }),
                new(NameArray("Jörg"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Irene"), new { male = 0, female=1, @object=0 }),
                new(NameArray("Marco"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Theodor"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Paul"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Julia"), new { male = 0, female=1, @object=0 }),
                new(NameArray("Felix"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Charlotte"), new { male = 0, female=1, @object=0 }),
                new(NameArray("Brad"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Konstanze"), new { male = 0, female=1, @object=0 }),
                new(NameArray("Sebastian"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Angela"), new { male = 0, female=1, @object=0 }),
                new(NameArray("Eberhart"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Jeanne"), new { male = 0, female=1, @object=0 }),
                new(NameArray("Anja"), new { male = 0, female=1, @object=0 }),
                new(NameArray("Dennis"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Ronald"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Sindy"), new { male = 0, female=1, @object=0 }),
                new(NameArray("Juliane"), new { male = 0, female=1, @object=0 }),
                new(NameArray("Lilith"), new { male = 0, female=1, @object=0 }),
                new(NameArray("Marcel"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Klaus"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Ben"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Bill"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Wesley"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Beverly"), new { male = 0, female=1, @object=0 }),
                new(NameArray("Maurice"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Ryan"), new { male = 1, female=0, @object=0 }),
                new(NameArray("Daniel"), new { male = 1, female=0, @object=0 }),
            ]),
            Runs = 500,
            Rivalism = 5,
            TargetFitness = 0.01f,
            AfterRun = (index, fitness) => {
                if ((index & 511) == 0)
                    Console.WriteLine("{0}: {1}", index, fitness);
            },
            Threads = 2
        };
        PopulationEntry<DynamicFFConfiguration> result=population.Train(setup);
        Console.WriteLine($"Fitness: {result.Fitness:F2}");
        
        DynamicFFNet resultNet = new(result.Chromosome);

        
        resultNet.SetInputValues(NameArray("Jack"));
        resultNet.Compute();
        Console.WriteLine($"m: {resultNet["male"]}, f: {resultNet["female"]}, o: {resultNet["object"]}");

        Console.WriteLine(result.Chromosome);
    }

}