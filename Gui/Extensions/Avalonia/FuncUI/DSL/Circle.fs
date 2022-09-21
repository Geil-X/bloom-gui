namespace Avalonia.FuncUI.DSL

open Avalonia.Controls.Shapes
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open Math.Geometry
open Math.Units

/// This type alias allows for the use of the `Ellipse` class functions from
/// the Circle module.
type Circle = Ellipse

module Circle =

    let create: IAttr<Circle> list -> IView<Circle> =
        Ellipse.create

    let from (circle: Circle2D<Meters, 'Coordinates>) attr : IView<Circle> =
        create (
            [ Circle.width (Length.inCssPixels circle.Radius * 2.)
              Circle.height (Length.inCssPixels circle.Radius * 2.)
              Circle.left (Length.inCssPixels (circle.Center.X - circle.Radius))
              Circle.top (Length.inCssPixels (circle.Center.Y - circle.Radius)) ]
            @ attr
        )
