namespace Evolution

/// The initial model generation
type Initialization<'Model> = unit -> 'Model

/// This function determines how randomness is added to an individual model
type Mutation<'Model> = 'Model -> 'Model

/// How two parent models produce a single child
type Crossover<'Model> = 'Model -> 'Model -> 'Model

type AlgorithmState<'Model> =
    | Uninitialized
    | Initialized of 'Model list

type EvolutionaryAlgorithm<'Model> =
    { Initializer: Initialization<'Model>
      Mutator: Mutation<'Model>
      Crossover: Crossover<'Model>
      AlgorithmState: AlgorithmState<'Model>
      PopulationSize: int
      }
