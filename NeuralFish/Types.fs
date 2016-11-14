module NeuralFish.Types

exception NodeRecordTypeException of string
exception NeuronInstanceException of string
exception NoBiasInRecordForNeuronException of string
exception SensorRecordDoesNotHaveASyncFunctionException of string

type NeuronId = int

type ActivationFunctionId = int
type SyncFunctionId = int
type OutputHookId = int

type Bias = float
type Weight = float
type NeuronOutput = float
type ActuatorOutput = float

type ActivationFunction = NeuronOutput -> NeuronOutput
type SyncFunction = unit -> NeuronOutput seq
type OutputHookFunction = ActuatorOutput -> unit

type ActivationFunctions = Map<ActivationFunctionId,ActivationFunction>
type SyncFunctions = Map<SyncFunctionId,SyncFunction>
type OutputHookFunctions = Map<OutputHookId, OutputHookFunction>

type OutputHookFunctionIds = OutputHookId seq

type NeuronLayerId = float

type NeuronConnectionId = System.Guid

type Synapse = NeuronId*NeuronOutput*Weight

type AxonHillockBarrier = Map<NeuronConnectionId,Synapse>

type IncomingSynapses = Map<NeuronConnectionId, Synapse>

type InactiveNeuronConnection = NeuronId*Weight

type NodeRecordType =
  | Neuron
  | Sensor
  | Actuator

type NodeRecordConnections = Map<NeuronConnectionId,InactiveNeuronConnection>

type NodeRecord =
  {
    Layer: NeuronLayerId
    NodeId: NeuronId
    NodeType: NodeRecordType
    OutboundConnections: NodeRecordConnections
    Bias: Bias option
    ActivationFunctionId: ActivationFunctionId option
    SyncFunctionId: SyncFunctionId option
    OutputHookId: OutputHookId option
    MaximumVectorLength: int option
  }

type NodeRecords = Map<NeuronId,NodeRecord>

type CortexMessage =
    | Think of AsyncReplyChannel<unit>
    | KillCortex of AsyncReplyChannel<NodeRecords>

type CortexInstance = MailboxProcessor<CortexMessage>

type NeuronProperties =
  {
    ActivationFunction: ActivationFunction
    Record: NodeRecord
  }

type SensorProperties =
  {
    SyncFunction: SyncFunction
    Record: NodeRecord
  }

type ActuatorProperties =
  {
    OutputHook: OutputHookFunction
    Record: NodeRecord
  }


type NeuronType =
  | Neuron of NeuronProperties
  | Sensor of SensorProperties
  | Actuator of ActuatorProperties

type NeuronActions =
  | Sync
  | ReceiveInput of NeuronConnectionId*Synapse*bool
  | AddOutboundConnection of (MailboxProcessor<NeuronActions>*NeuronId*NeuronLayerId*Weight)*AsyncReplyChannel<unit>
  | AddInboundConnection of NeuronConnectionId*AsyncReplyChannel<unit>
  | GetNodeRecord of AsyncReplyChannel<NodeRecord>
  | Die of AsyncReplyChannel<unit>
  | RegisterCortex of CortexInstance*AsyncReplyChannel<unit>
  | ActivateActuator of AsyncReplyChannel<unit>
  | CheckActuatorStatus of AsyncReplyChannel<bool>

type NeuronInstance = MailboxProcessor<NeuronActions>

type NeuronConnection =
  {
    Neuron: NeuronInstance
    NodeId: NeuronId
    Weight: Weight
  }

type NeuronConnections = Map<NeuronConnectionId,NeuronConnection>
type RecurrentNeuronConnections = Map<NeuronConnectionId,NeuronConnection>
type InboundNeuronConnections = NeuronConnectionId seq

type NeuralNetwork = Map<NeuronId, NeuronLayerId*NeuronInstance>

type Score = float

type ScoreKeeperMsg =
  | Gather of AsyncReplyChannel<unit>*OutputHookId*float
  | GetScore of AsyncReplyChannel<float>
  | KillScoreKeeper of AsyncReplyChannel<unit>

type ScoreKeeperInstance = MailboxProcessor<ScoreKeeperMsg>

type NodeRecordsId = int

type ScoredNodeRecords = array<NodeRecordsId*(Score*NodeRecords)>

type NeuralOutputs = Map<NeuronId, ActuatorOutput>

type FitnessFunction = NodeRecordsId -> NeuralOutputs -> Score

type GenerationRecords = Map<NodeRecordsId, NodeRecords>

type EndOfGenerationFunction = ScoredNodeRecords -> unit

type SyncFunctionSource = NodeRecordsId -> SyncFunction
type SyncFunctionSources = Map<SyncFunctionId, SyncFunctionSource>

type Mutation =
  | MutateActivationFunction
  | AddBias
  | RemoveBias
  | MutateWeights
  // //Choose random neuron, perturb each weight with probability of
  // //1/sqrt(# of weights)
  // //Intensity chosen randomly between -pi/2 and pi/2
  // | ResetWeights
  // //Choose random neuron, reset all weights to random values
  // // ranging between -pi/2 and pi/2
  | AddInboundConnection
  // //Choose a random neuron A, node B, and add a connection
  | AddOutboundConnection
  | AddNeuron
  // //Create a new neuron A, position randomly in NN.
  // //Random Activation Function
  // //Random inbound and outbound
  // | OutSplice
  // | InSplice
  // //Create a new neuron A and sandwich between two nodes
  | AddSensorLink
  | AddActuatorLink
  // | RemoveSensorLink
  // | RemoveActuatorLink
  | AddSensor
  | AddActuator
  // | RemoveInboundConnection
  // | RemoveOutboundConnection
  // | RemoveNeuron
  // //Remove neuron then connect inputs to outputs
  // | DespliceOut
  // | DespliceIn
  // | RemoveSensor
  // | RemoveActuator

type MutationSequence = Mutation seq
type EvolutionProperties =
  {
    MaximumMinds : int
    MaximumThinkCycles : int
    Generations : int
    MutationSequence : MutationSequence 
    FitnessFunction : FitnessFunction
    ActivationFunctions : ActivationFunctions
    SyncFunctionSources : SyncFunctionSources
    OutputHookFunctionIds : OutputHookFunctionIds
    EndOfGenerationFunctionOption : EndOfGenerationFunction option
    StartingRecords : GenerationRecords
  }
