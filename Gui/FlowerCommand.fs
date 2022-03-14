module Gui.FlowerCommand

open System.IO

open System.IO.Ports
open System.Threading.Tasks
open Gui.DataTypes


type FlowerCommand =
    | Home
    | Open
    | Close
    | OpenTo of ClampedPercentage

type CommandId =
    | Home = 0u
    | Open = 1u
    | Close = 2u
    | OpenTo = 3u

type internal Packet = byte list

let openSerialPort (port: string) (baud: int) : Task<SerialPort> =
    task {
        let port =
            new SerialPort(port, baud, Parity.None, 8, StopBits.One)

        port.DtrEnable <- true
        port.RtsEnable <- true
        port.ReadTimeout <- 250 //ms
        port.ReadTimeout <- 250 //ms

        port.Open()

        return port
    }

let private packetSize = 2

let private packet (command: FlowerCommand) : Packet =
    match command with
    | Home -> [ byte CommandId.Home ]
    | Open -> [ byte CommandId.Open ]
    | Close -> [ byte CommandId.Close ]
    | OpenTo clampedPercentage ->
        [ byte CommandId.OpenTo
          ClampedPercentage.inByte clampedPercentage ]

let sendCommand (serialPort: SerialPort) (address: I2cAddress) (command: FlowerCommand) : Task<unit> =
    task { serialPort.WriteLine(address :: packet command |> string) }
