namespace Gui.DataTypes

open System.Device.I2c
open System.IO.Ports
open System.Threading.Tasks
open Math.Units

open Gui.DataTypes

type Command =
    | NoCommand
    | Setup
    | Home
    | Open
    | Close
    | OpenTo of Percent
    | MaxSpeed of AngularSpeed
    | Acceleration of AngularAcceleration
    | Ping

module Command =

    /// <summary>
    ///   Commands that can be send to the bloom motor controllers.
    ///   for a full list you can check out the documentation for
    ///   <see href="https://github.com/Geil-X/bloom-microcontrollers/wiki/Communication#command-list">
    ///   Microcontroller Commands</see>
    /// </summary>
    type Id =
        | NoCommand = 0uy
        | Setup = 1uy
        | Home = 2uy
        | Open = 3uy
        | Close = 4uy
        | OpenTo = 5uy
        | MaxSpeed = 6uy
        | Acceleration = 7uy
        | Ping = 255uy

    /// Create a binary packet from a command that can be sent over an external
    /// communication channel.
    let private toPacket (command: Command) : byte array =
        match command with
        | NoCommand -> [| byte Id.NoCommand; 0uy; 0uy |]
        | Setup -> [| byte Id.Setup; 0uy; 0uy |]
        | Home -> [| byte Id.Home; 0uy; 0uy |]
        | Open -> [| byte Id.Open; 0uy; 0uy |]
        | Close -> [| byte Id.Close; 0uy; 0uy |]
        | OpenTo percentage -> Array.append [| byte Id.OpenTo |] (Percent.toBytes16 percentage)
        | MaxSpeed speed -> Array.append [| byte Id.MaxSpeed |] (AngularSpeed.inUint16Bytes speed)
        | Acceleration acceleration ->
            Array.append [| byte Id.Acceleration |] (AngularAcceleration.inUint16Bytes acceleration)
        | Ping -> [| byte Id.Ping; 0uy; 0uy |]

    /// Send a command through the serial chanel as a UART communication.
    /// You need to make sure you are passing this through a serial to i2c
    /// converter so that the command makes it to the particular i2c device
    /// you are looking for.
    let sendThroughSerial (serialPort: SerialPort) (addr: I2cAddress) (command: Command) : Task<unit> =
        let packet =
            toPacket command |> Array.append [| addr |]

        task { serialPort.Write(packet, 0, packet.Length) }

    /// Send a command through the devices i2c communication pins. This must
    /// be run on a device that supports i2c communication like the
    /// Raspberry PI. This will not work on a normal PC as they do not expose
    /// their i2c communication channels to external devices.
    let sendToI2c (addr: I2cAddress) (command: Command) : Task<unit> =
        if OS.getOS <> OS.Raspbian then
            failwith $"Cannot send i2c communications on the current os: {OS.getOS}"

        let i2cBus = I2cBus.Create(0)

        let i2cDevice =
            i2cBus.CreateDevice(int addr)

        task { i2cDevice.Write(toPacket command) }
