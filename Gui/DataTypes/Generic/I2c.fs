namespace Gui.DataTypes.Generic

module GpioController =
    open System.Device.Gpio

    let controller () = new GpioController()

    let openPin (pin: int) (controller: GpioController) : GpioController =
        controller.OpenPin(pin)
        controller

    let openPinInMode (pin: int) (state: PinMode) (controller: GpioController) : GpioController =
        controller.OpenPin(pin, state)
        controller

    let write (pin: int) (value: PinValue) (controller: GpioController) : GpioController =
        controller.Write(pin, value)
        controller

module I2cBus =
    open System.Device.I2c
    open Iot.Device.Board

    let create (busId: int) : I2cBus = I2cBus.Create(busId)

    let createI2cDevice (deviceAddress: int) (bus: I2cBus) : I2cDevice = bus.CreateDevice(deviceAddress)

    let removeI2cDevice (deviceAddress: int) (bus: I2cBus) : I2cBus =
        bus.RemoveDevice(deviceAddress)
        bus

    /// Get all the I2c Devices connected to the current bus
    let scan (bus: I2cBus) : int list = bus.PerformBusScan() |> List.ofSeq

module I2cDevice =

    open System.Device.I2c

    let create (busId: int) (deviceAddress: int) : I2cDevice =
        I2cDevice.Create(I2cConnectionSettings(busId, deviceAddress))

    let send (bytes: byte array) (device: I2cDevice) : I2cDevice =
        device.Write(bytes)
        device

    let request (bytes: int) (device: I2cDevice) : byte array =
        let mutable buffer = Array.create bytes 0uy
        device.Read(buffer)
        buffer
