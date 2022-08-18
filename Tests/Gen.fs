module Tests.Gen

open System
open Avalonia.Media
open FsCheck

open Gui.DataTypes

let int = Gen.choose (0, Int32.MaxValue)

/// Generates a random number from [0.0, 1.0]
let rand =
    Gen.choose (0, Int32.MaxValue)
    |> Gen.map (fun x -> float x / (float Int32.MaxValue))

let floatBetween low high =
    Gen.map (fun scale -> (low + (high - low)) * scale) rand

let float =
    Arb.generate<NormalFloat> |> Gen.map float


let byte = Arb.generate<Byte>


let color =
    Gen.map4 Color.rgba255 byte byte byte byte


let initializer =
    Gen.arrayOfLength 3 (floatBetween 0. 1.)
    |> Gen.map (fun init -> fun () -> init)


let evolutionaryAlgorithm =
    Gen.map EvolutionaryAlgorithm.withInitialization initializer
    |> Gen.map (
        EvolutionaryAlgorithm.withPopulationSize 5
        >> EvolutionaryAlgorithm.withSurvivorCount 2
        >> EvolutionaryAlgorithm.start
    )


type ArbGui =
    static member Register() = Arb.register<ArbGui> () |> ignore

    static member Color() = Arb.fromGen color

    static member EvolutionaryAlgorithm() = Arb.fromGen evolutionaryAlgorithm
