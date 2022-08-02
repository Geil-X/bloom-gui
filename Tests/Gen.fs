module Tests.Gen

open System
open FsCheck

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


type ArbGui =
    static member Color() = Arb.fromGen color

    static member Register() = Arb.register<ArbGui> () |> ignore
