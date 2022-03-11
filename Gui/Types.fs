namespace Gui

type Direction =
    | Left
    | Top
    | Right
    | Bottom

type UserSpace = UserSpace

module Constants =
    [<Literal>]
    let CanvasId = "Canvas"
