module Avalonia.FuncUI.DSL.Rectangle

open Avalonia.Controls.Shapes
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open Geometry

/// Draw a bounding box. Since this is based off of the Avalonia Rectangle
/// object, you must use that to add new attributes.
let fromBoundingBox (b: BoundingBox2D<'Unit, 'Coordinates>) attr : IView =
    Rectangle.create (
        [ Rectangle.width ((BoundingBox2D.width b).value ())
          Rectangle.height ((BoundingBox2D.height b).value ())
          Rectangle.left (b.MinX.value ())
          Rectangle.top (b.MinY.value ()) ]
        @ attr
    )
    :> IView
