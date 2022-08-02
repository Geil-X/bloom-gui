namespace Avalonia.FuncUI.DSL

open Avalonia.Controls.Shapes
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open Geometry

/// This type alias allows for the use of the `Ellipse` class functions from
/// the Circle module.
type Circle = Ellipse

module Circle =

    let create = Ellipse.create

    let from (circle: Circle2D<'Unit, 'Coordinates>) attr : IView =
        create (
            [ Circle.width (circle.Radius.value () * 2.)
              Circle.height (circle.Radius.value () * 2.)
              Circle.left ((circle.Center.X - circle.Radius).value ())
              Circle.top ((circle.Center.Y - circle.Radius).value ()) ]
            @ attr
        )
        :> IView
