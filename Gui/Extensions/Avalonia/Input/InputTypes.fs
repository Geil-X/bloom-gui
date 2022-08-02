module Avalonia.Input.InputTypes

open Avalonia.Input

let isPrimary button : bool = button = MouseButton.Left
let isSecondary button : bool = button = MouseButton.Right
let isMiddleClick button : bool = button = MouseButton.Middle
