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

    open Elmish
    open System.IO.Ports
    open System.Text
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

    type internal Packet = byte array

    let connectToSerialPort (port: string) : Task<SerialPort> =
        task {
            let baud = 115200

            let serialPort =
                new SerialPort(port, baud, Parity.None, 8, StopBits.One)

            serialPort.DtrEnable <- true
            serialPort.RtsEnable <- true
            serialPort.ReadTimeout <- 250 //ms
            serialPort.ReadTimeout <- 250 //ms

            serialPort.Open()

            return serialPort
        }
        
    let getSerialPorts () : Task<string list> =
        task {
            return SerialPort.GetPortNames() |> List.ofArray
        }

    let openSerialport (serialPort: SerialPort) : Task<SerialPort> =
        task {
            serialPort.Open()
            return serialPort
        }

    let closeSerialPort (serialPort: SerialPort) : Task<SerialPort> =
        task {
            serialPort.Close()
            return serialPort
        }

    let onReceived (serialPort: SerialPort) (msg: string -> 'Msg) : Cmd<'Msg> =
        let sub (dispatch: 'Msg -> unit) =
            let handler _ _ =
                let serialString = serialPort.ReadExisting().Trim()

                if serialString <> "" then
                    dispatch (msg serialString)
                else
                    ()

            let receivedEvent = serialPort.DataReceived
            receivedEvent.AddHandler handler

        Cmd.ofSub sub

    let sendCommand (serialPort: SerialPort) (address: I2cAddress) (command: Command) : Task<unit> =
        let packet: Packet =
            match command with
            | NoCommand -> [| byte CommandId.NoCommand; 0uy; 0uy |]
            | Setup -> [| byte CommandId.Setup; 0uy; 0uy |]
            | Home -> [| byte CommandId.Home; 0uy; 0uy |]
            | Open -> [| byte CommandId.Open; 0uy; 0uy |]
            | Close -> [| byte CommandId.Close; 0uy; 0uy |]
            | OpenTo percentage -> Array.append [| byte CommandId.OpenTo |] (ClampedPercentage.toBytes16 percentage)
            | Speed speed -> Array.append [| byte CommandId.Speed |] (uint16 speed |> UInt16.inBytes)
            | Acceleration acceleration ->
                Array.append [| byte CommandId.Acceleration |] (uint16 acceleration |> UInt16.inBytes)
            |> Array.append [| byte address |]

        task { serialPort.Write(Encoding.ASCII.GetString packet) }
