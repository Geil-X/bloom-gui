namespace Gui.DataTypes

type Command =
    | NoCommand
    | Setup
    | Home
    | Open
    | Close
    | OpenTo of ClampedPercentage
    | Speed of uint
    | Acceleration of uint

module Command =

    open System.IO.Ports
    open System.Threading.Tasks

    open Gui.DataTypes
    open Extensions

    type CommandId =
        | NoCommand = 0uy
        | Setup = 1uy
        | Home = 2uy
        | Open = 3uy
        | Close = 4uy
        | OpenTo = 5uy
        | Speed = 6uy
        | Acceleration = 7uy

    type internal Packet = byte list

    let openSerialPort (port: string) : Task<SerialPort> =
        task {
            let baud = 115200

            let port =
                new SerialPort(port, baud, Parity.None, 8, StopBits.One)

            port.DtrEnable <- true
            port.RtsEnable <- true
            port.ReadTimeout <- 250 //ms
            port.ReadTimeout <- 250 //ms

            port.Open()

            return port
        }

    let private packetSize = 3

    let private packet (command: Command) : Packet =
        match command with
        | NoCommand -> [ byte CommandId.NoCommand; 0uy; 0uy ]
        | Setup -> [ byte CommandId.Setup; 0uy; 0uy ]
        | Home -> [ byte CommandId.Home; 0uy; 0uy ]
        | Open -> [ byte CommandId.Open; 0uy; 0uy ]
        | Close -> [ byte CommandId.Close; 0uy; 0uy ]
        | OpenTo percentage ->
            byte CommandId.OpenTo
            :: ClampedPercentage.toBytes16 percentage
        | Speed speed ->
            byte CommandId.Speed
            :: (uint16 speed |> UInt16.inBytes)
        | Acceleration acceleration ->
            byte CommandId.Acceleration
            :: (uint16 acceleration |> UInt16.inBytes)

    let sendCommand (serialPort: SerialPort) (address: I2cAddress) (command: Command) : Task<unit> =
        task { serialPort.WriteLine(address :: packet command |> string) }
