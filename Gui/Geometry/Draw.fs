module Gui.Draw

open Avalonia.Controls.Shapes
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open Geometry

/// Avalonia has no Circle draw object, so to add attributes to the circle,
/// you must use the Ellipse module functions.
let circle (c: Circle2D<'Unit, 'Coordinates>) attr : IView =
    Ellipse.create (
        [ Ellipse.width (c.Radius.value () * 2.)
          Ellipse.height (c.Radius.value () * 2.)
          Ellipse.left ((c.Center.X - c.Radius).value ())
          Ellipse.top ((c.Center.Y - c.Radius).value ()) ]
        @ attr
    )
    :> IView

/// Draw a bounding box. Since this is based off of the Avalonia Rectangle
/// object, you must use that to add new attributes.
let boundingBox (b: BoundingBox2D<'Unit, 'Coordinates>) attr : IView =
    Rectangle.create (
        [ Rectangle.width ((BoundingBox2D.width b).value ())
          Rectangle.height ((BoundingBox2D.height b).value ())
          Rectangle.left (b.MinX.value ())
          Rectangle.top (b.MinY.value ()) ]
        @ attr
    )
    :> IView
