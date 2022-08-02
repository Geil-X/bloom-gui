module Gui.Shell

open System
open System.IO
open System.IO.Ports
open Avalonia.Controls
open Avalonia.Input
open Elmish

open Extensions
open Geometry
open Gui
open Gui.DataTypes
open Gui.DataTypes
open Gui.Panels
open Gui.Views
open Gui.Views.Menu


// ---- States ----

type Tab =
    | Simulation
    | Inputs

type State =
    { CanvasSize: Size<Pixels>
      Flowers: Map<Flower Id, Flower>
      FlowerInteraction: FlowerInteraction
      Selected: Flower Id option
      SerialPort: SerialPort option
      SerialPorts: string list
      Rerender: int
      Tab: Tab
      AppConfig: AppConfig }

and FlowerInteraction =
    | Hovering of Flower Id
    | Pressing of PressedData
    | Dragging of DraggingData
    | NoInteraction

and PressedData =
    { Id: Flower Id
      MousePressedLocation: Point2D<Pixels, UserSpace>
      InitialFlowerPosition: Point2D<Pixels, UserSpace> }

and DraggingData =
    { Id: Flower Id
      DraggingDelta: Vector2D<Pixels, UserSpace> }


// ---- Messaging ----

[<RequireQualifiedAccess>]
type BackgroundEvent = OnReleased of MouseButtonEvent<Pixels, UserSpace>

[<RequireQualifiedAccess>]
type public FlowerPointerEvent =
    | OnEnter of Flower Id * MouseEvent<Pixels, UserSpace>
    | OnLeave of Flower Id * MouseEvent<Pixels, UserSpace>
    | OnMoved of Flower Id * MouseEvent<Pixels, UserSpace>
    | OnPressed of Flower Id * MouseButtonEvent<Pixels, UserSpace>
    | OnReleased of Flower Id * MouseButtonEvent<Pixels, UserSpace>

type SimulationEvent =
    | BackgroundEvent of BackgroundEvent
    | FlowerEvent of FlowerPointerEvent

type Msg =
    // Shell Messages
    | Action of Action
    | RerenderView
    | ReadAppConfig of Result<AppConfig, File.ReadError>
    | WroteAppConfig of Result<FileInfo, File.WriteError>

    // Msg Mapping
    | SimulationEvent of SimulationEvent
    | MenuMsg of Menu.Msg
    | IconDockMsg of IconDock.Msg
    | FlowerPropertiesMsg of FlowerProperties.Msg
    | FlowerCommandsMsg of FlowerCommands.Msg

// ---- High Level Key Handling ------------------------------------------------

let keyUpHandler (window: Window) _ =
    let sub dispatch =
        window.KeyUp.Add (fun eventArgs ->
            match eventArgs.Key with
            | Key.Escape -> Action.DeselectFlower |> Action |> dispatch
            | Key.Delete -> Action.DeleteFlower |> Action |> dispatch
            | _ -> ())

    Cmd.ofSub sub

// ---- File Writing -----------------------------------------------------------

let saveAppConfigFile (appConfig: AppConfig) : Cmd<Msg> =
    File.write AppConfig.configPath appConfig WroteAppConfig

let loadAppConfigFile: Cmd<Msg> =
    File.read AppConfig.configPath ReadAppConfig


// ---- Init -------------------------------------------------------------------

let init () : State * Cmd<Msg> =
    { CanvasSize = Size.create Length.zero Length.zero
      Flowers = Map.empty
      FlowerInteraction = NoInteraction
      Selected = None
      SerialPort = None
      SerialPorts = []
      Rerender = 0
      Tab = Simulation
      AppConfig = AppConfig.init },
    Cmd.batch [
        loadAppConfigFile
        Cmd.ofMsg (Start() |> Action.RefreshSerialPorts |> Action)
    ]

// ---- Update helper functions ------------------------------------------------

let private minMouseMovement =
    Length.pixels 10.

let private minMouseMovementSquared =
    Length.square minMouseMovement

// ---- Serial Port Updates ----------------------------------------------------

/// Open up a connected serial port
let private openSerialPort (serialPort: SerialPort) : Cmd<Msg> =
    Cmd.OfTask.perform SerialPort.openPort serialPort (Finished >> Action.OpenSerialPort >> Action)

/// Close a connected serial port
let private closeSerialPort (serialPort: SerialPort) : Cmd<Msg> =
    Cmd.OfTask.perform SerialPort.closePort serialPort (Finished >> Action.CloseSerialPort >> Action)

/// Connect to a serial port and open it up for communication.
let private connectAndOpenSerialPort (newPortName: string) : Cmd<Msg> =
    Cmd.OfTask.perform SerialPort.connectAndOpenPort newPortName (Finished >> Action.OpenSerialPort >> Action)

/// Open up a new serial port. If there is a serial port currently opened, it
/// is closed and disconnected. If no port is selected, then the current serial
/// port is disconnected.
///
/// Note: This is generally the function that you want to use to connect to a
/// new serial port. This function handles change in program as well as
/// dispatching connection commands.
let private connectToSerialPort (newPortName: string) (state: State) : State * Cmd<Msg> =
    match state.SerialPort with
    | Some serialPort when
        newPortName = FlowerCommands.noPort
        || String.IsNullOrEmpty newPortName
        ->
        Log.debug $"Disconnecting from serial port '{serialPort.PortName}'"
        { state with SerialPort = None }, closeSerialPort serialPort

    | Some serialPort ->
        Log.debug $"Changing from serial port '{serialPort.PortName}' to '{newPortName}'"

        state,
        Cmd.batch [
            closeSerialPort serialPort
            connectAndOpenSerialPort newPortName
        ]

    // Nothing needs to be done in this case
    | None when
        newPortName = FlowerCommands.noPort
        || String.IsNullOrEmpty newPortName
        ->
        state, Cmd.none

    | None ->
        Log.debug $"Connecting to serial port '{newPortName}'"
        state, connectAndOpenSerialPort newPortName

// ---- Flower Functions -------------------------------------------------------

let private getFlower id flowers : Flower option = Map.tryFind id flowers

let private sendCommandTo (serialPort: SerialPort) (i2cAddress: I2cAddress) (command: Command) =
    Cmd.OfTask.attempt (Command.sendCommand serialPort i2cAddress) command (Finished >> SendCommand >> Action)

let private sendCommandToSelected (command: Command) (state: State) : Cmd<Msg> =
    let selectedFlowerOption =
        Option.bind (fun flowerId -> getFlower flowerId state.Flowers) state.Selected

    match state.SerialPort, selectedFlowerOption with
    | Some serialPort, Some flower ->
        Log.debug $"Sending command through serial port '{command}'"

        Cmd.OfTask.attempt
            (Command.sendCommand serialPort flower.I2cAddress)
            command
            (Finished >> SendCommand >> Action)

    | None, Some _ ->
        Log.warning "Serial port is not selected, cannot send command."
        Cmd.none

    | Some _, None ->
        Log.warning "Flower is not selected, cannot send command."
        Cmd.none

    | None, None ->
        Log.error "An unknown error occured when trying to send command."
        Cmd.none

let private pingFlower (serialPort: SerialPort) (flower: Flower) : Cmd<Msg> =
    sendCommandTo serialPort flower.I2cAddress Ping

let private pingAllFlowers (serialPort: SerialPort) (flowers: Map<Flower Id, Flower>) : Cmd<Msg> =
    let cmds =
        Seq.map (pingFlower serialPort) (Map.values flowers)

    Cmd.batch cmds

let private newFile (state: State) (flowers: Flower seq) : State =
    let flowerMap =
        Seq.fold (fun map (flower: Flower) -> Map.add flower.Id flower map) Map.empty flowers

    { state with
        Flowers = flowerMap
        FlowerInteraction = NoInteraction
        Selected = None }

let addFlower (flower: Flower) (state: State) : State =
    { state with Flowers = Map.add flower.Id flower state.Flowers }

let addFlowers (flowers: Flower seq) (state: State) : State =
    let flowerMap =
        Seq.fold (fun map (flower: Flower) -> Map.add flower.Id flower map) Map.empty flowers

    { state with Flowers = flowerMap }

let addNewFlower (state: State) : State * Cmd<Msg> =
    let flower =
        Flower.basic $"Flower {Map.count state.Flowers + 1}"
        |> Flower.setPosition (Point2D.pixels 100. 100.)

    let requestCmd =
        match state.SerialPort with
        | Some serialPort -> pingFlower serialPort flower
        | None -> Cmd.none

    addFlower flower state, requestCmd

let private updateFlower
    (id: Flower Id)
    (property: string)
    (f: 'a -> Flower -> Flower)
    (value: 'a)
    (state: State)
    : State =
    if Option.contains id state.Selected then
        Log.verbose $"Updated flower '{Id.shortName id}' with new {property} '{value}'"

        { state with Flowers = Map.update id (f value) state.Flowers }
    else
        state

let private flowersFromI2cAddress (i2cAddress: I2cAddress) (flowers: Map<Flower Id, Flower>) : Flower seq =
    Map.filter (fun _ flower -> Flower.i2cAddress flower = i2cAddress) flowers
    |> Map.values

let updateFlowerFromResponse (response: Response) (state: State) : State =
    let flowersToUpdate =
        flowersFromI2cAddress response.I2cAddress state.Flowers

    let updateFromResponse =
        Flower.connected
        >> Flower.setOpenPercent response.Position
        >> Flower.setTargetPercent response.Position
        >> Flower.setMaxSpeed response.MaxSpeed
        >> Flower.setAcceleration response.Acceleration

    let updatedFlowerMap =
        Seq.fold
            (fun flowerMap (flower: Flower) -> Map.update flower.Id updateFromResponse flowerMap)
            state.Flowers
            flowersToUpdate

    { state with Flowers = updatedFlowerMap }

// ---- Update -----------------------------------------------------------------

let private updateAction (action: Action) (state: State) (window: Window) : State * Cmd<Msg> =
    match action with
    // ---- File Actions ----

    | Action.NewFile -> newFile state Seq.empty, Cmd.none


    | Action.SaveAsDialog asyncOperation ->
        match asyncOperation with
        | Start _ ->
            state,
            Cmd.OfTask.either
                FlowerFile.saveFileDialog
                window
                (Start >> Action.SaveAs >> Action)
                (Finished >> Action.SaveAsDialog >> Action)

        | Finished exn ->
            Log.error $"Encountered an error when trying to save file{Environment.NewLine}{exn}"
            state, Cmd.none


    | Action.SaveAs asyncOperation ->
        match asyncOperation with
        | Start fileInfo ->
            let flowerFileData: Flower list =
                Map.values state.Flowers
                |> Seq.toList

            state, File.write fileInfo flowerFileData (Finished >> Action.SaveAs >> Action)

        | Finished (Ok fileInfo) ->
            Log.info $"Saved flower file {fileInfo.Name}"

            let newAppConfig =
                AppConfig.addRecentFile fileInfo state.AppConfig

            { state with AppConfig = newAppConfig }, saveAppConfigFile newAppConfig

        | Finished (Error fileWriteError) ->
            Log.error $"Could not save file{Environment.NewLine}{fileWriteError}"
            state, Cmd.none


    | Action.OpenFileDialog asyncOperation ->
        match asyncOperation with
        | Start _ ->
            state, Cmd.OfTask.perform FlowerFile.openFileDialog window (Finished >> Action.OpenFileDialog >> Action)

        | Finished files ->
            match Seq.tryHead files with
            | Some firstFile -> state, Cmd.ofMsg (Start firstFile |> Action.OpenFile |> Action)

            // No file was selected
            | _ -> state, Cmd.none


    | Action.OpenFile asyncOperation ->
        match asyncOperation with
        | Start path -> state, File.read path (Finished >> Action.OpenFile >> Action)

        | Finished fileResult ->
            match fileResult with
            | Ok flowers -> newFile state flowers, Cmd.none
            | Error readingError ->
                Log.error $"Could not read file{Environment.NewLine}{readingError}"
                state, Cmd.none


    // ---- Serial Port Actions ----

    | RefreshSerialPorts asyncOperation ->
        match asyncOperation with
        | Start _ -> state, Cmd.OfTask.perform SerialPort.getPorts () (Finished >> Action.RefreshSerialPorts >> Action)
        | Finished serialPorts -> { state with SerialPorts = serialPorts }, Cmd.none


    | Action.ConnectAndOpenPort asyncOperation ->
        match asyncOperation with
        | Start portName -> connectToSerialPort portName state
        | Finished serialPort -> { state with SerialPort = Some serialPort }, pingAllFlowers serialPort state.Flowers


    | Action.OpenSerialPort asyncOperation ->
        match asyncOperation with
        | Start serialPort -> state, openSerialPort serialPort
        | Finished serialPort ->
            Log.debug $"Connected to serial port '{serialPort.PortName}'"

            { state with SerialPort = Some serialPort },
            Cmd.batch [
                SerialPort.onReceived (Action.ReceivedDataFromSerialPort >> Action) serialPort
                Cmd.ofMsg RerenderView
                pingAllFlowers serialPort state.Flowers
            ]

    | Action.CloseSerialPort asyncOperation ->
        match asyncOperation with
        | Start serialPort -> state, closeSerialPort serialPort
        | Finished serialPort ->
            Log.debug $"Closed serial port '{serialPort.PortName}'"
            state, Cmd.ofMsg RerenderView

    | Action.ReceivedDataFromSerialPort packet ->
        match Response.fromPacket packet with
        | Some response ->
            Log.debug $"Processed response from flower with I2C address '{response.I2cAddress}'"
            updateFlowerFromResponse response state, Cmd.none

        | None ->
            Log.error "Could not properly parse data from serial port."
            state, Cmd.none

    // ---- Flower Actions ----

    | Action.NewFlower -> addNewFlower state

    | Action.SelectFlower id -> { state with Selected = Some id }, Cmd.none

    | Action.DeselectFlower -> { state with Selected = None }, Cmd.none

    | Action.DeleteFlower ->
        match state.Selected with
        | Some id ->
            { state with
                Flowers = Map.remove id state.Flowers
                Selected = None },
            Cmd.none

        | None -> state, Cmd.none

    | Action.SendCommand asyncOperation ->
        match asyncOperation with
        | Start command -> state, sendCommandToSelected command state
        | Finished exn ->
            Log.error $"Could not send command over the serial port{Environment.NewLine}{exn}"
            state, Cmd.none

    | Action.PingFlower asyncOperation ->
        match asyncOperation with
        | Start _ -> state, sendCommandToSelected Ping state

        | Finished exn ->
            Log.error $"Could not receive request from flower{Environment.NewLine}{exn}"
            state, Cmd.none


let private updateMenu (msg: Menu.Msg) (state: State) (window: Window) : State * Cmd<Msg> =
    match msg with
    | Menu.NewFile -> state, Cmd.ofMsg (Action.NewFile |> Action)
    | Menu.SaveAs -> state, Cmd.ofMsg (Start() |> Action.SaveAsDialog |> Action)
    | Menu.OpenFile -> state, Cmd.ofMsg (Start() |> Action.OpenFileDialog |> Action)
    | Menu.Open filePath -> updateAction (Start filePath |> Action.OpenFile) state window

let private updateIconDock (msg: IconDock.Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | IconDock.NewFile -> state, Cmd.ofMsg (Action.NewFile |> Action)
    | IconDock.SaveAs -> state, Cmd.ofMsg (Start() |> Action.SaveAsDialog |> Action)
    | IconDock.Open -> state, Cmd.ofMsg (Start() |> Action.OpenFileDialog |> Action)
    | IconDock.NewFlower -> state, Cmd.ofMsg (Action.NewFlower |> Action)


let private updateFlowerProperties (msg: FlowerProperties.Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | FlowerProperties.ChangeName (id, newName) -> updateFlower id "Name" Flower.setName newName state, Cmd.none

    | FlowerProperties.ChangeI2cAddress (id, i2cAddressString) ->
        if not <| String.IsNullOrEmpty i2cAddressString then
            match String.parseByte i2cAddressString with
            | Some i2cAddress -> updateFlower id "I2C Address" Flower.setI2cAddress i2cAddress state, Cmd.none
            | None ->
                Log.debug $"Could not parse invalid I2C address '{i2cAddressString}' for flower '{id}'"
                state, Cmd.none
        else
            state, Cmd.none

let private updateFlowerCommands (msg: FlowerCommands.Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | FlowerCommands.ChangePort newPortName -> connectToSerialPort newPortName state

    | FlowerCommands.OpenSerialPort serialPort ->
        state, Cmd.OfTask.perform SerialPort.openPort serialPort (Finished >> Action.OpenSerialPort >> Action)

    | FlowerCommands.CloseSerialPort serialPort ->
        state, Cmd.OfTask.perform SerialPort.closePort serialPort (Finished >> Action.CloseSerialPort >> Action)

    | FlowerCommands.OpenSerialPortsDropdown -> state, Cmd.ofMsg (Start() |> Action.RefreshSerialPorts |> Action)

    | FlowerCommands.Msg.ChangePercentage (id, percentage) ->
        updateFlower id "Open Percentage" Flower.setOpenPercent percentage state, Cmd.none

    | FlowerCommands.Msg.ChangeSpeed (id, speed) -> updateFlower id "Speed" Flower.setMaxSpeed speed state, Cmd.none

    | FlowerCommands.Msg.ChangeAcceleration (id, acceleration) ->
        updateFlower id "Acceleration" Flower.setAcceleration acceleration state, Cmd.none

    | FlowerCommands.Msg.SendCommand command -> state, sendCommandToSelected command state


let private updateSimulationEvent (msg: SimulationEvent) (state: State) : State * Cmd<Msg> =
    match msg with
    | BackgroundEvent backgroundEvent ->
        match backgroundEvent with
        | BackgroundEvent.OnReleased _ ->
            Log.verbose "Background: Pointer Released"

            { state with
                FlowerInteraction = NoInteraction
                Selected = None },
            Cmd.none

    | FlowerEvent flowerEvent ->
        match flowerEvent with
        | FlowerPointerEvent.OnEnter (flowerId, _) ->
            Log.verbose $"Flower: Hovering {Id.shortName flowerId}"

            { state with FlowerInteraction = Hovering flowerId }, Cmd.none

        | FlowerPointerEvent.OnLeave (flowerId, _) ->
            Log.verbose $"Flower: Pointer Left {Id.shortName flowerId}"

            { state with FlowerInteraction = NoInteraction }, Cmd.none

        | FlowerPointerEvent.OnMoved (flowerId, e) ->
            match state.FlowerInteraction with
            | Pressing pressing when
                pressing.Id = flowerId
                && Point2D.distanceSquaredTo pressing.MousePressedLocation e.Position > minMouseMovementSquared
                ->
                Log.verbose $"Flower: Start Dragging {Id.shortName flowerId}"

                let delta =
                    pressing.InitialFlowerPosition
                    - pressing.MousePressedLocation

                let newPosition = e.Position + delta

                { state with
                    Flowers = Map.update pressing.Id (Flower.setPosition newPosition) state.Flowers
                    FlowerInteraction =
                        Dragging
                            { Id = pressing.Id
                              DraggingDelta = delta } },
                Cmd.none

            | Dragging draggingData ->
                // Continue dragging
                let newPosition =
                    e.Position + draggingData.DraggingDelta

                { state with Flowers = Map.update draggingData.Id (Flower.setPosition newPosition) state.Flowers },
                Cmd.none

            // Take no action
            | _ -> state, Cmd.none


        | FlowerPointerEvent.OnPressed (flowerId, e) ->
            if InputTypes.isPrimary e.MouseButton then
                let maybeFlower =
                    getFlower flowerId state.Flowers

                match maybeFlower with
                | Some pressed ->
                    Log.verbose $"Flower: Pressed {Id.shortName pressed.Id}"

                    { state with
                        FlowerInteraction =
                            Pressing
                                { Id = flowerId
                                  MousePressedLocation = e.Position
                                  InitialFlowerPosition = pressed.Position } },
                    Cmd.none

                | None ->
                    Log.error "Could not find the flower that was pressed"
                    state, Cmd.none
            else
                state, Cmd.none


        | FlowerPointerEvent.OnReleased (flowerId, e) ->
            if InputTypes.isPrimary e.MouseButton then
                match state.FlowerInteraction with
                | Dragging _ ->
                    Log.verbose $"Flower: Dragging -> Hovering {Id.shortName flowerId}"

                    { state with FlowerInteraction = Hovering flowerId }, Cmd.none

                | Pressing _ ->
                    Log.verbose $"Flower: Selected {Id.shortName flowerId}"

                    { state with
                        FlowerInteraction = Hovering flowerId
                        Selected = Some flowerId },
                    Cmd.none


                | flowerEvent ->
                    Log.warning $"Unhandled event {flowerEvent}"
                    state, Cmd.none

            // Non primary button pressed
            else
                state, Cmd.none

let update (msg: Msg) (state: State) (window: Window) : State * Cmd<Msg> =

    match msg with
    // Shell Messages

    | Action action -> updateAction action state window

    | RerenderView -> { state with Rerender = state.Rerender + 1 }, Cmd.none

    | ReadAppConfig appConfigResult ->
        match appConfigResult with
        | Ok appConfig ->
            Log.info "Loaded Application Configuration from the disk."
            { state with AppConfig = appConfig }, Cmd.none

        | Error readError ->
            match readError with
            | File.ReadError.DirectoryDoesNotExist _
            | File.ReadError.FileDoesNotExist _ ->
                Log.info "Could not find a configuration file on this computer so I'm creating one."
                state, saveAppConfigFile state.AppConfig

            | File.ReadError.FileAlreadyOpened _ ->
                Log.error "Cannot open the Application Configuration file, it is already opened by another program."
                state, Cmd.none
                
            | File.ReadError.JsonDeserializationError (_, error) ->
                Log.error $"An error occured when decoding the Json data in the Application Configuration file.{Environment.NewLine}{error}"
                state, Cmd.none

            | File.ReadError.InvalidFilePermissions _ ->
                Log.error "Cannot open the configuration file because I don't have the right file permissions."
                state, Cmd.none

            | File.ReadError.UnknownException exn ->
                match exn with
                | _ ->
                    Log.error
                        $"There was an error when trying to load the Application Configuration file{Environment.NewLine}{exn}"

                    state, Cmd.none

    | WroteAppConfig result ->
        match result with
        | Ok _ ->
            Log.info "Wrote and updated the Application Configuration file."
            state, Cmd.none

        | Error writeError ->
            match writeError with
            | File.WriteError.DirectoryDoesNotExist _
            | File.WriteError.FileDoesNotExist _ ->
                Log.info "Could not find a configuration file on this computer so I'm creating one."
                state, saveAppConfigFile state.AppConfig

            | File.WriteError.FileAlreadyOpened _ ->
                Log.error "Cannot open the configuration file, it is already opened."
                state, Cmd.none

            | File.WriteError.InvalidFilePermissions _ ->
                Log.error "Cannot open the configuration file because I don't have the right file permissions."
                state, Cmd.none

            | File.WriteError.UnknownException exn ->
                Log.error $"There was an error writing the Application Configuration{Environment.NewLine}{exn}"
                state, Cmd.none


    // Msg Mapping
    | MenuMsg menuMsg -> updateMenu menuMsg state window
    | IconDockMsg iconDockMsg -> updateIconDock iconDockMsg state
    | SimulationEvent event -> updateSimulationEvent event state
    | FlowerPropertiesMsg flowerPropertiesMsg -> updateFlowerProperties flowerPropertiesMsg state
    | FlowerCommandsMsg flowerCommandsMsg -> updateFlowerCommands flowerCommandsMsg state


// ---- View Functions ----

open Avalonia.FuncUI.Types
open Avalonia.FuncUI.DSL

let private drawFlower (state: State) (dispatch: FlowerPointerEvent -> unit) (flower: Flower) : IView =
    let flowerState (flower: Flower) : Flower.Attribute option =
        match state.FlowerInteraction with
        | Hovering id when id = flower.Id -> Flower.hovered |> Some
        | Pressing pressing when pressing.Id = flower.Id -> Flower.pressed |> Some
        | Dragging dragging when dragging.Id = flower.Id -> Flower.dragged |> Some
        | _ -> None

    Flower.draw
        flower
        [ if Option.contains flower.Id state.Selected then
              Flower.selected
          yield! flowerState flower |> Option.toList

          Flower.onPointerEnter (FlowerPointerEvent.OnEnter >> dispatch)
          Flower.onPointerLeave (FlowerPointerEvent.OnLeave >> dispatch)
          Flower.onPointerMoved (FlowerPointerEvent.OnMoved >> dispatch)
          Flower.onPointerPressed (FlowerPointerEvent.OnPressed >> dispatch)
          Flower.onPointerReleased (FlowerPointerEvent.OnReleased >> dispatch) ]
    :> IView

let private simulationSpace state (dispatch: SimulationEvent -> unit) : IView =
    let flowers =
        state.Flowers
        |> Map.values
        |> Seq.map (drawFlower state (SimulationEvent.FlowerEvent >> dispatch))
        |> Seq.toList

    Canvas.create [
        Canvas.children flowers
        Canvas.background Theme.palette.canvasBackground
        Canvas.name Constants.CanvasId
        Canvas.onPointerReleased (
            Event.pointerReleased Constants.CanvasId
            >> Option.map (
                BackgroundEvent.OnReleased
                >> SimulationEvent.BackgroundEvent
                >> dispatch
            )
            >> Option.defaultValue ()
        )
    ]
    :> IView


let private simulationView (state: State) (dispatch: Msg -> unit) =
    let selectedFlowerOption =
        Option.bind (fun id -> getFlower id state.Flowers) state.Selected

    DockPanel.create [
        DockPanel.children [
            DockPanel.child Dock.Top (Menu.applicationMenu state.AppConfig (MenuMsg >> dispatch))

            DockPanel.child Dock.Top (IconDock.view (IconDockMsg >> dispatch))

            DockPanel.child Dock.Left (FlowerProperties.view selectedFlowerOption (FlowerPropertiesMsg >> dispatch))

            DockPanel.child
                Dock.Right
                (FlowerCommands.view
                    selectedFlowerOption
                    state.SerialPorts
                    state.SerialPort
                    (FlowerCommandsMsg >> dispatch))


            simulationSpace state (Msg.SimulationEvent >> dispatch)
        ]
    ]


let view (state: State) (dispatch: Msg -> unit) = simulationView state dispatch
