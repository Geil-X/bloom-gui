namespace Gui

open System.IO.Ports
open Gui.DataTypes

type Action =
    // File Actions
    | NewFile
    | SaveAsDialog
    | SaveAs of string
    | OpenFileDialog
    | OpenFile of string
    | FileOpened of Flower seq
    | RefreshSerialPorts

    // Flower Actions
    | NewFlower
    | SelectFlower of Flower Id
    | DeselectFlower
    | DeleteFlower
    | SendCommand of Command
    
[<RequireQualifiedAccess>]
type ActionResult =
    | SerialPortOpened of SerialPort
    | SerialPortClosed of SerialPort
    | SerialPortReceivedData of string
    | GotSerialPorts of string list

[<RequireQualifiedAccess>]
type ActionError =
    | ErrorSavingFile of exn
    | ErrorPickingSaveFile
    | ErrorPickingFileToOpen
    | CouldNotOpenFile of exn
    | CouldNotSendCommand of exn

[<RequireQualifiedAccess>]
type Direction =
    | Left
    | Top
    | Right
    | Bottom
