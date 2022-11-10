/// The program shell is the main file which ties together the whole application
/// state as well as all the view functions used to render the application to
/// the screen.
module Gui.Shell

open System
open System.IO
open System.IO.Ports
open Elmish
open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.Input
open Math.Units

open Extensions
open Gui.DataTypes
open Gui.Views
open Gui.Views.Components
open Gui.Views.Menu
open Gui.Views.Panels


// ---- States ---------------------------------------------------------------------------------------------------------

/// This is the program state for the whole application. This contains all
/// the program components for the user interface as well as data needed to
/// execute commands externally to the flowers.
[<StructuralEquality; NoComparison>]
type State =
    { SerialPort: SerialPort option
      Rerender: int
      AppConfig: AppConfig
      FlowerManager: FlowerManager.State
      FlowerCommandsState: FlowerCommands.State
      // Tabs
      EaTab: EaTab.State }


// ---- Messaging ------------------------------------------------------------------------------------------------------

/// Events that are triggered when the background of the application is clicked.
/// This is triggered when the avalonia canvas element is selected.
[<RequireQualifiedAccess>]
type BackgroundEvent = OnReleased of MouseButtonEvent<ScreenSpace>


/// These are the main messages for the application. Messages that are sent from
/// subscriptions are handled from this data type. Messages that can only be
/// handled from the top level program are also handled here. These are generally
/// messages like file reading/writing. The last type of messages are the messages
/// that map sub-component messages. These messages need wrappers to be handled
/// by the program, but are then sent to the components themselves to be handled
/// by those modules themselves.
type Msg =
    // Shell Messages
    | Tick of Duration
    | Action of Action
    | ReadAppConfig of Result<AppConfig, File.ReadError>
    | WroteAppConfig of Result<FileInfo, File.WriteError>

    // Msg Mapping
    | EaTabMsg of EaTab.Msg
    | MenuMsg of Menu.Msg
    | IconDockMsg of IconDock.Msg
    | FlowerManagerMsg of FlowerManager.Msg
    | FlowerPropertiesMsg of FlowerProperties.Msg
    | FlowerCommandsMsg of FlowerCommands.Msg


/// Call this function to rerender the application.
///
/// This function is a bit of a hack but is needed to rerender the application.
/// This function needs to be called when mutable data structures since they
/// use reference equality instead of structural equality. Avalonia.FuncUI uses
/// state diffing to determine if the program needs to be rerendered and mutable
/// components don't change their equality state when their internal values are
/// changed.
let private rerender (state: State) : State =
    { state with Rerender = state.Rerender + 1 }


// ---- High Level Key Handling ----------------------------------------------------------------------------------------

/// Key handler for the whole application. This registered key events to the
/// application window, so key events regardless of focus or mouse position
/// are triggered if they are registered here.
let keyUpHandler (window: Window) _ : Cmd<Msg> =
    let sub dispatch =
        window.KeyUp.Add (fun eventArgs ->
            match eventArgs.Key with
            | Key.Escape -> Action.DeselectFlower |> Action |> dispatch
            | Key.Delete -> Action.DeleteFlower |> Action |> dispatch
            | _ -> ())

    Cmd.ofSub sub


// ---- Serial Port Updates --------------------------------------------------------------------------------------------

/// Open up a connected serial port
let private openSerialPort (serialPort: SerialPort) : Cmd<Msg> =
    Cmd.OfTask.perform SerialPort.openPort serialPort (Finished >> Action.OpenSerialPort >> Action)

/// Close a connected serial port
let private closeSerialPort (serialPort: SerialPort) : Cmd<Msg> =
    Cmd.OfTask.perform SerialPort.closePort serialPort (Finished >> Action.CloseSerialPort >> Action)

/// Connect to a serial port and open it up for communication.
let private connectAndOpenSerialPort (newPortName: SerialPortName) : Cmd<Msg> =
    Cmd.OfTask.perform SerialPort.connectAndOpenPort newPortName (Finished >> Action.OpenSerialPort >> Action)

/// Open up a new serial port. If there is a serial port currently opened, it
/// is closed and disconnected. If no port is selected, then the current serial
/// port is disconnected.
///
/// Note: This is generally the function that you want to use to connect to a
/// new serial port. This function handles change in program as well as
/// dispatching connection commands.
let private connectToSerialPort (newPortName: SerialPortName) (state: State) : State * Cmd<Msg> =
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

/// This is a wrapper function to make it easier to update the
/// FlowerManager.State object within the Shell.State. If you give it a function
/// that updates the FlowerManager.State it will update it within the Shell.State.
let private mapFlowerManager (f: FlowerManager.State -> FlowerManager.State) (state: State) : State =
    { state with FlowerManager = f state.FlowerManager }


/// Send a flower command to the currently selected flower.
let private sendCommandToSelected (command: Command) (state: State) : State * Cmd<Msg> =
    let maybeSelected =
        FlowerManager.getSelected state.FlowerManager

    let newStateFrom (flower: Flower) : State =
        mapFlowerManager (FlowerManager.updateFlower flower.Id "Apply Command" Flower.applyCommand command) state

    match state.SerialPort, maybeSelected with
    | Some serialPort, Some flower ->
        Log.debug $"Sending command through serial port '{command}'"

        let communicationCmd =
            Cmd.OfTask.attempt
                (Command.sendCommand serialPort flower.I2cAddress)
                command
                (Finished >> SendCommand >> Action)

        newStateFrom flower, communicationCmd

    | None, Some flower ->
        Log.warning "Serial port is not selected, cannot send command."
        newStateFrom flower, Cmd.none

    | Some _, None ->
        Log.warning "Flower is not selected, cannot send command."
        state, Cmd.none

    | None, None ->
        Log.error "An unknown error occured when trying to send command."
        state, Cmd.none

/// Send the ping command through the serial port to the given flower.
let private pingFlower (serialPort: SerialPort) (flower: Flower) : Cmd<Msg> =
    Cmd.OfTask.attempt (Command.sendCommand serialPort flower.I2cAddress) Ping (Finished >> SendCommand >> Action)

/// Ping all flowers that are registered within the application.
let private pingAllFlowers (serialPort: SerialPort) (state: State) : Cmd<Msg> =
    let flowers =
        FlowerManager.getFlowers state.FlowerManager

    let cmds =
        Seq.map (pingFlower serialPort) flowers

    Cmd.batch cmds

/// Set a new name for a flower with the given Id.
let updateName (id: Flower Id) (newName: string) (state: State) : State =
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

/// Set the percentage that a flower is open.
let updateOpenPercent (id: Flower Id) (percentage: Percent) (state: State) : State =
    mapFlowerManager (FlowerManager.updateFlower id "Open Percentage" Flower.setOpenPercent percentage) state


/// Set the percent that the flower should move to.
let updateTargetPercent (id: Flower Id) (percentage: Percent) (state: State) : State =
    mapFlowerManager (FlowerManager.updateFlower id "Open Target" Flower.setTargetPercent percentage) state


/// Set the current speed of the flower.
let updateSpeed (id: Flower Id) (speed: AngularSpeed) (state: State) : State =
    mapFlowerManager (FlowerManager.updateFlower id "Speed" Flower.setSpeed speed) state

/// Set the maximum speed the flower can go.
let updateMaxSpeed (id: Flower Id) (speed: AngularSpeed) (state: State) : State =
    mapFlowerManager (FlowerManager.updateFlower id "Max Speed" Flower.setMaxSpeed speed) state

/// Set the acceleration of the flower. Acceleration/deceleration is constant
/// for flower movement.
let updateAcceleration (id: Flower Id) (acceleration: AngularAcceleration) (state: State) : State =
    mapFlowerManager (FlowerManager.updateFlower id "Acceleration" Flower.setAcceleration acceleration) state


/// Update the flower simulation based on the amount of elapsed time since the
/// last simulation update.
let tick (elapsed: Duration) (state: State) : State =
    mapFlowerManager (FlowerManager.tick elapsed) state


// ---- File Writing ---------------------------------------------------------------------------------------------------

/// Save the application configuration state. This includes application
/// information that is not specific to flower manipulation.
let saveAppConfigFile (appConfig: AppConfig) : Cmd<Msg> =
    File.write AppConfig.configPath appConfig WroteAppConfig

/// Load the application configuration state. This includes application
/// information that is not specific to flower manipulation.
let loadAppConfigFile: Cmd<Msg> =
    File.read AppConfig.configPath ReadAppConfig

/// Create a new application state with only the flowers that are given to
/// this function.
let private startWithFlowers (state: State) (flowers: Flower seq) : State =
    mapFlowerManager
        (FlowerManager.clear
         >> FlowerManager.addFlowers flowers)
        state

/// Command used to save the current flower state to the hard drive.
let private flowerSaveAsCmd (fileInfo: FileInfo) (state: State) : Cmd<Msg> =
    let flowerFileData: Flower list =
        FlowerManager.getFlowers state.FlowerManager
        |> List.ofSeq

    File.write fileInfo flowerFileData (Finished >> Action.SaveAs >> Action)

/// This is used to update the current state of the application after a file
/// save has been successfully completed.
let private saveAs (fileInfo: FileInfo) (state: State) : State * Cmd<Msg> =
    Log.info $"Saved flower file {fileInfo.Name}"

    let newAppConfig =
        AppConfig.addRecentFile fileInfo state.AppConfig

    { state with AppConfig = newAppConfig }, saveAppConfigFile newAppConfig

/// Command used to open a flower file.
let openFile (path: FileInfo) : Cmd<Msg> =
    File.read path (Finished >> Action.OpenFile >> Action)

/// This function handles the file result returned from an attempt to open
/// a flower file. This will update the state with the new flowers that are
/// loaded or print the error that occurred when opening the file.
let fileOpened (fileResult: Result<Flower list, 'a>) (state: State) : State =
    match fileResult with
    | Ok flowers -> startWithFlowers state flowers
    | Error readingError ->
        Log.error $"Could not read file{Environment.NewLine}{readingError}"
        state

/// Start a new file. This clears the current application state and starts with
/// a fresh file with no flowers initialized.
let private newFile (state: State) : State =
    mapFlowerManager FlowerManager.clear state


// ---- Windows --------------------------------------------------------------------------------------------------------

/// Creates a command to open a new file manager window to select the file
/// location to save a flower file.
let private openSaveDialog (window: Window) : Cmd<Msg> =
    Cmd.OfTask.either
        FlowerFile.saveFileDialog
        window
        (Start >> Action.SaveAs >> Action)
        (Finished >> Action.SaveAsDialog >> Action)

/// Creates a command to open a new file manager window to select the flower
/// file you would like to open.
let private openFileDialog (window: Window) : Cmd<Msg> =
    Cmd.OfTask.perform FlowerFile.openFileDialog window (Finished >> Action.OpenFileDialog >> Action)


// ---- Init -----------------------------------------------------------------------------------------------------------

/// This initializes the program state when the program first launches.
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

// ---- Update ---------------------------------------------------------------------------------------------------------

/// Update the program state when given a particular action.
let private updateAction (action: Action) (state: State) (window: Window) : State * Cmd<Msg> =
    match action with
    // ---- File Actions ----
    | Action.NewFile -> newFile state, Cmd.none

    | Action.SaveAsDialog asyncOperation ->
        match asyncOperation with
        | Start _ -> state, openSaveDialog window
        | Finished exn ->
            Log.error $"Encountered an error when trying to save file{Environment.NewLine}{exn}"
            state, Cmd.none

    | Action.SaveAs asyncOperation ->
        match asyncOperation with
        | Start fileInfo -> state, flowerSaveAsCmd fileInfo state
        | Finished (Ok fileInfo) -> saveAs fileInfo state
        | Finished (Error fileWriteError) ->
            Log.error $"Could not save file{Environment.NewLine}{fileWriteError}"
            state, Cmd.none

    | Action.OpenFileDialog asyncOperation ->
        match asyncOperation with
        | Start _ -> state, openFileDialog window
        | Finished files ->
            match Seq.tryHead files with
            | Some file -> state, openFile file
            // No file was selected
            | _ -> state, Cmd.none

    | Action.OpenFile asyncOperation ->
        match asyncOperation with
        | Start path -> state, openFile path
        | Finished fileResult -> fileOpened fileResult state, Cmd.none


    // ---- Serial Port Actions ----

    | Action.ConnectAndOpenPort asyncOperation ->
        match asyncOperation with
        | Start portName -> connectToSerialPort portName state
        | Finished serialPort -> { state with SerialPort = Some serialPort }, pingAllFlowers serialPort state


    | Action.OpenSerialPort asyncOperation ->
        match asyncOperation with
        | Start serialPort -> state, openSerialPort serialPort
        | Finished serialPort ->
            Log.debug $"Connected to serial port '{serialPort.PortName}'"

            { state with SerialPort = Some serialPort }
            |> rerender,
            Cmd.batch [
                SerialPort.onReceived (Action.ReceivedDataFromSerialPort >> Action) serialPort
                pingAllFlowers serialPort state
            ]

    | Action.CloseSerialPort asyncOperation ->
        match asyncOperation with
        | Start serialPort -> state, closeSerialPort serialPort
        | Finished serialPort ->
            Log.debug $"Closed serial port '{serialPort.PortName}'"
            rerender state, Cmd.none

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

    | Action.SelectFlower id ->
        let newState =
            state
            |> mapFlowerManager (FlowerManager.select id)

        newState, Cmd.none

    | Action.DeselectFlower -> mapFlowerManager FlowerManager.deselect state, Cmd.none

    | Action.SendCommand asyncOperation ->
        match asyncOperation with
        | Start command -> sendCommandToSelected command state
        | Finished exn ->
            Log.error $"Could not send command over the serial port{Environment.NewLine}{exn}"
            state, Cmd.none

    | Action.PingFlower asyncOperation ->
        match asyncOperation with
        | Start _ -> sendCommandToSelected Ping state

        | Finished exn ->
            Log.error $"Could not receive request from flower{Environment.NewLine}{exn}"
            state, Cmd.none

    | Action.DeleteFlower -> mapFlowerManager FlowerManager.deleteSelected state, Cmd.none


/// Update the program state when something in the menu has been clicked.
let private updateMenu (msg: Menu.Msg) (state: State) (window: Window) : State * Cmd<Msg> =
    match msg with
    | Menu.NewFile -> state, Cmd.ofMsg (Action.NewFile |> Action)
    | Menu.SaveAs -> state, Cmd.ofMsg (Start() |> Action.SaveAsDialog |> Action)
    | Menu.OpenFile -> state, Cmd.ofMsg (Start() |> Action.OpenFileDialog |> Action)
    | Menu.Open filePath -> updateAction (Start filePath |> Action.OpenFile) state window

/// Update the program state when a button in the icon dock has been clicked.
let private updateIconDock (msg: IconDock.Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | IconDock.NewFile -> state, Cmd.ofMsg (Action.NewFile |> Action)
    | IconDock.SaveAs -> state, Cmd.ofMsg (Start() |> Action.SaveAsDialog |> Action)
    | IconDock.Open -> state, Cmd.ofMsg (Start() |> Action.OpenFileDialog |> Action)
    | IconDock.NewFlower -> state, Cmd.ofMsg (Action.NewFlower |> Action)


/// Update the program state when something in the flower properties panel has
/// been changed.
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


/// Determine what to do when the flower commands panel sends out an external
/// command.
let private receiveFlowerCommandsExternal (msg: FlowerCommands.External) (state: State) : State * Cmd<Msg> =
    match msg with
    | FlowerCommands.External.ChangePort newPortName -> connectToSerialPort newPortName state
    | FlowerCommands.External.OpenSerialPort serialPort -> state, openSerialPort serialPort
    | FlowerCommands.External.CloseSerialPort serialPort -> state, closeSerialPort serialPort
    | FlowerCommands.External.SendCommand command -> sendCommandToSelected command state
    | FlowerCommands.External.BehaviorSelected behavior ->
        mapFlowerManager (FlowerManager.setBehavior behavior) state, Cmd.none
    | FlowerCommands.External.NoMsg -> state, Cmd.none


// ---- Update ---------------------------------------------------------------------------------------------------------

/// The main update function for the bloom application. This handles all the
/// messages that the program can send. This updates the Shell.State information
/// either directly or by sending messages the sub-components and updating them.
let update (msg: Msg) (state: State) (window: Window) : State * Cmd<Msg> =
    match msg with
    // Shell Messages
    | Tick elapsed -> tick elapsed state, Cmd.none

    | Action action -> updateAction action state window

    | ReadAppConfig appConfigResult ->
        match appConfigResult with
        | Ok appConfig ->
            Log.info "Loaded Application Configuration from the disk."

            let maybeOpenRecentFlowerFile =
                match List.tryHead appConfig.RecentFiles with
                | Some recentFile -> openFile recentFile
                | None -> Cmd.none

            { state with AppConfig = appConfig }, maybeOpenRecentFlowerFile

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

/// The view showing all the flowers on the screen. This simulation view is the
/// interactive panel in the center of the screen where you can select and move
/// the flowers to different locations.
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
                    state.FlowerManager.Behavior
                    state.SerialPort
                    (FlowerCommandsMsg >> dispatch))

            FlowerManager.view state.FlowerManager (Msg.FlowerManagerMsg >> dispatch)
        ]
    ]


/// The main view function for the application. This contains all the high
/// level view components for the application and defers a lot of the rendering
/// specifics to the modules in charge of those panels.
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
