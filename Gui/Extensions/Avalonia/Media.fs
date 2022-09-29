namespace Avalonia.Media

open Avalonia
open Avalonia.Media

module QuadraticBezierSegment =
    let create p1 p2 =
        let qbs = QuadraticBezierSegment()
        qbs.Point1 <- p1
        qbs.Point2 <- p2

        qbs

module LineSegment =
    let create x y =
        let ls = LineSegment()
        ls.Point <- Point(x, y)

module RotateTransform =
    let inDegrees degrees = RotateTransform(degrees)

module TranslateTransform =
    open Math.Geometry
    open Math.Units

    let inCssPixels (translation: Vector2D<Meters, 'Coordinates>) : TranslateTransform =
        TranslateTransform(Length.inCssPixels translation.X, Length.inCssPixels translation.Y)
