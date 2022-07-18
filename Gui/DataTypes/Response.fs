module Gui.DataTypes.Response

open Extensions
open Gui
open Gui.Generics

[<Literal>]
let responseSize =  12

let fromPacket (packet: Packet) : Response option =
    if packet.Length <> responseSize then
        None
        
    else
        let time = UInt32.fromBytes packet.[0] packet.[1] packet.[2] packet.[3] |> int
        let position = ClampedPercentage.fromBytes packet.[4] packet.[5]
        let target = ClampedPercentage.fromBytes packet.[6] packet.[7]
        let acceleration = UInt16.fromBytes packet.[8] packet.[9] |> int
        let speed = UInt16.fromBytes packet.[10] packet.[11] |> int
        
        Some
            { Time = time
              Position = position
              Target = target
              Acceleration = acceleration
              MaxSpeed = speed }