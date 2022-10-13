namespace Math.Units

open Extensions

[<AutoOpen>]
module Extensions =
    type Quantity<'Units> with
        static member asType(q: Quantity<'UnitB>) : Quantity<'UnitA> = Quantity.create<'UnitA> q.Value

        /// Takes a number and returns 1 if the number is zero or positive
        /// and -1 if the number is negative.
        static member sign(x: Quantity<'Units>) : int = if x.Value >= 0 then 1 else 0


module Constants =
    open Math.Units


    /// The number of rotations needed to complete one bloom cycle.
    type TurnRatio = Quantity<Rate<Radians, Percentage>>

    /// The number of microsteps used by the stepper controller
    [<Literal>]
    let private Microsteps = 16.

    /// The number of steps it takes the stepper motor to make one revolution
    [<Literal>]
    let private Steps = 200.

    let StepsPerRevolution = Microsteps * Steps

    let MicrostepsPerRevolution =
        Microsteps * StepsPerRevolution

    /// The number of steps in the full cycle of the flower. That is the distance
    /// from the open to closed state in the flower in microsteps.
    [<Literal>]
    let BloomRange = 32000.

    /// The number of revolutions the stepper motor shaft needs to go to
    /// complete one bloom open to close cycle.
    let private RevolutionsPerBloom: Angle =
        Angle.turns BloomRange
        / (StepsPerRevolution * Microsteps)


    /// The turn ratio of the stepper motor. This is the number of steps
    /// including microsteps
    let BloomTurnRatio: TurnRatio =
        RevolutionsPerBloom
        |> Quantity.per (Percent.create 1.)

module AngularAcceleration =
    let microstepsPerSecondSquared (accel: float) : AngularAcceleration =
        accel / Constants.MicrostepsPerRevolution
        |> AngularAcceleration.turnsPerSecondSquared

    let inMicrostepsPerSecondSquared (accel: AngularAcceleration) : float =
        AngularAcceleration.inTurnsPerSecondSquared accel
        * Constants.MicrostepsPerRevolution

    let inUint16Bytes (accel: AngularAcceleration) =
        inMicrostepsPerSecondSquared accel
        |> uint16
        |> UInt16.inBytes

    let fromUint16Bytes (first: byte) (second: byte) : AngularAcceleration =
        UInt16.fromBytes first second
        |> float
        |> microstepsPerSecondSquared


module AngularSpeed =
    let microstepsPerSecond (speed: float) : AngularSpeed =
        speed / Constants.MicrostepsPerRevolution
        |> AngularSpeed.turnsPerSecond

    let inMicrostepsPerSecond (speed: AngularSpeed) : float =
        AngularSpeed.inTurnsPerSecond speed
        * Constants.MicrostepsPerRevolution

    let inUint16Bytes (speed: AngularSpeed) =
        inMicrostepsPerSecond speed
        |> uint16
        |> UInt16.inBytes

    let fromUint16Bytes (first: byte) (second: byte) : AngularSpeed =
        UInt16.fromBytes first second
        |> float
        |> microstepsPerSecond

module Angle =
    let steps (steps: float) : Angle =
        (steps * Angle.turn)
        / Constants.StepsPerRevolution

    let inSteps (angle: Angle) : float =
        (angle / Angle.turn)
        * Constants.StepsPerRevolution

    let microsteps (microsteps: float) : Angle =
        (microsteps * Angle.turn)
        / Constants.MicrostepsPerRevolution

    let inMicrosteps (angle: Angle) : float =
        (angle / Angle.turn)
        * Constants.MicrostepsPerRevolution

module Percent =

    open System
    open Math.Units

    // ---- Constants ----

    [<Literal>]
    let minimum = 0.

    [<Literal>]
    let maxPercentage = 100.

    [<Literal>]
    let maxDecimal = 1.

    // ---- Builders ----


    /// Create a clamped percentage in the range 0 to 1
    let decimal (n: float) : Percent =
        n |> max minimum |> min maxDecimal |> Percent

    /// Create a clamped percentage in the range 0 to 100
    let percent (n: float) : Percent = n / maxPercentage |> decimal

    let zero = percent minimum
    let oneHundred = percent maxPercentage


    // ---- Accessors ----

    /// Get the percentage out of 1
    let inDecimal (p: Percent) : float = p.Value

    /// Get the percentage out of 100
    let inPercentage (p: Percent) : float = p.Value * maxPercentage

    let fromAngle (angle: Angle) : Percent =
        angle |> Quantity.at_ Constants.BloomTurnRatio

    let toAngle (percent: Percent) : Angle =
        percent |> Quantity.at Constants.BloomTurnRatio

    /// Get the percentage as a range from 0 to 255. This spreads out a
    /// percentage over the whole range of an 8 bit unsigned number.
    let toByte (p: Percent) : byte =
        Percent.unwrap p * float Byte.MaxValue |> byte

    /// Get the percentage as a range from 0 to 65536. This spreads out a
    /// percentage over the whole range of an 16 bit unsigned number.
    let toUint16 (p: Percent) : uint16 =
        Percent.unwrap p * float UInt16.MaxValue |> uint16

    /// Get the percentage as a range from 0 to 65536. This spreads out a
    /// percentage over the whole range of an 16 bit unsigned number.
    let toBytes16 (p: Percent) : Byte [] = toUint16 p |> UInt16.inBytes

    let fromBytes (first: byte) (second: byte) : Percent =
        float (UInt16.fromBytes first second)
        / float UInt16.MaxValue
        |> decimal
