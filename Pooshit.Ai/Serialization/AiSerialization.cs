using System.Text;
using Pooshit.Ai.Genetics;
using Pooshit.Ai.Net.DynamicBO;
using Pooshit.Ai.Net.DynamicFF;
using Pooshit.Ai.Net.Operations;
using Pooshit.Ai.Neurons;

namespace Pooshit.Ai.Serialization;

/// <summary>
/// serializes ai structures to a binary stream
/// </summary>
/// <remarks>
/// this is mainly used to serialize large populations to take up less space
/// for small populations they can also just be serialized to json for ease of use
/// </remarks>
public static class AiSerialization {
    const string Header = "P00AI";
    /// <summary>
    /// serialized a population to a stream
    /// </summary>
    /// <param name="population">population to serialize</param>
    /// <param name="outStream">stream to serialize population to</param>
    /// <typeparam name="T">type of chromosome contained in population</typeparam>
    public static void Serialize<T>(Population<T> population, Stream outStream) 
        where T : class, IChromosome<T> {
        using BinaryWriter writer = new(outStream);
        writer.Write(Encoding.UTF8.GetBytes(Header));
        writer.Write((byte)SerializationChunk.Type);
        if (population is Population<DynamicBOConfiguration> dboPopulation)
            Serialize(dboPopulation, writer);
        else if (population is Population<DynamicFFConfiguration> dffPopulation)
            Serialize(dffPopulation, writer);
        else throw new NotSupportedException("Unsupported population type");
    }

    /// <summary>
    /// deserializes population entries from a serialized population stream
    /// </summary>
    /// <param name="inStream">stream from which to read population data</param>
    /// <typeparam name="T">type of population data to read</typeparam>
    /// <returns>deserialized population entries</returns>
    public static IEnumerable<PopulationEntry<T>> Deserialize<T>(Stream inStream)
        where T : IChromosome<T> {
        using BinaryReader reader = new(inStream, Encoding.UTF8);
        string header = Encoding.UTF8.GetString(reader.ReadBytes(5));
        if (header != Header)
            throw new InvalidOperationException("Incorrect header");

        if (reader.ReadByte() != (byte)SerializationChunk.Type)
            throw new InvalidOperationException("Expected type chunk");

        switch ((ChromosomeType)reader.ReadByte()) {
            case ChromosomeType.DynamicBO:
                foreach (PopulationEntry<DynamicBOConfiguration> entry in ReadDBOPopulation(reader))
                    yield return (PopulationEntry<T>)(object)entry;
                break;
            case ChromosomeType.DynamicFF:
                foreach (PopulationEntry<DynamicFFConfiguration> entry in ReadDFFPopulation(reader))
                    yield return (PopulationEntry<T>)(object)entry;
                break;
        }
    }

    static NeuronConfig DeserializeInputNeuron(BinaryReader reader) {
        string name = reader.ReadString();
        return new() {
                         Name = string.IsNullOrEmpty(name) ? null : name
                     };
    }

    static NeuronConfig DeserializeGeneratorNeuron(BinaryReader reader) {
        string name = reader.ReadString();
        string generator = reader.ReadString();
        return new() {
                         Name = string.IsNullOrEmpty(name) ? null : name,
                         Generator = string.IsNullOrEmpty(generator) ? null : generator
                     };
    }
    
    static NeuronConfig DeserializeOutputNeuron(BinaryReader reader) {
        return new() {
                         Name = reader.ReadString(),
                         Aggregate = (AggregateType)reader.ReadByte(),
                         Activation = (ActivationFunc)reader.ReadByte(),
                         OrderNumber = 1.0f
                     };
    }

    static NeuronConfig DeserializeNeuron(BinaryReader reader) {
        return new() {
                         OrderNumber = reader.ReadSingle(),
                         Aggregate = (AggregateType)reader.ReadByte(),
                         Activation = (ActivationFunc)reader.ReadByte()
                     };
    }

    static BOConnection DeserializeBOConnection(BinaryReader reader) {
        return new() {
                         Lhs = reader.ReadInt32(),
                         Rhs = reader.ReadInt32(),
                         Target = reader.ReadInt32(),
                         Operation = (OperationType)reader.ReadByte(),
                         Weight = reader.ReadSingle()
                     };
    }

    static FFConnection DeserializeFFConnection(BinaryReader reader) {
        return new() {
                         Source = reader.ReadInt32(),
                         Target = reader.ReadInt32(),
                         Weight = reader.ReadSingle()
                     };
    }

    static IEnumerable<PopulationEntry<DynamicBOConfiguration>> ReadDBOPopulation(BinaryReader reader) {
        if (reader.ReadByte() != (byte)SerializationChunk.Chromosomes)
            throw new InvalidOperationException("Unexpected chunk type");

        int chromosomeLength = reader.ReadInt32();
        while (chromosomeLength-- > 0) {
            PopulationEntry<DynamicBOConfiguration> entry = new() {
                                                                      Fitness = reader.ReadSingle()
                                                                  };

            SerializationChunk chunk = (SerializationChunk)reader.ReadByte();
            if (chunk == SerializationChunk.Ancestry) {
                entry.AncestryId = new(reader.ReadBytes(16));
                chunk = (SerializationChunk)reader.ReadByte();
            }

            List<NeuronConfig> neurons = [];
            int neuronCount;
            switch (chunk) {
                case SerializationChunk.InputNeurons:
                    neuronCount = reader.ReadInt32();
                    while (neuronCount-- > 0) {
                        NeuronConfig neuron = DeserializeInputNeuron(reader);
                        neuron.Index = neurons.Count;
                        neurons.Add(neuron);
                    }
                break;
                case SerializationChunk.InputGenerators:
                    neuronCount = reader.ReadInt32();
                    while (neuronCount-- > 0) {
                        NeuronConfig neuron = DeserializeGeneratorNeuron(reader);
                        neuron.Index = neurons.Count;
                        neurons.Add(neuron);
                    }
                break;
                default:
                    throw new InvalidOperationException("Unexpected chunk type");
            }

            if(reader.ReadByte()!=(byte)SerializationChunk.OutputNeurons)
                throw new InvalidOperationException("Unexpected chunk type");

            neuronCount = reader.ReadInt32();
            while (neuronCount-- > 0) {
                NeuronConfig neuron = DeserializeOutputNeuron(reader);
                neuron.Index = neurons.Count;
                neuron.OrderNumber = 1.0f;
                neurons.Add(neuron);
            }

            if(reader.ReadByte()!=(byte)SerializationChunk.Neurons)
                throw new InvalidOperationException("Unexpected chunk type");

            neuronCount = reader.ReadInt32();

            while (neuronCount-- > 0) {
                NeuronConfig neuron = DeserializeNeuron(reader);
                neuron.Index = neurons.Count;
                neurons.Add(neuron);
            }

            if(reader.ReadByte()!=(byte)SerializationChunk.Connections)
                throw new InvalidOperationException("Unexpected chunk type");

            List<BOConnection> connections = [];
            int connectionCount = reader.ReadInt32();
            while (connectionCount-- > 0) {
                connections.Add(DeserializeBOConnection(reader));
            }

            entry.Chromosome = new(neurons.ToArray(), connections.ToArray());
            yield return entry;
        }
    }
    
    static IEnumerable<PopulationEntry<DynamicFFConfiguration>> ReadDFFPopulation(BinaryReader reader) {
        if (reader.ReadByte() != (byte)SerializationChunk.Chromosomes)
            throw new InvalidOperationException("Unexpected chunk type");

        int chromosomeLength = reader.ReadInt32();
        while (chromosomeLength-- > 0) {
            PopulationEntry<DynamicFFConfiguration> entry = new() {
                                                                      Fitness = reader.ReadSingle()
                                                                  };

            SerializationChunk chunk = (SerializationChunk)reader.ReadByte();
            if (chunk == SerializationChunk.Ancestry) {
                entry.AncestryId = new(reader.ReadBytes(16));
                chunk = (SerializationChunk)reader.ReadByte();
            }

            List<NeuronConfig> neurons = [];
            int neuronCount;
            switch (chunk) {
                case SerializationChunk.InputNeurons:
                    neuronCount = reader.ReadInt32();
                    while (neuronCount-- > 0) {
                        NeuronConfig neuron = DeserializeInputNeuron(reader);
                        neuron.Index = neurons.Count;
                        neurons.Add(neuron);
                    }
                    break;
                case SerializationChunk.InputGenerators:
                    neuronCount = reader.ReadInt32();
                    while (neuronCount-- > 0) {
                        NeuronConfig neuron = DeserializeGeneratorNeuron(reader);
                        neuron.Index = neurons.Count;
                        neurons.Add(neuron);
                    }
                    break;
                default:
                    throw new InvalidOperationException("Unexpected chunk type");
            }


            if(reader.ReadByte()!=(byte)SerializationChunk.OutputNeurons)
                throw new InvalidOperationException("Unexpected chunk type");

            neuronCount = reader.ReadInt32();
            while (neuronCount-- > 0) {
                NeuronConfig neuron = DeserializeOutputNeuron(reader);
                neuron.Index = neurons.Count;
                neuron.OrderNumber = 1.0f;
                neurons.Add(neuron);
            }

            if(reader.ReadByte()!=(byte)SerializationChunk.Neurons)
                throw new InvalidOperationException("Unexpected chunk type");

            neuronCount = reader.ReadInt32();

            while (neuronCount-- > 0) {
                NeuronConfig neuron = DeserializeNeuron(reader);
                neuron.Index = neurons.Count;
                neurons.Add(neuron);
            }

            if(reader.ReadByte()!=(byte)SerializationChunk.Connections)
                throw new InvalidOperationException("Unexpected chunk type");

            List<FFConnection> connections = [];
            int connectionCount = reader.ReadInt32();
            while (connectionCount-- > 0) {
                connections.Add(DeserializeFFConnection(reader));
            }

            entry.Chromosome = new(neurons.ToArray(), connections.ToArray());
            yield return entry;
        }

    }

    static void SerializeInputNeuron(NeuronConfig neuron, BinaryWriter writer) {
        writer.Write(neuron.Name??"");
    }

    static void SerializeGeneratorNeuron(NeuronConfig neuron, BinaryWriter writer) {
        writer.Write(neuron.Name??"");
        writer.Write(neuron.Generator??"");
    }

    static void SerializeOutputNeuron(NeuronConfig neuron, BinaryWriter writer) {
        writer.Write(neuron.Name);
        writer.Write((byte)neuron.Aggregate);
        writer.Write((byte)neuron.Activation);
    }

    static void SerializeNeuron(NeuronConfig neuron, BinaryWriter writer) {
        writer.Write(neuron.OrderNumber);
        writer.Write((byte)neuron.Aggregate);
        writer.Write((byte)neuron.Activation);
    }

    static void SerializeBOConnection(BOConnection connection, BinaryWriter writer) {
        writer.Write(connection.Lhs);
        writer.Write(connection.Rhs);
        writer.Write(connection.Target);
        writer.Write((byte)connection.Operation);
        writer.Write(connection.Weight);
    }

    static void SerializeFFConnection(FFConnection connection, BinaryWriter writer) {
        writer.Write(connection.Source);
        writer.Write(connection.Target);
        writer.Write(connection.Weight);
    }

    static void Serialize(Population<DynamicBOConfiguration> dboPopulation, BinaryWriter writer) {
        writer.Write((byte)ChromosomeType.DynamicBO);
        writer.Write((byte)SerializationChunk.Chromosomes);
        writer.Write(dboPopulation.Entries.Length);
        foreach (PopulationEntry<DynamicBOConfiguration> entry in dboPopulation.Entries) {
            writer.Write(entry.Fitness);
            writer.Write((byte)SerializationChunk.Ancestry);
            writer.Write(entry.AncestryId.ToByteArray());
            if (entry.Chromosome.Neurons.Take(entry.Chromosome.InputCount).Any(n => !string.IsNullOrEmpty(n.Generator))) {
                writer.Write((byte)SerializationChunk.InputGenerators);
                writer.Write(entry.Chromosome.InputCount);
                foreach (NeuronConfig neuron in entry.Chromosome.Neurons.Take(entry.Chromosome.InputCount))
                    SerializeGeneratorNeuron(neuron, writer);
            }
            else {
                writer.Write((byte)SerializationChunk.InputNeurons);
                writer.Write(entry.Chromosome.InputCount);
                foreach (NeuronConfig neuron in entry.Chromosome.Neurons.Take(entry.Chromosome.InputCount))
                    SerializeInputNeuron(neuron, writer);
            }

            writer.Write((byte)SerializationChunk.OutputNeurons);
            writer.Write(entry.Chromosome.OutputCount);
            foreach (NeuronConfig neuron in entry.Chromosome.Neurons.Skip(entry.Chromosome.InputCount).Take(entry.Chromosome.OutputCount))
                SerializeOutputNeuron(neuron, writer);
            writer.Write((byte)SerializationChunk.Neurons);
            writer.Write(entry.Chromosome.Neurons.Length - entry.Chromosome.InputCount - entry.Chromosome.OutputCount);
            foreach (NeuronConfig neuron in entry.Chromosome.Neurons.Skip(entry.Chromosome.InputCount+entry.Chromosome.OutputCount))
                SerializeNeuron(neuron, writer);
            writer.Write((byte)SerializationChunk.Connections);
            writer.Write(entry.Chromosome.Connections.Length);
            foreach (BOConnection connection in entry.Chromosome.Connections)
                SerializeBOConnection(connection, writer);
        }
    }
    
    static void Serialize(Population<DynamicFFConfiguration> dffPopulation, BinaryWriter writer) {
        writer.Write((byte)ChromosomeType.DynamicFF);
        writer.Write((byte)SerializationChunk.Chromosomes);
        writer.Write(dffPopulation.Entries.Length);
        foreach (PopulationEntry<DynamicFFConfiguration> entry in dffPopulation.Entries) {
            writer.Write(entry.Fitness);
            writer.Write((byte)SerializationChunk.Ancestry);
            writer.Write(entry.AncestryId.ToByteArray());
            writer.Write((byte)SerializationChunk.InputNeurons);
            writer.Write(entry.Chromosome.InputCount);
            foreach (NeuronConfig neuron in entry.Chromosome.Neurons.Take(entry.Chromosome.InputCount))
                SerializeInputNeuron(neuron, writer);
            writer.Write((byte)SerializationChunk.OutputNeurons);
            writer.Write(entry.Chromosome.OutputCount);
            foreach (NeuronConfig neuron in entry.Chromosome.Neurons.Skip(entry.Chromosome.InputCount).Take(entry.Chromosome.OutputCount))
                SerializeOutputNeuron(neuron, writer);
            writer.Write((byte)SerializationChunk.Neurons);
            writer.Write(entry.Chromosome.Neurons.Length - entry.Chromosome.InputCount - entry.Chromosome.OutputCount);
            foreach (NeuronConfig neuron in entry.Chromosome.Neurons.Skip(entry.Chromosome.InputCount+entry.Chromosome.OutputCount))
                SerializeNeuron(neuron, writer);
            writer.Write((byte)SerializationChunk.Connections);
            writer.Write(entry.Chromosome.Connections.Length);
            foreach (FFConnection connection in entry.Chromosome.Connections)
                SerializeFFConnection(connection, writer);
        }
    }

}