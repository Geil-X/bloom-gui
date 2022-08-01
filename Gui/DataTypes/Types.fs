namespace Gui

open System.IO
open System.IO.Ports
open Avalonia.Media
open Geometry

open Gui.DataTypes
open Gui.Generics

// ---- Constants --------------------------------------------------------------


type UserSpace = UserSpace

module Constants =
    [<Literal>]
    let CanvasId = "Canvas"

// ---- Generic Types ----------------------------------------------------------

type Speed = int

type Acceleration = int

type I2cAddress = byte

[<RequireQualifiedAccess>]
type Direction =
    | Left
    | Top
    | Right
    | Bottom

// ---- Flower Types -----------------------------------------------------------

type Time = int

type Packet = byte []

type ConnectionStatus =
    | Disconnected
    | Connected

type Flower =
    { Id: Flower Id
      Name: string
      I2cAddress: I2cAddress
      Position: Point2D<Pixels, UserSpace>
      Color: Color
      OpenPercent: ClampedPercentage
      TargetPercent: ClampedPercentage
      MaxSpeed: Speed
      Acceleration: Acceleration
      Radius: Length<Pixels>
      ConnectionStatus: ConnectionStatus }


type Command =
    | NoCommand
    | Setup
    | Home
    | Open
    | Close
    | OpenTo of ClampedPercentage
    | Speed of uint
    | Acceleration of uint
    | Ping

// ---- Actions ----------------------------------------------------------------

// TODO: type SerialPortName = string

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

// ---- Menu Controls ----

type MenuAction<'Msg> = { Name: string; Msg: 'Msg }

type MenuDropdown<'Msg> =
    { Name: string
      Actions: MenuAction<'Msg> list }

[<RequireQualifiedAccess>]
type MenuItem<'Msg> =
    | Action of MenuAction<'Msg>
    | Dropdown of MenuDropdown<'Msg>

type MenuTab<'Msg> =
    { Name: string
      Items: MenuItem<'Msg> list }

type MenuBar<'Msg> = MenuTab<'Msg> list
