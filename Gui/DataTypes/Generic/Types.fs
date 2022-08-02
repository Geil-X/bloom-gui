namespace Gui.DataTypes

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

type ConnectionStatus =
    | Disconnected
    | Connected

// ---- Actions ----------------------------------------------------------------

// TODO: type SerialPortName = string


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
