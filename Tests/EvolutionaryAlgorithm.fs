module Tests.EvolutionaryAlgorithm

open FsCheck
open FsCheck.NUnit

open Gui.DataTypes
open Tests.Gen

let Setup () = ArbGui.Register()


let advanceGeneration (inputEa: EvolutionaryAlgorithm<'Model>) : EvolutionaryAlgorithm<'Model> =
    let fitnesses =
        List.replicate inputEa.Parameters.PopulationSize 0.5

    List.fold (fun ea fitness -> EvolutionaryAlgorithm.setCurrentIndividualsFitness fitness ea) inputEa fitnesses



[<Property>]
let ``Generation advances`` () =
    Prop.forAll (ArbGui.EvolutionaryAlgorithm()) (fun ea ->
        let nextGen = advanceGeneration ea

        Test.equal 2 nextGen.Generation
        && Test.equal 0 (List.length nextGen.Tested)
        && Test.equal (nextGen.Parameters.PopulationSize - 1) (List.length nextGen.Remaining)

    )
