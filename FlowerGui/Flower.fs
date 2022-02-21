namespace FlowerGui

open System
open Avalonia.Media
open Avalonia.Controls.Shapes
open Avalonia.FuncUI.DSL

open Geometry

type FlowerId = Guid

type Flower =
    { Id: FlowerId
      Name: string
      I2cAddress: uint
      Position: Point2D<Pixels, UserSpace>
      Color: Color
      Radius: uint }

module Flower =

    // ---- Builders -----

    let basic name =
        { Id = Guid.NewGuid()
          Name = name
          Position = Point2D.origin ()
          I2cAddress = 0u
          Color = Colors.White
          Radius = 10u }

    // ---- Modifiers ----

    let setName name flower : Flower = { flower with Name = name }
    let setI2cAddress i2CAddress flower : Flower = { flower with I2cAddress = i2CAddress }
    let setColor color flower : Flower = { flower with Color = color }
    let setPosition position flower : Flower = { flower with Position = position }


    // ---- Actions ----

    // ---- Drawing ----

    let draw (flower: Flower) =
        let circle =
            Circle2D.atPoint flower.Position (Length.pixels 20.)

        Draw.circle
            circle
            [ Ellipse.fill Theme.palette.primary
              Ellipse.stroke Theme.colors.darkGray
              Ellipse.strokeThickness Theme.drawing.strokeWidth ]
