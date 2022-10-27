namespace Gui.DataTypes

open Math.Geometry
open Math.Units

/// A simulation of the Bloom flower. This holds all the information relating
/// to a single flower in the application. This includes information needing
/// to simulate and communicate with the flower as well as the information
/// about how the flower is being used within the application.
type Flower =
    { /// The unique identifier for this flower.
      Id: Flower Id
      /// An easily recognizable name for the flower.
      Name: string
      /// The I2C address that is being used to communicate with the flower.
      I2cAddress: I2cAddress
      /// The position on the screen of the flower.
      Position: Point2D<Meters, ScreenSpace>
      /// The amount that the flower is open. A flower that is 0% open is
      /// fully closed, and a flower that is 100% open is a fully open flower.
      /// This values is always between 0% and 100%.
      OpenPercent: Percent
      /// The target for the flower to move to.
      /// This values is always between 0% and 100%.
      TargetPercent: Percent
      /// Max speed is a positive number representing the maximum magnitude
      /// the speed can go.
      MaxSpeed: AngularSpeed
      /// The current speed of the flower. This speed can be both positive and
      /// negative. A positive
      Speed: AngularSpeed
      /// The pace at which the speed can increase. The acceleration is always
      /// positive and controls. This value represents both the acceleration
      /// and deceleration speed.
      Acceleration: AngularAcceleration
      /// The size that the flower appears on the screen.
      Radius: Length
      /// Shows the connection status of the real world flower.
      ConnectionStatus: ConnectionStatus }
    
/// The flowers have different behaviors that they can take on. The flowers
/// movement is based on their current behavior. If the flower is in the
/// UserControlled mode, then the flower's behavior is based on the commands
/// that the user sends to the flowers individually.
type Behavior =
    | UserControlled
    | Bloom

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

    // ---- Constants ----

    /// The percentage that a flower will open from a single step of the
    /// stepper motor. This is a theoretical approximation of the accuracy
    /// of the stepper in the simulation.
    let stepSize: Percent =
        Angle.steps 1. |> Percent.fromAngle

    /// The percentage that a flower will open from a single microstep of the
    /// stepper motor. This is a theoretical approximation of the accuracy
    /// of the stepper in the simulation.
    let microstepSize: Percent =
        Angle.microsteps 1. |> Percent.fromAngle

    // ---- Builders -----

    /// Create a flower with the default parameters.
    let empty () : Flower =
        { Id = Id.create ()
          Name = ""
          Position = Point2D.origin
          I2cAddress = 0uy
          OpenPercent = Percent.zero
          TargetPercent = Percent.zero
          Speed = AngularSpeed.microstepsPerSecond 0.
          MaxSpeed = AngularSpeed.microstepsPerSecond 10000.
          Acceleration = AngularAcceleration.microstepsPerSecondSquared 2500.
          Radius = Length.cssPixels 20.
          ConnectionStatus = Disconnected }


    /// Create a flower with the basic initialization parameters, the flower
    /// name and I2C address.
    let basic name i2c : Flower =
        { empty () with
            Name = name
            I2cAddress = i2c }

    // ---- Accessors ----

    let name (flower: Flower) : string = flower.Name
    let i2cAddress (flower: Flower) : I2cAddress = flower.I2cAddress
    let position (flower: Flower) : Point2D<Meters, ScreenSpace> = flower.Position
    let openPercent (flower: Flower) : Percent = flower.OpenPercent
    let targetPercent (flower: Flower) : Percent = flower.TargetPercent
    let maxSpeed (flower: Flower) : AngularSpeed = flower.MaxSpeed
    let speed (flower: Flower) : AngularSpeed = flower.Speed
    let acceleration (flower: Flower) : AngularAcceleration = flower.Acceleration

    /// The distance it is going to take the stepper motor to stop given it's
    /// current speed and maximum deceleration.
    let distanceToStop (flower: Flower) : Percent =
        Quantity.squared flower.Speed
        / (2. * flower.Acceleration)
        |> Quantity.asType
        |> Percent.fromAngle

    let circle (flower: Flower) : Circle2D<Meters, ScreenSpace> =
        Circle2D.atPoint flower.Position flower.Radius


    // ---- Modifiers ----

    let setName name flower : Flower = { flower with Name = name }
    let setI2cAddress i2CAddress flower : Flower = { flower with I2cAddress = i2CAddress }
    let setPosition position flower : Flower = { flower with Position = position }

    /// Set current position of the flower. This value is locked between 0% and 100%.
    let setOpenPercent (percent: Percent) (flower: Flower) : Flower =
        if percent <= Percent.zero then
            { flower with
                OpenPercent = Percent.zero
                Speed = AngularSpeed.zero }

        else if percent >= Percent.oneHundred then
            { flower with
                OpenPercent = Percent.oneHundred
                Speed = AngularSpeed.zero }

        else
            { flower with OpenPercent = percent }

    /// Set the desired position to move to. This value is locked between 0% and 100%.
    let setTargetPercent (percent: Percent) (flower: Flower) : Flower =
        { flower with TargetPercent = Percent.clamp Percent.zero Percent.oneHundred percent }

    /// <summary>
    /// Sets the maximum possible speed. If the current speed, <see cref="P:Gui.DataTypes.Flower.Speed"/> is larger than the max speed
    /// being set, the speed is throttled down to the max speed.
    /// </summary>
    let setMaxSpeed maxSpeed flower : Flower =
        { flower with
            Speed = Quantity.min flower.Speed maxSpeed
            MaxSpeed = maxSpeed }

    /// Set the acceleration for this flower. This value is always positive, so
    /// if you enter a negative number, the flower uses the absolute value.
    let setAcceleration acceleration flower : Flower =
        { flower with Acceleration = Quantity.abs acceleration }

    /// <summary>
    /// Set the current speed of the flower. This quantity is limited by the
    /// <see cref="P:Gui.DataTypes.Flower.MaxSpeed"/>. This value stays between
    /// <c> -MaxSpeed </c> and <c> MaxSpeed <c>.
    ///
    /// </summary>
    let setSpeed speed flower : Flower =
        { flower with Speed = Quantity.clamp -flower.MaxSpeed flower.MaxSpeed speed }

    /// Mark the current flower as connected
    let connect flower : Flower =
        { flower with ConnectionStatus = Connected }

    /// Mark the current flower as disconnected
    let disconnect flower : Flower =
        { flower with ConnectionStatus = Disconnected }

    /// <summary>
    /// Update the current position and speed of the flower to simulate it's movement.
    /// </summary>
    ///
    /// <param name="dt">Elapsed time since last update.</param>
    /// <param name="flower">The flower to update</param>
    let tick (dt: Duration) (flower: Flower) : Flower =
        let stopDistance = distanceToStop flower

        let almostStopped =
            Quantity.equalWithin stepSize stopDistance Percent.zero

        // Flower is at target, nothing needs to be done
        if flower.TargetPercent = flower.OpenPercent
           && almostStopped then
            flower

        // Default Updating
        else
            // The distance and direction needed to travel before
            let travelDistance = flower.TargetPercent - flower.OpenPercent
            
            // The quantity of distance remaining until the destination. Always positive
            let distanceToGo: Percent =
                Quantity.abs travelDistance
            
            /// Determines if the flower needs to open or close.
            /// This value is '1' if the flower needs to accelerate
            /// This value is '-1' if the flower needs to decelerate
            let accelerationDirection =
                let speedDirection = AngularSpeed.sign flower.Speed
                let targetDirection = Quantity.compare flower.TargetPercent flower.OpenPercent
                if distanceToGo < stopDistance &&  speedDirection = targetDirection then
                    -targetDirection
                else
                    targetDirection
                    
            let newSpeed: AngularSpeed =
                flower.Acceleration
                |> AngularAcceleration.multiplyBy accelerationDirection
                |> Quantity.for_ dt
                |> AngularSpeed.plus flower.Speed

            let newPosition =
                newSpeed
                |> Quantity.for_ dt
                |> Percent.fromAngle
                |> Percent.plus flower.OpenPercent

            let atTarget =
                Quantity.equalWithin stepSize flower.TargetPercent newPosition

            // Reached target
            // TODO: this doesn't take into account the current motor speed. This assumes instantaneous deceleration
            if almostStopped && atTarget then
                flower
                |> setOpenPercent flower.TargetPercent
                |> setSpeed AngularSpeed.zero

            else
                flower
                |> setOpenPercent newPosition
                |> setSpeed newSpeed




    /// Simulate receiving a command and apply that to the flower.
    let applyCommand (command: Command) (flower: Flower) : Flower =
        match command with
        // Do nothing for these commands
        | NoCommand
        | Setup
        | Ping
        | Home -> flower
        | Open -> setTargetPercent Percent.oneHundred flower
        | Close -> setTargetPercent Percent.zero flower
        | OpenTo percent -> setTargetPercent percent flower
        | MaxSpeed speed -> setMaxSpeed speed flower
        | Acceleration acceleration -> setAcceleration acceleration flower


    // ---- Queries ----

    /// Test to see if the flower with the current size on the screen contains a point.
    let containsPoint (point: Point2D<Meters, ScreenSpace>) (state: Flower) =
        Circle2D.atPoint state.Position state.Radius
        |> Circle2D.containsPoint point

    /// The remaining distance that the flower has to move
    let remainingDistance (flower: Flower) : Percent =
        flower.OpenPercent - flower.TargetPercent
        |> Quantity.abs



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
