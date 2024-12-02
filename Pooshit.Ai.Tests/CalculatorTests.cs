using System.Collections.Concurrent;
using Pooshit.Ai.Genetics;
using Pooshit.Ai.Net;
using Pooshit.Ai.Net.DynamicBO;
using Pooshit.Ai.Net.DynamicFF;
using Pooshit.Ai.Neurons;
using Pooshit.Json;
using Pooshit.Scripting;
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
        ConcurrentStack<DynamicBONet> netStack = new();

        Population<DynamicBOConfiguration> population = new(100, rng => new(new[]{"x", "y", "z"}, ["result"], rng));
        EvolutionSetup<DynamicBOConfiguration> setup = new() {
                                                                    Evaluator = new SamplesEvaluator<DynamicBOConfiguration, DynamicBONet>([
                                                                                                                                                     new(new{x=5,y=2,z=7},new{result=3}),
                                                                                                                                                     new(new{x=3,y=3,z=3},new{result=6}),
                                                                                                                                                     new(new{x=10,y=10,z=2},new{result=98}),
                                                                                                                                                     new(new{x=5,y=5,z=1},new{result=24}),
                                                                                                                                                     new(new{x=1,y=40,z=9},new{result=31}),
                                                                                                                                                     new(new{x=6,y=10,z=10},new{result=50}),
                                                                                                                                                     new(new{x=7,y=8,z=6},new{result=50}),
                                                                                                                                                     new(new{x=11,y=8,z=6},new{result=82}),
                                                                                                                                                     new(new{x=2,y=70,z=12},new{result=128}),
                                                                                                                                                     new(new{x=12,y=12,z=4},new{result=140}),
                                                                                                                                                     new(new{x=9,y=12,z=19},new{result=89}),
                                                                                                                                                     new(new{x=1,y=2,z=3},new{result=-1}),
                                                                                                                                                     new(new{x=8,y=3,z=8},new{result=16}),
                                                                                                                                                     new(new{x=2,y=34,z=9},new{result=59}),
                                                                                                                                                     new(new{x=8,y=66,z=3},new{result=525}),
                                                                                                                                                     new(new{x=20,y=6,z=333},new{result=-213}),
                                                                                                                                                     new(new{x=4,y=60,z=399},new{result=-159}),
                                                                                                                                                     new(new{x=7,y=18,z=170},new{result=-49}),
                                                                                                                                                     new(new{x=-3,y=7,z=20},new{result=-41}),
                                                                                                                                                     new(new{x=-3,y=8,z=20},new{result=-44}),
                                                                                                                                                     new(new{x=-3,y=-8,z=20},new{result=4}),
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
        
        net["x"] = 5;
        net["y"] = 13;
        net["z"] = 44;
        net.Compute();
        Console.WriteLine($"f(5,13,44)={net["result"]}");

        Console.WriteLine(result.Chromosome);
    }
    
    [Test, Parallelizable]
    public void ExcellenceDynamicBinOp() {
        ConcurrentStack<DynamicBONet> netStack = new();
        
        Dictionary<string, object> samples = Json.Read<Dictionary<string, object>>(File.ReadAllText("Data/excellence_samples.json"));
        
        Population<DynamicBOConfiguration> population = new(100, rng => new(new NeuronSpec[]{"visits", "appclicks", "applications", "profit", new("vcr", "net[\"appclicks\"]/net[\"visits\"].max(1.0)") , new("ccr", "net[\"applications\"]/net[\"appclicks\"].max(1.0)"), new("ppa", "net[\"profit\"]/net[\"applications\"].max(1.0)")}, ["excellence"], rng));
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
        Dictionary<string, IScript> scripts = new();

        Dictionary<string, IScript> ScriptBuilder(DynamicBOConfiguration config) {
            Dictionary<string, IScript> scripts = new();
            foreach (NeuronConfig neuronConfig in config.Neurons.Take(config.InputCount).Where(n => !string.IsNullOrEmpty(n.Generator))) scripts[neuronConfig.Name] = parser.Parse(neuronConfig.Generator);
            return scripts;
        }

        EvolutionSetup<DynamicBOConfiguration> setup = new()
        {
            Evaluator = new SamplesEvaluator<DynamicBOConfiguration, DynamicBONet>(trainingSamples) {
                InputGenerator = (net, config) => {
                    scripts ??= ScriptBuilder(config);
                    foreach ((string neuron, IScript generator) in scripts)
                        net[neuron] = generator.Execute<float>();
                }
            },
            Runs = 5000,
            AfterRun = (index, fitness) =>
            {
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

        net["visits"] = 144;
        net["appclicks"] = 4;
        net["applications"] = 1;
        net["profit"] = 5;
        net["vcr"] = 0.02777777777777777777777777777778f;
        net["ccr"] = 0.25f;
        net["ppa"] = 5;
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
            Runs = 5000,
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
                                                                                                                                            new(new{x=1},new{y=2}),
                                                                                                                                            new(new{x=2},new{y=6}),
                                                                                                                                            new(new{x=3},new{y=12}),
                                                                                                                                            new(new{x=4},new{y=20}),
                                                                                                                                            new(new{x=5},new{y=30}),
                                                                                                                                            new(new{x=6},new{y=42}),
                                                                                                                                            new(new{x=7},new{y=56}),
                                                                                                                                            new(new{x=8},new{y=72}),
                                                                                                                                            new(new{x=9},new{y=90}),
                                                                                                                                            new(new{x=10},new{y=110}),
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
        ConcurrentStack<DynamicBONet> netStack = new();
        
        Population<DynamicBOConfiguration> population = new(100, rng => new(new[]{"x"}, ["y"], rng));
        EvolutionSetup<DynamicBOConfiguration> setup = new() {
                                                                    Evaluator = new SamplesEvaluator<DynamicBOConfiguration, DynamicBONet>([
                                                                                                                                                     new(new{x=1},new{y=2}),
                                                                                                                                                     new(new{x=2},new{y=6}),
                                                                                                                                                     new(new{x=3},new{y=12}),
                                                                                                                                                     new(new{x=4},new{y=20}),
                                                                                                                                                     new(new{x=5},new{y=30}),
                                                                                                                                                     new(new{x=6},new{y=42}),
                                                                                                                                                     new(new{x=7},new{y=56}),
                                                                                                                                                     new(new{x=8},new{y=72}),
                                                                                                                                                     new(new{x=9},new{y=90}),
                                                                                                                                                     new(new{x=10},new{y=110}),
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

        if (!netStack.TryPop(out DynamicBONet resultNet))
            resultNet = new(result.Chromosome);
        else resultNet.Update(result.Chromosome);

        resultNet["x"] = 20;
        resultNet.Compute();
        Console.WriteLine($"{resultNet["y"]}");

        Console.WriteLine(result.Chromosome);
        
        result=population.Train(setup);
        Console.WriteLine($"Fitness: {result.Fitness:F2}");
        if (!netStack.TryPop(out resultNet))
            resultNet = new(result.Chromosome);
        else resultNet.Update(result.Chromosome);

        resultNet["x"] = 20;

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
                                                                                                                                                     new(NameArray("Spitzenplatz"),new{y=3}),
                                                                                                                                                     new(NameArray("Kräuterbeet"),new{y=5}),
                                                                                                                                                     new(NameArray("Muschelstrand"),new{y=3}),
                                                                                                                                                     new(NameArray("Oldenburg"),new{y=3}),
                                                                                                                                                     new(NameArray("Fernsehgarten"),new{y=4}),
                                                                                                                                                     new(NameArray("Ananasrhabarbersalat"),new{y=8}),
                                                                                                                                                     new(NameArray("Interkontinental"),new{y=6}),
                                                                                                                                                     new(NameArray("Sonnenallee"),new{y=5}),
                                                                                                                                                     new(NameArray("Gemüsebrühe"),new{y=5}),
                                                                                                                                                     new(NameArray("Ölplattform"),new{y=3}),
                                                                                                                                                     new(NameArray("Mandelbrot"), new{y=3}),
                                                                                                                                                     new(NameArray("Sonderrecht"), new{y=3}),
                                                                                                                                                     new(NameArray("Molekularbiologe"), new{y=8}),
                                                                                                                                                     new(NameArray("Wetteransage"), new{y=5}),
                                                                                                                                                     new(NameArray("Ananas"), new{y=3}),
                                                                                                                                                     new(NameArray("Schonungslos"), new{y=3}),
                                                                                                                                                     new(NameArray("Sauerkraut"), new{y=5}),
                                                                                                                                                     new(NameArray("Loeffelbiscuit"), new{y=6}),
                                                                                                                                                     new(NameArray("Pfeiffenblaeser"), new{y=6}),
                                                                                                                                                     new(NameArray("Affenbrotbaum"), new{y=5}),
                                                                                                                                                     new(NameArray("Illustrationsbuch"), new{y=6}),
                                                                                                                                                     new(NameArray("Offenbarung"), new{y=4}),
                                                                                                                                                     new(NameArray("Hausverkauf"), new{y=5}),
                                                                                                                                                     new(NameArray("Lampenkranz"), new{y=3}),
                                                                                                                                                     new(NameArray("Kostenstelle"), new{y=4}),
                                                                                                                                                     new(NameArray("Oberfachverkaeufer"), new{y=8})
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

        
        resultNet.SetInputValues(NameArray("Semmelbroesel"));
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
                                                                                                                                               new(NameArray("Spitzenplatz"),new{y=3}),
                                                                                                                                               new(NameArray("Kräuterbeet"),new{y=5}),
                                                                                                                                               new(NameArray("Muschelstrand"),new{y=3}),
                                                                                                                                               new(NameArray("Oldenburg"),new{y=3}),
                                                                                                                                               new(NameArray("Fernsehgarten"),new{y=4}),
                                                                                                                                               new(NameArray("Ananasrhabarbersalat"),new{y=8}),
                                                                                                                                               new(NameArray("Interkontinental"),new{y=6}),
                                                                                                                                               new(NameArray("Sonnenallee"),new{y=5}),
                                                                                                                                               new(NameArray("Gemüsebrühe"),new{y=5}),
                                                                                                                                               new(NameArray("Ölplattform"),new{y=3}),
                                                                                                                                               new(NameArray("Mandelbrot"), new{y=3}),
                                                                                                                                               new(NameArray("Sonderrecht"), new{y=3}),
                                                                                                                                               new(NameArray("Molekularbiologe"), new{y=8}),
                                                                                                                                               new(NameArray("Wetteransage"), new{y=5}),
                                                                                                                                               new(NameArray("Ananas"), new{y=3}),
                                                                                                                                               new(NameArray("Schonungslos"), new{y=3}),
                                                                                                                                               new(NameArray("Sauerkraut"), new{y=5}),
                                                                                                                                               new(NameArray("Loeffelbiscuit"), new{y=6}),
                                                                                                                                               new(NameArray("Pfeiffenblaeser"), new{y=6}),
                                                                                                                                               new(NameArray("Affenbrotbaum"), new{y=5}),
                                                                                                                                               new(NameArray("Illustrationsbuch"), new{y=6}),
                                                                                                                                               new(NameArray("Offenbarung"), new{y=4}),
                                                                                                                                               new(NameArray("Hausverkauf"), new{y=5}),
                                                                                                                                               new(NameArray("Lampenkranz"), new{y=3}),
                                                                                                                                               new(NameArray("Kostenstelle"), new{y=4}),
                                                                                                                                               new(NameArray("Oberfachverkaeufer"), new{y=8})
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
}