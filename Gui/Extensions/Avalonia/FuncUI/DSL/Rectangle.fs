module Avalonia.FuncUI.DSL.Rectangle

open Avalonia.Controls.Shapes
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open Math.Geometry
open Math.Units

/// Draw a bounding box. Since this is based off of the Avalonia Rectangle
/// object, you must use that to add new attributes.
let fromBoundingBox (b: BoundingBox2D<Meters, 'Coordinates>) attr : IView =
    Rectangle.create (
        [ Rectangle.width
          <| Length.inCssPixels (BoundingBox2D.width b)

          Rectangle.height
          <| Length.inCssPixels (BoundingBox2D.height b)

          Rectangle.left <| Length.inCssPixels b.MinX
          Rectangle.top <| Length.inCssPixels b.MinY ]

        @ attr
    )
    :> IView
