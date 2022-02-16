namespace FlowerGui

open System

open Avalonia.Media
open Geometry

type FlowerId = Guid

type Flower =
    { Id : FlowerId
      Name: string
      I2cAddress: uint
      Position: Point2D
      Color: Color
      Radius: uint }

module Flower =
    
    // ---- Builders -----

    let basic name =
        { Id = Guid.NewGuid()
          Name = name
          Position = Point2D.origin
          I2cAddress = 0u
          Color = Colors.White
          Radius = 10u }

    // ---- Modifiers ----

    let setName name flower : Flower = { flower with Name = name }
    let setI2cAddress i2CAddress flower : Flower = { flower with I2cAddress = i2CAddress }
    let setColor color flower : Flower = { flower with Color = color }

// ---- Actions ----
