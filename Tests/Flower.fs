module Tests.Flower

open FsCheck.NUnit

open Gui.DataTypes
open Math.Geometry
open Math.Units
open Tests.Gen

let Setup () = ArbGui.Register()

let testSetterAndGetter setter getter property =
    Flower.empty
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
let ``Open Percent Property`` (openPercent: Percent) =
    testSetterAndGetter Flower.setOpenPercent Flower.openPercent openPercent

[<Property>]
let ``Target Percent Property`` (targetPercent: Percent) =
    testSetterAndGetter Flower.setTargetPercent Flower.targetPercent targetPercent

[<Property>]
let ``Speed Property`` (speed: AngularSpeed) =
    let flower =
        Flower.empty
        |> Flower.setSpeed speed
        
    flower.Speed >= Quantity.zero && flower.Speed <= flower.MaxSpeed

[<Property>]
let ``Max Speed Property`` (maxSpeed: AngularSpeed) =
    testSetterAndGetter Flower.setMaxSpeed Flower.maxSpeed maxSpeed

[<Property>]
let ``Acceleration Property`` (acceleration: AngularAcceleration) =
    testSetterAndGetter Flower.setAcceleration Flower.acceleration acceleration
