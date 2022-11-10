module Gui.DataTypes.Bloom.I2c

open Iot.Device.Board
open System.Device.I2c

open Gui.DataTypes

let getConnectedDevices () : I2cAddress list =
    let i2cBus = I2cBus.Create(0)

    I2cBusExtensions.PerformBusScan(i2cBus)
    |> Seq.map byte
    |> List.ofSeq
