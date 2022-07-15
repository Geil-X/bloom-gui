namespace Gui

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
type Choreography =
    | None
    | OpenClose

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
      Speed: RemoteValue<Speed>
      Acceleration: Acceleration
      Radius: Length<Pixels>
      ConnectionStatus: ConnectionStatus }

type Response =
    { Time: Time
      Position: ClampedPercentage
      Target: ClampedPercentage
      Acceleration: Acceleration
      MaxSpeed: Speed }

type Command =
    | NoCommand
    | Setup
    | Home
    | Open
    | Close
    | OpenTo of ClampedPercentage
    | Speed of uint
    | Acceleration of uint

// ---- Actions ----------------------------------------------------------------

type PathName = string

type Action =
    // File Actions
    | NewFile
    | SaveAsDialog of AsyncOperationStatus<unit, exn>
    | SaveAs of AsyncOperationStatus<PathName, Result<unit, exn>>
    | OpenFileDialog of AsyncOperationStatus<unit, string option>
    | OpenFile of AsyncOperationStatus<PathName, Result<Flower seq, exn>>
    | RefreshSerialPorts of AsyncOperationStatus<unit, string list>

    // Flower Actions
    | NewFlower
    | SelectFlower of Flower Id
    | DeselectFlower
    | DeleteFlower
    | SendCommand of AsyncOperationStatus<Command, exn>
    | PingFlower of AsyncOperationStatus<unit, exn>
    | SelectChoreography of Choreography

[<RequireQualifiedAccess>]
type ActionResult =
    | SerialPortOpened of SerialPort
    | SerialPortClosed of SerialPort
    | SerialPortReceivedData of Packet
