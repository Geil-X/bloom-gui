module Gui.DataTypes.Command

open System.IO.Ports
open System.Threading.Tasks

open Gui
open Gui.DataTypes
open Extensions

type CommandId =
    | NoCommand = 0uy
    | Setup = 1uy
    | Home = 2uy
    | Open = 3uy
    | Close = 4uy
    | OpenTo = 5uy
    | Speed = 6uy
    | Acceleration = 7uy
    | Ping = 255uy

let sendCommand (serialPort: SerialPort) (address: I2cAddress) (command: Command) : Task<unit> =
    let packet: Packet =
        match command with
        | NoCommand -> [| byte CommandId.NoCommand; 0uy; 0uy |]
        | Setup -> [| byte CommandId.Setup; 0uy; 0uy |]
        | Home -> [| byte CommandId.Home; 0uy; 0uy |]
        | Open -> [| byte CommandId.Open; 0uy; 0uy |]
        | Close -> [| byte CommandId.Close; 0uy; 0uy |]
        | OpenTo percentage -> Array.append [| byte CommandId.OpenTo |] (ClampedPercentage.toBytes16 percentage)
        | Speed speed -> Array.append [| byte CommandId.Speed |] (uint16 speed |> UInt16.inBytes)
        | Acceleration acceleration ->
            Array.append [| byte CommandId.Acceleration |] (uint16 acceleration |> UInt16.inBytes)
        | Ping -> [| byte CommandId.Ping; 0uy; 0uy |]
        |> Array.append [| address |]

    task { serialPort.Write(packet, 0, packet.Length) }