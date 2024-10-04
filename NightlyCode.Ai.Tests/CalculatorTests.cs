using System.Collections.Concurrent;
using NightlyCode.Ai.Extern;
using NightlyCode.Ai.Genetics;
using NightlyCode.Ai.Net;
using NightlyCode.Ai.Net.Configurations;
using NightlyCode.Ai.Net.DynamicBinOp;
using NightlyCode.Json;

namespace NightlyCode.Ai.Tests;

[TestFixture, Parallelizable]
public class CalculatorTests {
    
    [Test, Parallelizable]
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
                                                                                         }*/
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
    }

    [Test, Parallelizable]
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
                                                                                }*/
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
    }
    
    [Test, Parallelizable]
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
                                                                                         }*/
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
    }
    
    [Test, Parallelizable]
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
    }
    
    [Test, Parallelizable]
    public void MultiplyMinusDynamicBinOp() {
        ConcurrentStack<DynamicBinOpNet> netStack = new();
        float Test(DynamicBinOpConfiguration config, float x, float y, float z, float expected) {
            if (!netStack.TryPop(out DynamicBinOpNet net))
                net = new(config);
            else net.Update(config);
            
            net["x"] = x;
            net["y"] = y;
            net["z"] = z;
            net.Compute();
            float result = net["result"];

            netStack.Push(net);
            return Math.Abs(expected - result);
        }

        Population<DynamicBinOpConfiguration> population = new(100, rng => new(["x", "y", "z"], ["result"], rng));
        EvolutionSetup<DynamicBinOpConfiguration> setup = new() {
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
                                                                    AfterRun = (index, fitness) => {
                                                                                   if ((index & 511) == 0)
                                                                                       Console.WriteLine("{0}: {1}", index, fitness);
                                                                               },
                                                                    Threads = 2
                                                                };
        PopulationEntry<DynamicBinOpConfiguration> result=population.Train(setup);
        Console.WriteLine($"Fitness: {result.Fitness:F2}");

        if (!netStack.TryPop(out DynamicBinOpNet net))
            net = new(result.Chromosome);
        else net.Update(result.Chromosome);
        
        net["x"] = -3;
        net["y"] = 7;
        net["z"] = 20;
        net.Compute();
        Console.WriteLine($"f(-3,7,20)={net["result"]}");

        Console.WriteLine(result.Chromosome);
    }
    
    [Test, Parallelizable]
    public void ExcellenceDynamicBinOp() {
        ConcurrentStack<DynamicBinOpNet> netStack = new();
        
        float Test(DynamicBinOpConfiguration config, float visits, float appclicks, float applications, float profit, float expected) {
            if (!netStack.TryPop(out DynamicBinOpNet net))
                net = new(config);
            else net.Update(config);

            net["visits"] = visits;
            net["appclicks"] = appclicks;
            net["applications"] = applications;
            net["profit"] = profit;
            net.Compute();
            float result = net["excellence"];
            return Math.Abs(expected - result);
        }

        Dictionary<string, object> samples = Json.Json.Read<Dictionary<string, object>>(File.ReadAllText("Data/excellence_samples.json"));
        
        Population<DynamicBinOpConfiguration> population = new(100, rng => new(["visits", "appclicks", "applications", "profit"], ["excellence"], rng));
        EvolutionSetup<DynamicBinOpConfiguration> setup = new() {
                                                                    TrainingSet = JPath.Select<object[]>(samples, "samples")
                                                                                       .Select(s => new Func<DynamicBinOpConfiguration, float>(config => Test(config,
                                                                                                                                                              Converter.Convert<float>(JPath.Select(s, "inputs/visits")),
                                                                                                                                                              Converter.Convert<float>(JPath.Select(s, "inputs/appclicks")),
                                                                                                                                                              Converter.Convert<float>(JPath.Select(s, "inputs/applications")),
                                                                                                                                                              Converter.Convert<float>(JPath.Select(s, "inputs/profit")),
                                                                                                                                                              Converter.Convert<float>(JPath.Select(s, "outputs/excellence")))))
                                                                                       .ToArray(),
                                                                    Runs = 5000,
                                                                    AfterRun = (index, fitness) => {
                                                                                   if ((index & 511) == 0)
                                                                                       Console.WriteLine("{0}: {1}", index, fitness);
                                                                               },
                                                                    Threads = 2
                                                                };
        PopulationEntry<DynamicBinOpConfiguration> result=population.Train(setup);
        Console.WriteLine($"Fitness: {result.Fitness:F2}");
        if (!netStack.TryPop(out DynamicBinOpNet net))
            net = new(result.Chromosome);
        else net.Update(result.Chromosome);

        net["visits"] = 50;
        net["appclicks"] = 4;
        net["applications"] = 4;
        net["profit"] = 300;
        net.Compute();
        Console.WriteLine($"Excellence {net["excellence"]}");

        Console.WriteLine(result.Chromosome);
    }
    
    [Test, Parallelizable]
    public void SequenceDynamicBinOp() {
        ConcurrentStack<DynamicBinOpNet> netStack = new();
        
        float Test(DynamicBinOpConfiguration config, float x, float expected) {
            if (!netStack.TryPop(out DynamicBinOpNet net))
                net = new(config);
            else net.Update(config);
            
            net["x"] = x;
            net.Compute();
            float result = net["y"];
            netStack.Push(net);
            return Math.Abs(expected - result);
        }

        Population<DynamicBinOpConfiguration> population = new(100, rng => new(["x"], ["y"], rng));
        EvolutionSetup<DynamicBinOpConfiguration> setup = new() {
                                                                    TrainingSet = [
                                                                                      c => Test(c, 1,2),
                                                                                      c => Test(c, 2, 6),
                                                                                      c=>Test(c, 3, 12),
                                                                                      c=>Test(c, 4, 20),
                                                                                      c=>Test(c, 5, 30),
                                                                                      c=>Test(c, 6, 42),
                                                                                      c=>Test(c, 7, 56),
                                                                                      c=>Test(c, 8, 72),
                                                                                      c=>Test(c, 9, 90),
                                                                                      c=>Test(c, 10, 110)
                                                                                  ],
                                                                    Runs = 5000,
                                                                    AfterRun = (index, fitness) => {
                                                                                   if ((index & 511) == 0)
                                                                                       Console.WriteLine("{0}: {1}", index, fitness);
                                                                               },
                                                                    Threads = 2
                                                                };
        PopulationEntry<DynamicBinOpConfiguration> result=population.Train(setup);
        Console.WriteLine($"Fitness: {result.Fitness:F2}");
        if (!netStack.TryPop(out DynamicBinOpNet net))
            net = new(result.Chromosome);
        else net.Update(result.Chromosome);

        net["x"] = 20;
        net.Compute();
        Console.WriteLine($"{net["y"]}");

        Console.WriteLine(result.Chromosome);
        Console.WriteLine(Json.Json.WriteString(result.Chromosome));
    }
    
    [Test, Parallelizable]
    public void SequenceDynamicBinOpTwice() {
        ConcurrentStack<DynamicBinOpNet> netStack = new();
        float Test(DynamicBinOpConfiguration config, float x, float expected) {
            if (!netStack.TryPop(out DynamicBinOpNet localNet))
                localNet = new(config);
            else localNet.Update(config);

            localNet["x"] = x;
            localNet.Compute();
            float result = localNet["y"];
            netStack.Push(localNet);
            return Math.Abs(expected - result);
        }

        Population<DynamicBinOpConfiguration> population = new(100, rng => new(["x"], ["y"], rng));
        EvolutionSetup<DynamicBinOpConfiguration> setup = new() {
                                                                    TrainingSet = [
                                                                                      c => Test(c, 1,2),
                                                                                      c => Test(c, 2, 6),
                                                                                      c=>Test(c, 3, 12),
                                                                                      c=>Test(c, 4, 20),
                                                                                      c=>Test(c, 5, 30),
                                                                                      c=>Test(c, 6, 42),
                                                                                      c=>Test(c, 7, 56),
                                                                                      c=>Test(c, 8, 72),
                                                                                      c=>Test(c, 9, 90),
                                                                                      c=>Test(c, 10, 110)
                                                                                  ],
                                                                    Runs = 5000,
                                                                    TargetFitness = 0.01f,
                                                                    AfterRun = (index, fitness) => {
                                                                                   if ((index & 511) == 0)
                                                                                       Console.WriteLine("{0}: {1}", index, fitness);
                                                                               },
                                                                    Threads = 2
                                                                };
        PopulationEntry<DynamicBinOpConfiguration> result=population.Train(setup);
        Console.WriteLine($"Fitness: {result.Fitness:F2}");

        if (!netStack.TryPop(out DynamicBinOpNet resultNet))
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
}