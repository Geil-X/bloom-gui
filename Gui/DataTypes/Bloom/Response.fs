namespace Gui.DataTypes

open Math.Units


type Response =
    { I2cAddress: I2cAddress
      Time: Time
      Position: Percent
      Target: Percent
      AngularAcceleration: AngularAcceleration
      MaxAngularSpeed: AngularSpeed }

module Response =

    open Extensions
    open Gui.DataTypes

    [<Literal>]
    let responseSize = 13

    let fromPacket (packet: Packet) : Response option =
        if packet.Length <> responseSize then
            None

        else
            let i2cAddress = packet[0]

            let time =
                UInt32.fromBytes packet[1] packet[2] packet[3] packet[4]
                |> int

            let position =
                Percent.fromBytes packet[5] packet[6]

            let target =
                Percent.fromBytes packet[7] packet[8]

            let acceleration =
                UInt16.fromBytes packet[9] packet[10] |> int

            let speed =
                UInt16.fromBytes packet[11] packet[12] |> int

            Some
                { I2cAddress = i2cAddress
                  Time = time
                  Position = position
                  Target = target
                  AngularAcceleration = AngularAcceleration.microstepsPerSecondSquared acceleration
                  MaxAngularSpeed = AngularSpeed.microstepsPerSecond speed }
