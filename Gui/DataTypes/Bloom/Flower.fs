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
      Speed: AngularSpeed
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

    /// The first 16 Addresses are reserved so the starting address must be the
    /// 17th address.
    let mutable private initialI2cAddress = 16uy

    let basic name : Flower =
        initialI2cAddress <- initialI2cAddress + 1uy

        { Id = Id.create ()
          Name = name
          Position = Point2D.origin
          I2cAddress = initialI2cAddress
          OpenPercent = Percent.zero
          TargetPercent = Percent.zero
          Speed = AngularSpeed.turnsPerSecond 0.
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
    let speed (flower: Flower) : AngularSpeed = flower.Speed
    let acceleration (flower: Flower) : AngularAcceleration = flower.Acceleration

    let circle (flower: Flower) : Circle2D<Meters, ScreenSpace> =
        Circle2D.atPoint flower.Position flower.Radius


    // ---- Modifiers ----

    let setName name flower : Flower = { flower with Name = name }
    let setI2cAddress i2CAddress flower : Flower = { flower with I2cAddress = i2CAddress }
    let setPosition position flower : Flower = { flower with Position = position }
    let setOpenPercent percent flower : Flower = { flower with OpenPercent = percent }
    let setTargetPercent percent flower : Flower = { flower with TargetPercent = percent }

    /// <summary>
    /// Sets the maximum possible speed. If the current speed, <see cref="P:Gui.DataTypes.Flower.Speed"/> is larger than the max speed
    /// being set, the speed is throttled down to the max speed.
    /// </summary>
    let setMaxSpeed maxSpeed flower : Flower =
        { flower with
            Speed = Quantity.min flower.Speed maxSpeed
            MaxSpeed = maxSpeed }

    /// <summary>
    /// Set the current speed of the flower. This quantity is limited by the
    /// <see cref="P:Gui.DataTypes.Flower.MaxSpeed"/>.
    /// </summary>
    let setSpeed speed flower : Flower =
        { flower with Speed = Quantity.clamp Quantity.zero flower.MaxSpeed speed }

    /// Mark the current flower as connected
    let connect flower : Flower =
        { flower with ConnectionStatus = Connected }

    /// Mark the current flower as disconnected
    let disconnect flower : Flower =
        { flower with ConnectionStatus = Disconnected }

    let setAcceleration acceleration flower : Flower =
        { flower with Acceleration = acceleration }

    /// Update the flower position and speed based on the amount of time that has elapsed
    let update (duration: Duration) (flower: Flower) =
        let positionChange =
            Quantity.at flower.MaxSpeed duration

        flower
        |> setMaxSpeed flower.MaxSpeed
        |> setOpenPercent flower.OpenPercent
        |> setAcceleration flower.Acceleration




    // ---- Updating Flower ----

    [<Literal>]
    let private MicroSteps = 16

    type TurnRatio = Quantity<Rate<Percentage, Radians>>

    let private turnRatio: TurnRatio =
        (Percent.percent 100.) |> Quantity.per Angle.turn


    /// <summary>
    /// Update the current position and speed of the flower to simulate it's movement.
    /// </summary>
    ///
    /// <param name="dt">Elapsed time since last update.</param>
    /// <param name="flower">The flower to update</param>
    let tick (dt: Duration) (flower: Flower) : Flower =
        let flowerOpeningChange: Percent =
            dt
            |> Quantity.at flower.Speed
            |> Quantity.at turnRatio

        { flower with
            OpenPercent =
                Percent.clamp
                    Percent.zero
                    (Percent.decimal Percent.maxDecimal)
                    (flower.OpenPercent + flowerOpeningChange)
            Speed = flower.Speed }


    // ---- Queries ----

    /// Test to see if the flower with the current size on the screen contains a point.
    let containsPoint (point: Point2D<Meters, ScreenSpace>) (state: Flower) =
        Circle2D.atPoint state.Position state.Radius
        |> Circle2D.containsPoint point


    // ---- Attributes ----

    // ---- Flowers
    let hovered = Attribute.Hovered
    let pressed = Attribute.Pressed
    let selected = Attribute.Selected
    let dragged = Attribute.Dragged

    // ---- Events
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
