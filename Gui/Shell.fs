module Gui.Shell

open System
open System.IO
open System.IO.Ports
open Elmish
open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.Input
open Math.Geometry
open Math.Units

open Extensions
open Gui.DataTypes
open Gui.Views
open Gui.Views.Components
open Gui.Views.Menu
open Gui.Views.Panels


// ---- States ---------------------------------------------------------------------------------------------------------

[<StructuralEquality; NoComparison>]
type State =
    { SerialPort: SerialPort option
      Rerender: int
      AppConfig: AppConfig
      FlowerManager: FlowerManager.State
      FlowerCommandsState: FlowerCommands.State
      // Tabs
      EaTab: EaTab.State }

and FlowerInteraction =
    | Hovering of Flower Id
    | Pressing of PressedData
    | Dragging of DraggingData
    | NoInteraction

and PressedData =
    { Id: Flower Id
      MousePressedLocation: Point2D<Meters, ScreenSpace>
      InitialFlowerPosition: Point2D<Meters, ScreenSpace> }

and DraggingData =
    { Id: Flower Id
      DraggingDelta: Vector2D<Meters, ScreenSpace> }


// ---- Messaging ------------------------------------------------------------------------------------------------------

[<RequireQualifiedAccess>]
type BackgroundEvent = OnReleased of MouseButtonEvent<ScreenSpace>


type Msg =
    // Shell Messages
    | Tick of Duration
    | Action of Action
    | RerenderView
    | ReadAppConfig of Result<AppConfig, File.ReadError>
    | WroteAppConfig of Result<FileInfo, File.WriteError>

    // Msg Mapping
    | EaTabMsg of EaTab.Msg
    | MenuMsg of Menu.Msg
    | IconDockMsg of IconDock.Msg
    | FlowerManagerMsg of FlowerManager.Msg
    | FlowerPropertiesMsg of FlowerProperties.Msg
    | FlowerCommandsMsg of FlowerCommands.Msg


// ---- High Level Key Handling ----------------------------------------------------------------------------------------

let keyUpHandler (window: Window) _ =
    let sub dispatch =
        window.KeyUp.Add (fun eventArgs ->
            match eventArgs.Key with
            | Key.Escape -> Action.DeselectFlower |> Action |> dispatch
            | Key.Delete -> Action.DeleteFlower |> Action |> dispatch
            | _ -> ())

    Cmd.ofSub sub


// ---- File Writing ---------------------------------------------------------------------------------------------------

let saveAppConfigFile (appConfig: AppConfig) : Cmd<Msg> =
    File.write AppConfig.configPath appConfig WroteAppConfig

let loadAppConfigFile: Cmd<Msg> =
    File.read AppConfig.configPath ReadAppConfig


// ---- Init -----------------------------------------------------------------------------------------------------------

let init () : State * Cmd<Msg> =
    let fps = 30

    let flowerState, flowerCmd =
        FlowerCommands.init ()

    { FlowerManager = FlowerManager.init ()
      SerialPort = None
      Rerender = 0
      AppConfig = AppConfig.init
      FlowerCommandsState = flowerState
      EaTab = EaTab.init () },

    Cmd.batch [
        loadAppConfigFile
        Cmd.map FlowerCommandsMsg flowerCmd
        Sub.timer fps Tick
    ]


// ---- Serial Port Updates --------------------------------------------------------------------------------------------

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


// ---- Flower Functions -------------------------------------------------------------------------------------------------

let private mapFlowerManager (f: FlowerManager.State -> FlowerManager.State) (state: State) : State =
    { state with FlowerManager = f state.FlowerManager }


let private sendCommandTo (serialPort: SerialPort) (i2cAddress: I2cAddress) (command: Command) =
    Cmd.OfTask.attempt (Command.sendCommand serialPort i2cAddress) command (Finished >> SendCommand >> Action)

let private sendCommandToSelected (command: Command) (state: State) : Cmd<Msg> =
    let maybeSelected =
        FlowerManager.getSelected state.FlowerManager

    match state.SerialPort, maybeSelected with
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

let private pingAllFlowers (serialPort: SerialPort) (state: State) : Cmd<Msg> =
    let flowers =
        FlowerManager.getFlowers state.FlowerManager

    let cmds =
        Seq.map (pingFlower serialPort) flowers

    Cmd.batch cmds

let private newFile (state: State) (flowers: Flower seq) : State =
    mapFlowerManager
        (FlowerManager.clear
         >> FlowerManager.addFlowers flowers)
        state


let updateName id newName state =
    mapFlowerManager (FlowerManager.updateFlower id "Name" Flower.setName newName) state

let updateI2cAddress id i2cAddressString state =
    match String.parseByte i2cAddressString with
    | Some i2cAddress ->
        let updateI2c =
            FlowerManager.updateFlower id "I2C Address" Flower.setI2cAddress i2cAddress

        mapFlowerManager updateI2c state

    | None ->
        Log.debug $"Could not parse invalid I2C address '{i2cAddressString}' for flower '{id}'"
        state

let updateOpenPercent id percentage state =
    mapFlowerManager (FlowerManager.updateFlower id "Open Percentage" Flower.setOpenPercent percentage) state


let updateTargetPercent id percentage state =
    mapFlowerManager (FlowerManager.updateFlower id "Open Target" Flower.setTargetPercent percentage) state


let updateSpeed id speed state =
    mapFlowerManager (FlowerManager.updateFlower id "Speed" Flower.setSpeed speed) state

let updateMaxSpeed id speed state =
    mapFlowerManager (FlowerManager.updateFlower id "Max Speed" Flower.setMaxSpeed speed) state

let updateAcceleration id acceleration state =
    mapFlowerManager (FlowerManager.updateFlower id "Acceleration" Flower.setAcceleration acceleration) state

// ---- Update ---------------------------------------------------------------------------------------------------------

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
            let flowerFileData =
                FlowerManager.getFlowers state.FlowerManager

            let writeCmd =
                File.write fileInfo flowerFileData (Finished >> Action.SaveAs >> Action)

            state, writeCmd

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

    | Action.ConnectAndOpenPort asyncOperation ->
        match asyncOperation with
        | Start portName -> connectToSerialPort portName state
        | Finished serialPort ->
            let pingFlowers =
                pingAllFlowers serialPort state


            { state with SerialPort = Some serialPort }, pingFlowers


    | Action.OpenSerialPort asyncOperation ->
        match asyncOperation with
        | Start serialPort -> state, openSerialPort serialPort
        | Finished serialPort ->
            Log.debug $"Connected to serial port '{serialPort.PortName}'"

            { state with SerialPort = Some serialPort },
            Cmd.batch [
                SerialPort.onReceived (Action.ReceivedDataFromSerialPort >> Action) serialPort
                Cmd.ofMsg RerenderView
                pingAllFlowers serialPort state
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
            mapFlowerManager (FlowerManager.updateFlowerFromResponse response) state, Cmd.none

        | None ->
            Log.error "Could not properly parse data from serial port."
            state, Cmd.none

    // ---- Flower Actions ----

    | Action.NewFlower ->
        let flowerManager, flower =
            FlowerManager.addNewFlower state.FlowerManager

        let requestCmd =
            match state.SerialPort with
            | Some serialPort -> pingFlower serialPort flower
            | None -> Cmd.none

        { state with FlowerManager = flowerManager }, requestCmd

    | Action.SelectFlower id -> mapFlowerManager (FlowerManager.select id) state, Cmd.none

    | Action.DeselectFlower -> mapFlowerManager FlowerManager.deselect state, Cmd.none

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

    | Action.DeleteFlower -> mapFlowerManager FlowerManager.deleteSelected state, Cmd.none


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


let private updateFlowerProperties (msg: FlowerProperties.Msg) (state: State) (window: Window) : State * Cmd<Msg> =
    match msg with
    | FlowerProperties.Action action -> updateAction action state window
    | FlowerProperties.ChangeName (id, newName) -> updateName id newName state, Cmd.none
    | FlowerProperties.ChangeI2cAddress (id, i2cAddressString) -> updateI2cAddress id i2cAddressString state, Cmd.none
    | FlowerProperties.Msg.ChangeOpenPercentage (id, percentage) -> updateOpenPercent id percentage state, Cmd.none
    | FlowerProperties.Msg.ChangeTargetPercentage (id, percentage) -> updateTargetPercent id percentage state, Cmd.none
    | FlowerProperties.Msg.ChangeSpeed (id, speed) -> updateSpeed id speed state, Cmd.none
    | FlowerProperties.Msg.ChangeMaxSpeed (id, speed) -> updateMaxSpeed id speed state, Cmd.none
    | FlowerProperties.Msg.ChangeAcceleration (id, acceleration) -> updateAcceleration id acceleration state, Cmd.none


let private receiveFlowerCommandsExternal (msg: FlowerCommands.External) (state: State) : State * Cmd<Msg> =
    match msg with
    | FlowerCommands.External.ChangePort newPortName -> connectToSerialPort newPortName state

    | FlowerCommands.External.OpenSerialPort serialPort ->
        state, Cmd.OfTask.perform SerialPort.openPort serialPort (Finished >> Action.OpenSerialPort >> Action)

    | FlowerCommands.External.CloseSerialPort serialPort ->
        state, Cmd.OfTask.perform SerialPort.closePort serialPort (Finished >> Action.CloseSerialPort >> Action)

    | FlowerCommands.External.SendCommand command -> state, sendCommandToSelected command state

    | FlowerCommands.External.NoMsg -> state, Cmd.none


// ---- Update ---------------------------------------------------------------------------------------------------------

let update (msg: Msg) (state: State) (window: Window) : State * Cmd<Msg> =

    match msg with
    // Shell Messages
    // TODO
    | Tick _ -> state, Cmd.none

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
                Log.error
                    $"An error occured when decoding the Json data in the Application Configuration file.{Environment.NewLine}{error}"

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
    | EaTabMsg eaTabMsg ->
        let eaTabState, eaTabCmd =
            EaTab.update eaTabMsg state.EaTab

        { state with EaTab = eaTabState }, Cmd.map EaTabMsg eaTabCmd

    | MenuMsg menuMsg -> updateMenu menuMsg state window
    | IconDockMsg iconDockMsg -> updateIconDock iconDockMsg state
    | FlowerManagerMsg msg -> mapFlowerManager (FlowerManager.updateMsg msg) state, Cmd.none
    | FlowerPropertiesMsg flowerPropertiesMsg -> updateFlowerProperties flowerPropertiesMsg state window
    | FlowerCommandsMsg flowerCommandsMsg ->
        let flowerCommandsState, flowerCommandsCmd, flowerCommandsExternal =
            FlowerCommands.update flowerCommandsMsg state.FlowerCommandsState

        let newState, shellCmd =
            { state with FlowerCommandsState = flowerCommandsState }
            |> receiveFlowerCommandsExternal flowerCommandsExternal

        newState,
        Cmd.batch [
            shellCmd
            Cmd.map FlowerCommandsMsg flowerCommandsCmd
        ]


// ---- View Functions -------------------------------------------------------------------------------------------------

let private simulationView (state: State) (dispatch: Msg -> unit) =
    let maybeSelectedFlower =
        FlowerManager.getSelected state.FlowerManager

    let flowers =
        FlowerManager.getFlowers state.FlowerManager

    DockPanel.create [
        DockPanel.children [
            DockPanel.child Dock.Top (Menu.applicationMenu state.AppConfig (MenuMsg >> dispatch))

            DockPanel.child Dock.Top (IconDock.view (IconDockMsg >> dispatch))

            DockPanel.child
                Dock.Left
                (FlowerProperties.view flowers maybeSelectedFlower (FlowerPropertiesMsg >> dispatch))

            DockPanel.child
                Dock.Right
                (FlowerCommands.view
                    state.FlowerCommandsState
                    maybeSelectedFlower
                    state.SerialPort
                    (FlowerCommandsMsg >> dispatch))


            FlowerManager.view state.FlowerManager (Msg.FlowerManagerMsg >> dispatch)
        ]
    ]

let view (state: State) (dispatch: Msg -> unit) =
    let flowerTab =
        TabItem.create [
            TabItem.header "Simulation"
            TabItem.content (simulationView state dispatch)
        ]

    let eaTab =
        TabItem.create [
            TabItem.header "Evolutionary Algorithm"
            TabItem.content (EaTab.view state.EaTab (EaTabMsg >> dispatch))
        ]

    TabControl.create [
        TabControl.viewItems [
            flowerTab
            eaTab
        ]
    ]
