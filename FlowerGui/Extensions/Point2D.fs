module Extensions.Point2D

open Geometry

let pixels (x: float) (y: float) : Point2D<Pixels, 'Coordiantes> =
    Point2D.xy (Length.pixels x) (Length.pixels y)
