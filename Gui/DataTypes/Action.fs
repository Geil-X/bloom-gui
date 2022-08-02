namespace Gui.DataTypes

open System.IO.Ports
open System.IO
open Elmish

open Gui.DataTypes

type Action =
    // File Actions
    | NewFile
    | SaveAsDialog of AsyncOperationStatus<unit, exn>
    | SaveAs of AsyncOperationStatus<FileInfo, Result<FileInfo, File.WriteError>>
    | OpenFileDialog of AsyncOperationStatus<unit, FileInfo seq>
    | OpenFile of AsyncOperationStatus<FileInfo, Result<Flower list, File.ReadError>>

    // Serial Port Actions
    | RefreshSerialPorts of AsyncOperationStatus<unit, string list>
    | ConnectAndOpenPort of AsyncOperationStatus<string, SerialPort>
    | OpenSerialPort of AsyncOperationStatus<SerialPort, SerialPort>
    | CloseSerialPort of AsyncOperationStatus<SerialPort, SerialPort>
    | ReceivedDataFromSerialPort of Packet

    // Flower Actions
    | NewFlower
    | SelectFlower of Flower Id
    | DeselectFlower
    | DeleteFlower
    | SendCommand of AsyncOperationStatus<Command, exn>
    | PingFlower of AsyncOperationStatus<unit, exn>

module Action = ()