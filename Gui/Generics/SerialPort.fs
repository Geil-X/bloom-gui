namespace Gui.Generics

type Packet = byte array

module SerialPort =
    open Elmish
    open System.IO.Ports
    open System.Threading.Tasks
    open System.Text

    open Extensions

    let connect (port: string) : Task<SerialPort> =
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

    let getPorts () : Task<string list> =
        task { return SerialPort.GetPortNames() |> List.ofArray }

    let openPort (serialPort: SerialPort) : Task<SerialPort> =
        task {
            serialPort.Open()
            return serialPort
        }

    let connectAndOpen (port: string) : Task<SerialPort> = connect port |> Task.bind openPort

    let closePort (serialPort: SerialPort) : Task<SerialPort> =
        task {
            serialPort.Close()
            return serialPort
        }

    let onReceived (msg: Packet -> 'Msg) (serialPort: SerialPort) : Cmd<'Msg> =
        let sub (dispatch: 'Msg -> unit) =
            let handler _ _ : unit =
                if not serialPort.IsOpen then
                    ()
                else
                    let serialString = serialPort.ReadExisting()

                    if serialString = "" then
                        ()

                    else
                        let packet =
                            Encoding.ASCII.GetBytes serialString

                        dispatch (msg packet)

            let receivedEvent = serialPort.DataReceived
            receivedEvent.AddHandler handler

        Cmd.ofSub sub

    let send (packet: Packet) (serialPort: SerialPort) : Task<unit> =
        task { serialPort.Write(packet, 0, packet.Length) }
