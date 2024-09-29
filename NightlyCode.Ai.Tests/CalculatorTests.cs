using NightlyCode.Ai.Genetics;
using NightlyCode.Ai.Net;
using NightlyCode.Ai.Net.Configurations;
using NightlyCode.Ai.Net.DynamicBinOp;
using NightlyCode.Ai.Net.Operations;
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

        Population<NeuronalOperationNetConfiguration> population = new(100, () => new(["x"], ["result"], 3, 3));
        EvolutionSetup<NeuronalOperationNetConfiguration> setup = new() {
                                                                            TrainingSet = [
                                                                                              config => Test(config, 1, 1), config => Test(config, 4, 2), config => Test(config, 9, 3),
                                                                                              config => Test(config, 16, 4), config => Test(config, 25, 5), config => Test(config, 36, 6),
                                                                                              config => Test(config, 49, 7), config => Test(config, 64, 8), config => Test(config, 81, 9),
                                                                                              config => Test(config, 100, 10), config=>Test(config, 121, 11), config=>Test(config, 144, 12)
                                                                                          ],
                                                                            Runs = 1000,
                                                                            FitnessAggregate = AggregateType.Max
                                                                            //Threads = Environment.ProcessorCount
                                                                            /*Acceptable = config => {
                                                                                             net.UpdateWeights(config.Operations);
                                                                                             net.Input["x"] = 16;
                                                                                             net.Compute();
                                                                                             double value = net.Output["result"];
                                                                                             return false;
                                                                                         }*/
                                                                        };
        Tuple<NeuronalOperationNetConfiguration, double> result=population.Train(setup);
        Console.WriteLine($"Fitness: {result.Item2:F2}");
        net.Update(result.Item1);
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

        Population<FeedForwardConfiguration> population = new(100, () => new(["x"], ["result"], 3, 3));
        EvolutionSetup<FeedForwardConfiguration> setup = new() {
                                                                   TrainingSet = [
                                                                                     config => Test(config, 1, 1), config => Test(config, 4, 2), config => Test(config, 9, 3),
                                                                                     config => Test(config, 16, 4), config => Test(config, 25, 5), config => Test(config, 36, 6),
                                                                                     config => Test(config, 49, 7), config => Test(config, 64, 8), config => Test(config, 81, 9),
                                                                                     config => Test(config, 100, 10), config=>Test(config, 121, 11), config=>Test(config, 144, 12)
                                                                                 ],
                                                                   Runs = 1000,
                                                                   FitnessAggregate = AggregateType.Average
                                                                   //Threads = Environment.ProcessorCount
                                                                   /*Acceptable = config => {
                                                                                    net.UpdateWeights(config.Operations);
                                                                                    net.Input["x"] = 16;
                                                                                    net.Compute();
                                                                                    double value = net.Output["result"];
                                                                                    return false;
                                                                                }*/
                                                               };
        Tuple<FeedForwardConfiguration, double> result=population.Train(setup);
        Console.WriteLine($"Fitness: {result.Item2:F2}");
        net.Update(result.Item1);
        for (int i = 1; i < 20; ++i) {
            net.Input["x"] = i;
            net.Compute();  
            Console.WriteLine($"sqrt({i})={net.Output["result"]}");
        }
        net.Input["x"] = 422933.0f;
        net.Compute();
        Console.WriteLine($"sqrt(422933)={net.Output["result"]}");
        Console.WriteLine(Json.Json.WriteString(result.Item1));
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

        Population<NeuronalOperationNetConfiguration> population = new(100, () => new(["x", "y", "z"], ["result"], 3, 3));
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
                                                                            FitnessAggregate = AggregateType.Average,
                                                                            //Threads = Environment.ProcessorCount
                                                                            /*Acceptable = config => {
                                                                                             net.UpdateWeights(config.Operations);
                                                                                             net.Input["x"] = 16;
                                                                                             net.Compute();
                                                                                             double value = net.Output["result"];
                                                                                             return false;
                                                                                         }*/
                                                                        };
        Tuple<NeuronalOperationNetConfiguration, double> result=population.Train(setup);
        Console.WriteLine($"Fitness: {result.Item2:F2}");
        net.Update(result.Item1);
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

        Population<FeedForwardConfiguration> population = new(100, () => new(["x", "y", "z"], ["result"], 3, 3));
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
                                                                   FitnessAggregate = AggregateType.Average
                                                               };
        Tuple<FeedForwardConfiguration, double> result=population.Train(setup);
        Console.WriteLine($"Fitness: {result.Item2:F2}");
        net.Update(result.Item1);
        net.Input["x"] = -3;
        net.Input["y"] = 7;
        net.Input["z"] = 20;
        net.Compute();
        Console.WriteLine($"f(-3,7,20)={net.Output["result"]}");

        //Console.WriteLine(Json.Json.WriteString(result.Item1.ExportDictionary()));
    }
    
    [Test, Parallelizable]
    public void MultiplyMinusDynamicBinOp() {
        DynamicBinOpConfiguration configuration = new(["x", "y", "z"], ["result"]);
        DynamicBinOpNet net = new(configuration);

        float Test(DynamicBinOpConfiguration config, float x, float y, float z, float expected) {
            net.Update(config);
            net.Input["x"] = x;
            net.Input["y"] = y;
            net.Input["z"] = z;
            net.Compute();
            float result = net.Output["result"];
            return Math.Abs(expected - result);
        }

        Population<DynamicBinOpConfiguration> population = new(100, () => new(["x", "y", "z"], ["result"]));
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
                                                                    FitnessAggregate = AggregateType.Average,
                                                                    AfterRun = (index, fitness) => {
                                                                                   if ((index & 511) == 0)
                                                                                       Console.WriteLine("{0}: {1}", index, fitness);
                                                                               }
                                                                };
        Tuple<DynamicBinOpConfiguration, double> result=population.Train(setup);
        Console.WriteLine($"Fitness: {result.Item2:F2}");
        net.Update(result.Item1);
        net.Input["x"] = -3;
        net.Input["y"] = 7;
        net.Input["z"] = 20;
        net.Compute();
        Console.WriteLine($"f(-3,7,20)={net.Output["result"]}");

        Console.WriteLine(Json.Json.WriteString(result.Item1, JsonOptions.Camel));
    }
}