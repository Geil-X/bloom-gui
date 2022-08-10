module Evolution.EvolutionaryAlgorithm


let withInitialization (initializer: Initialization<'Model>) : EvolutionaryAlgorithm<'Model> =
    { Initializer = initializer
      Mutator = id
      Crossover = (fun a _ -> a)
      AlgorithmState = Uninitialized
      PopulationSize = 10 }

let withMutator (mutator: Mutation<'Model>) (ea: EvolutionaryAlgorithm<'Model>) : EvolutionaryAlgorithm<'Model> =
    { ea with Mutator = mutator }

let withCrossover (crossover: Crossover<'Model>) (ea: EvolutionaryAlgorithm<'Model>) : EvolutionaryAlgorithm<'Model> =
    { ea with Crossover = crossover }

let withPopulationSize (populationSize: int) (ea: EvolutionaryAlgorithm<'Model>) : EvolutionaryAlgorithm<'Model> =
    { ea with PopulationSize = populationSize }

let init (ea: EvolutionaryAlgorithm<'Model>) : EvolutionaryAlgorithm<'Model> =
    { ea with AlgorithmState = Initialized(List.init ea.PopulationSize (fun _ -> ea.Initializer())) }
