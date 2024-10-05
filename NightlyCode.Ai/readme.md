This is a preview package and is expected to make breaking changes like every other release.

# NightlyCode.Ai
With this package you can build and train neuronal nets.

## DynamicBinOp
The best working implementation in this package currently is the DynamicBinOp net which features a self growing net which trains by mutating its elements and generating new neurons from time to time.

### Usage

The most basic case to use a dynamicbinopnet would be to create one manually.
```cs
DynamicBinOpNet net=new(new(["x"], ["y"]))
```

This would create a net with one input neuron "x" and an output neuron "y". While this is a working neuronal net it doesn't do anything meaningful as there are no connections in it.

To have a working neuronal net you first need to train a configuration.

### Training
Training is done by a population which hopefully evolves to contain a working configuration at some point.
```cs
Population<DynamicBinOpConfiguration> population = new(100, rng => new(["x"], ["y"], rng));
```

Then a setup is needed which contains the training samples used to train the configurations.

```cs
        EvolutionSetup<DynamicBinOpConfiguration> setup = new() {
                                                                    Evaluator = new DboEvaluator([...]),
                                                                    Runs = 5000,
                                                                    Threads = 2
};
```
The Evaluator property needs to be an evaluation logic which measures the deviation of model results to the expected values. The evaluator above needs to be filled with samples which are then used in training.

```

When the setup is created with samples the population can be trained with

```cs
PopulationEntry<DynamicBinOpConfiguration> result=population.Train(setup);
```

The population returns the best configuration based on the training set after the number of runs are completed or a threshold is reached. This configuration can be used to feed a neuronal net and compute values.