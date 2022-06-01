namespace Gui

open System.IO.Ports
open Avalonia.Media
open Geometry
open Gui.DataTypes

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

type Flower =
    { Id: Flower Id
      Name: string
      I2cAddress: I2cAddress
      Position: Point2D<Pixels, UserSpace>
      Color: Color
      OpenPercent: ClampedPercentage
      Speed: Speed
      Acceleration: Acceleration
      Radius: Length<Pixels> }

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
    | SelectChoreography of Choreography

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
