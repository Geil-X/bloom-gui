namespace Gui.DataTypes

open Math.Geometry
open Math.Units

type Flower =
    { Id: Flower Id
      Name: string
      I2cAddress: I2cAddress
      Position: Point2D<Meters, ScreenSpace>
      OpenPercent: Percent
      TargetPercent: Percent
      MaxSpeed: AngularSpeed
      Acceleration: AngularAcceleration
      Radius: Length
      ConnectionStatus: ConnectionStatus }

module Flower =
    open Avalonia.Input

    open Gui.DataTypes

    type Id = Flower Id

    type Attribute =
        // Flowers
        | Hovered
        | Selected
        | Pressed
        | Dragged

        // Events
        | OnPointerEnter of (Flower Id * MouseEvent<ScreenSpace> -> unit)
        | OnPointerLeave of (Flower Id * MouseEvent<ScreenSpace> -> unit)
        | OnPointerMoved of (Flower Id * MouseEvent<ScreenSpace> -> unit)
        | OnPointerPressed of (Flower Id * MouseButtonEvent<ScreenSpace> -> unit)
        | OnPointerReleased of (Flower Id * MouseButtonEvent<ScreenSpace> -> unit)


    // ---- Builders -----

    /// The first 8 Addresses are reserved so the starting address must be the
    /// 9th address.
    let mutable private initialI2cAddress = 16uy

    let basic name : Flower =
        initialI2cAddress <- initialI2cAddress + 1uy

        { Id = Id.create ()
          Name = name
          Position = Point2D.origin
          I2cAddress = initialI2cAddress
          OpenPercent = Percent.zero
          TargetPercent = Percent.zero
          MaxSpeed = AngularSpeed.turnsPerSecond 5000.
          Acceleration = AngularAcceleration.turnsPerSecondSquared 1000
          Radius = Length.cssPixels 20.
          ConnectionStatus = Disconnected }

    // ---- Accessors ----

    let name (flower: Flower) : string = flower.Name
    let i2cAddress (flower: Flower) : I2cAddress = flower.I2cAddress
    let position (flower: Flower) : Point2D<Meters, ScreenSpace> = flower.Position
    let openPercent (flower: Flower) : Percent = flower.OpenPercent
    let targetPercent (flower: Flower) : Percent = flower.TargetPercent
    let maxSpeed (flower: Flower) : AngularSpeed = flower.MaxSpeed

    let acceleration (flower: Flower) : AngularAcceleration = flower.Acceleration
    let circle (flower: Flower): Circle2D<Meters, ScreenSpace> = Circle2D.atPoint flower.Position flower.Radius
   

    // ---- Modifiers ----

    let setName name flower : Flower = { flower with Name = name }
    let setI2cAddress i2CAddress flower : Flower = { flower with I2cAddress = i2CAddress }
    let setPosition position flower : Flower = { flower with Position = position }
    let setOpenPercent percent flower : Flower = { flower with OpenPercent = percent }
    let setTargetPercent percent flower : Flower = { flower with TargetPercent = percent }
    let setMaxSpeed speed flower : Flower = { flower with MaxSpeed = speed }

    let connected flower : Flower =
        { flower with ConnectionStatus = Connected }

    let disconnected flower : Flower =
        { flower with ConnectionStatus = Disconnected }

    let setAcceleration acceleration flower : Flower =
        { flower with Acceleration = acceleration }


    // ---- Queries ----

    let containsPoint (point: Point2D<Meters, ScreenSpace>) (state: Flower) =
        Circle2D.atPoint state.Position state.Radius
        |> Circle2D.containsPoint point


    // ---- Attributes ----

    // Flowers
    let hovered = Attribute.Hovered
    let pressed = Attribute.Pressed
    let selected = Attribute.Selected
    let dragged = Attribute.Dragged

    // Events
    let onPointerEnter =
        Attribute.OnPointerEnter

    let onPointerLeave =
        Attribute.OnPointerLeave

    let onPointerPressed =
        Attribute.OnPointerPressed

    let onPointerReleased =
        Attribute.OnPointerReleased

    let onPointerMoved =
        Attribute.OnPointerMoved
