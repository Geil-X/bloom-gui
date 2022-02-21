module FlowerGui.Draw

open Avalonia.Controls.Shapes
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open Geometry

/// Avalonia has no Circle draw object, so to add attributes to the circle,
/// you must use the Ellipse module functions.
let circle (c: Circle2D<'Unit, 'Coordinates>) attr : IView =
    Ellipse.create (
        [ Ellipse.width (c.Radius.value ())
          Ellipse.height (c.Radius.value ())
          Ellipse.left ((c.Center.X - c.Radius / 2.).value ())
          Ellipse.top ((c.Center.Y - c.Radius / 2.).value ()) ]
        @ attr
    )
    :> IView
