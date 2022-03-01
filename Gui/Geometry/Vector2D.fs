module Gui.Vector2D

open Geometry

let pixels x y =
    Vector2D.xy (Length.pixels x) (Length.pixels y)
