namespace Gui.DataTypes

// ---- Types ------------------------------------------------------------------

/// This describes how to create the individuals within the starting population.
type Initialization<'Model> = unit -> 'Model

/// This function determines how randomness is added to an individual model.
type Mutation<'Model> = 'Model -> 'Model

/// How two parent models produce a single child.
type Crossover<'Model> = 'Model -> 'Model -> 'Model

/// The fitness of an individual in the evolutionary algorithm.
type Fitness = float

/// A model that has been evaluated for it's fitness.
type Evaluated<'Model> = 'Model * Fitness

/// All the information that is needed to run an evolutionary algorithm.
type EvolutionaryAlgorithmParameters<'Model> =
    { Initializer: Initialization<'Model>
      Mutator: Mutation<'Model>
      Crossover: Crossover<'Model>
      PopulationSize: int
      Survivors: int
      MutationRate: float }

/// The currently running evolutionary algorithm. This contains all the
/// parameters needed to run the evolutionary algorithm as well as
/// all the individuals currently being evaluated.
type EvolutionaryAlgorithm<'Model> =
    { Parameters: EvolutionaryAlgorithmParameters<'Model>
      Tested: Evaluated<'Model> list
      Current: 'Model
      Remaining: 'Model list
      Generation: int }


/// Helper functions for creating and running an evolutionary algorithm. This
/// module is focused around continuously running the evolutionary algorithm
/// and evaluating a single individual at a time.
module EvolutionaryAlgorithm =

    /// Random number generator for the module. This needs to be declared to
    /// ensure that random numbers are available and don't repeat due to
    /// multiple instantiations of random modules. This can be moved into the
    /// evolutionary algorithm if seeded randomness is something that is
    /// needed.
    let private random = System.Random()


    // ---- Builders ---------------------------------------------------------------

    /// Start the evolutionary algorithm from a set of initial parameters.
    /// This initializes the starting population and allows for individuals
    /// to be tested based on their fitness.
    let start (parameters: EvolutionaryAlgorithmParameters<'Model>) : EvolutionaryAlgorithm<'Model> =
        { Parameters = parameters
          Tested = []
          Current = parameters.Initializer()
          Remaining = List.init (parameters.PopulationSize - 1) (fun _ -> parameters.Initializer())
          Generation = 1 }


    // ---- Accessors --------------------------------------------------------------

    /// Assign an initialization function to the algorithm. This describes how
    /// individuals are created for the initial population. This is the base
    /// function for creating an evolutionary algorithm since it is the only
    /// parameter that doesn't have a reasonable default.
    let withInitialization (initializer: Initialization<'Model>) : EvolutionaryAlgorithmParameters<'Model> =
        { Initializer = initializer
          Mutator = id
          Crossover = (fun a _ -> a)
          PopulationSize = 10
          Survivors = 3
          MutationRate = 0.01 }


    /// Add the mutation function. The mutation function describes how an
    /// individuals genome is slightly randomized after each generation.
    let withMutator
        (mutator: Mutation<'Model>)
        (parameters: EvolutionaryAlgorithmParameters<'Model>)
        : EvolutionaryAlgorithmParameters<'Model> =
        { parameters with Mutator = mutator }


    /// Add the crossover function. The crossover function describes how to
    /// combine two individuals genomes to produce a new offspring genome
    /// that is a combination of the two genomes.
    let withCrossover
        (crossover: Crossover<'Model>)
        (parameters: EvolutionaryAlgorithmParameters<'Model>)
        : EvolutionaryAlgorithmParameters<'Model> =
        { parameters with Crossover = crossover }


    /// Set the population size. The population is limited to a minimum of 1 individual.
    let withPopulationSize
        (populationSize: int)
        (parameters: EvolutionaryAlgorithmParameters<'Model>)
        : EvolutionaryAlgorithmParameters<'Model> =
        { parameters with PopulationSize = max 1 populationSize }

    /// Set the number of individuals that survive to the next generation
    let withSurvivorCount
        (survivors: int)
        (parameters: EvolutionaryAlgorithmParameters<'Model>)
        : EvolutionaryAlgorithmParameters<'Model> =
        { parameters with Survivors = max 0 survivors }


    /// Set the rate at which an allele in a gene is mutated.
    let withMutationRate
        (mutationRate: float)
        (parameters: EvolutionaryAlgorithmParameters<'Model>)
        : EvolutionaryAlgorithmParameters<'Model> =
        { parameters with MutationRate = max mutationRate 0 }


    // ---- Accessors --------------------------------------------------------------

    /// Get the current individual that needs to be tested
    let getCurrentIndividual (ea: EvolutionaryAlgorithm<'Model>) : 'Model = ea.Current


    // ---- Runtime Algorithm ------------------------------------------------------


    /// Select individuals whose genetic information is going to be preserved.
    /// This function keeps the half of the population that is the most fit.
    let survivorSelection (survivors: int) (individuals: Evaluated<'Model> list) : 'Model list =
        List.sortBy snd individuals
        |> List.map fst
        |> List.take survivors


    /// From a list of individuals, perform crossover mutation. This creates a
    /// new population of individuals based on the genetic information of
    /// randomly selected individuals from the parent generation. The parents
    /// are chosen randomly weighted by the fitness of those individuals.
    /// The number of individuals that are returns is the population size minus
    /// the number of individuals that survive from the previous generation.
    let performCrossover
        (parameters: EvolutionaryAlgorithmParameters<'Model>)
        (individuals: Evaluated<'Model> list)
        : 'Model list =

        let totalFitness =
            List.map snd individuals |> List.sum

        let sortedAndNormalizedIndividuals =
            if totalFitness > 0 then
                List.sortBy snd individuals
                |> List.map (fun (model, fitness) -> model, fitness / totalFitness)
            else
                individuals

        /// Get a random individual weighted by it's fitness value. If getting
        /// an individual by it's weighted values doesn't work, just select a
        /// random individual.
        let randomIndividual () : 'Model =
            let r = random.NextDouble()

            let maybeIndividual =
                List.tryFind (fun (_, normalizedFitness) -> normalizedFitness > r) sortedAndNormalizedIndividuals

            match maybeIndividual with
            | Some individual -> individual |> fst
            | None ->
                let index =
                    random.NextInt64(List.length individuals) |> int

                List.item index individuals |> fst

        let numChildren =
            max (parameters.PopulationSize - parameters.Survivors) 0

        List.init numChildren (fun _ -> parameters.Crossover (randomIndividual ()) (randomIndividual ()))

    let performMutation (mutator: Mutation<'Model>) (individuals: 'Model list) : 'Model list =
        List.map mutator individuals


    /// When the fitness of all the individuals are tested, the next generation
    /// can be created from the previous generation. The next generation is
    /// created based off the mutation and crossover functions that are provided
    /// to the algorithm at initialization.
    let nextGeneration
        (currentGeneration: int)
        (individuals: Evaluated<'Model> list)
        (parameters: EvolutionaryAlgorithmParameters<'Model>)
        : EvolutionaryAlgorithm<'Model> =

        let survivors =
            survivorSelection parameters.Survivors individuals

        let children =
            performCrossover parameters individuals

        let nextGenerationIndividuals =
            survivors @ children
            |> performMutation parameters.Mutator

        match nextGenerationIndividuals with
        | first :: rest ->
            { Parameters = parameters
              Tested = []
              Current = first
              Remaining = rest
              Generation = currentGeneration + 1 }
        | [] -> failwith "The evolutionary algorithm failed to produce another generation with any individuals"


    /// Assign the fitness of the current individual and roll the algorithm to
    /// the next individual.
    ///
    /// Since this algorithm is designed to be continuously running, this
    /// function runs much of the heavy lifting under the covers. This function
    /// assigns the current individual's fitness and sets the next individual
    /// to be the new individual for testing. If all the individuals of the
    /// current generation have been tested, the algorithm moves the population
    /// into the next generation and starts testing on those individuals.
    let setCurrentIndividualsFitness
        (fitness: float)
        (ea: EvolutionaryAlgorithm<'Model>)
        : EvolutionaryAlgorithm<'Model> =
        match ea.Remaining with
        | next :: rest ->
            { ea with
                Tested = List.append ea.Tested [ ea.Current, fitness ]
                Current = next

                Remaining = rest }

        | [] ->
            let evaluatedIndividuals: Evaluated<'Model> list =
                (ea.Current, fitness) :: ea.Tested

            nextGeneration ea.Generation evaluatedIndividuals ea.Parameters
