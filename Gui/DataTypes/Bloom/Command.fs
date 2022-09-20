namespace Gui.DataTypes

open System.IO.Ports
open System.Threading.Tasks
open Math.Units

open Gui.DataTypes
open Extensions

type Command =
    | NoCommand
    | Setup
    | Home
    | Open
    | Close
    | OpenTo of Percent
    | Speed of uint
    | Acceleration of uint
    | Ping

module Command =
    type Id =
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
            | NoCommand -> [| byte Id.NoCommand; 0uy; 0uy |]
            | Setup -> [| byte Id.Setup; 0uy; 0uy |]
            | Home -> [| byte Id.Home; 0uy; 0uy |]
            | Open -> [| byte Id.Open; 0uy; 0uy |]
            | Close -> [| byte Id.Close; 0uy; 0uy |]
            | OpenTo percentage -> Array.append [| byte Id.OpenTo |] (Percent.toBytes16 percentage)
            | Speed speed -> Array.append [| byte Id.Speed |] (uint16 speed |> UInt16.inBytes)
            | Acceleration acceleration ->
                Array.append [| byte Id.Acceleration |] (uint16 acceleration |> UInt16.inBytes)
            | Ping -> [| byte Id.Ping; 0uy; 0uy |]
            |> Array.append [| address |]

        task { serialPort.Write(packet, 0, packet.Length) }
