module Tests.Flower

open NUnit.Framework
open FsCheck.NUnit

open Gui.DataTypes
open Math.Geometry
open Math.Units
open Tests.Gen

let Setup () = ArbGui.Register()

let testSetterAndGetter setter getter property =
    Flower.empty ()
    |> setter property
    |> getter
    |> Test.equal property

[<Property>]
let ``Name Property`` (name: string) =
    testSetterAndGetter Flower.setName Flower.name name

[<Property>]
let ``I2cAddress Property`` (i2cAddress: I2cAddress) =
    testSetterAndGetter Flower.setI2cAddress Flower.i2cAddress i2cAddress

[<Property>]
let ``Position Property`` (position: Point2D<Meters, ScreenSpace>) =
    testSetterAndGetter Flower.setPosition Flower.position position

[<Property>]
[<Ignore("Needs unit tests from Math.Units and Math.Geometry")>]
let ``Open Percent Property`` (openPercent: Percent) =
    testSetterAndGetter Flower.setOpenPercent Flower.openPercent openPercent

[<Property>]
[<Ignore("Needs unit tests from Math.Units and Math.Geometry")>]
let ``Target Percent Property`` (targetPercent: Percent) =
    testSetterAndGetter Flower.setTargetPercent Flower.targetPercent targetPercent

[<Property>]
[<Ignore("Needs unit tests from Math.Units and Math.Geometry")>]
let ``Speed Property`` (speed: AngularSpeed) =
    let flower =
        Flower.empty () |> Flower.setSpeed speed

    flower.Speed >= Quantity.zero
    && flower.Speed <= flower.MaxSpeed

[<Property>]
let ``Max Speed Property`` (maxSpeed: AngularSpeed) =
    testSetterAndGetter Flower.setMaxSpeed Flower.maxSpeed maxSpeed

[<Property>]
[<Ignore("Needs unit tests from Math.Units and Math.Geometry")>]
let ``Acceleration Property`` (acceleration: AngularAcceleration) =
    testSetterAndGetter Flower.setAcceleration Flower.acceleration acceleration


type SimulationTest =
    { Name: string
      InitialState: Flower
      Expected: Flower }

let duration = Duration.millisecond

let ``Flower simulation test cases`` =
    let maxSpeed =
        AngularSpeed.microstepsPerSecond 1000.

    let acceleration =
        AngularAcceleration.microstepsPerSecondSquared 1000.

    let initialState =
        Flower.empty ()
        |> Flower.setMaxSpeed maxSpeed
        |> Flower.setAcceleration acceleration

    [ // Flower is initially opening from a closed position
      { Name = "Closed to Open"
        InitialState =
          initialState
          |> Flower.setOpenPercent Percent.zero
          |> Flower.setTargetPercent Percent.oneHundred
        Expected =
          initialState
          |> Flower.setOpenPercent (Angle.microsteps 0.001 |> Percent.fromAngle)
          |> Flower.setTargetPercent Percent.oneHundred
          |> Flower.setSpeed (AngularSpeed.microstepsPerSecond 1.) }

      // Flower is initially closing from an open position
      { Name = "Open to Closed"
        InitialState =
          initialState
          |> Flower.setOpenPercent Percent.oneHundred
          |> Flower.setTargetPercent Percent.zero
        Expected =
          initialState
          |> Flower.setOpenPercent (
              Angle.microsteps 0.001
              |> Percent.fromAngle
              |> (-) Percent.oneHundred
          )
          |> Flower.setTargetPercent Percent.zero
          |> Flower.setSpeed (-AngularSpeed.microstepsPerSecond 1.) }

      ]
    |> List.map (fun testCase ->
        TestCaseData(testCase.InitialState)
            .SetName(testCase.Name)
            .Returns(testCase.Expected))

[<TestCaseSource(nameof ``Flower simulation test cases``)>]
let ``Flower simulation`` (initialState: Flower) : Flower = Flower.tick duration initialState
