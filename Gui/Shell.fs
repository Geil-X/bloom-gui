module Gui.Shell

open System
open System.IO.Ports
open Avalonia.Controls
open Avalonia.Input
open Elmish
open Avalonia.FuncUI.Elmish
open Avalonia.FuncUI.Components.Hosts

open Geometry
open Gui
open Gui.DataTypes
open Gui.Menu
open Gui.Panels
open Gui.Widgets
open Extensions


// ---- States ----

type State =
    { CanvasSize: Size<Pixels>
      Flowers: Map<Flower Id, Flower>
      FlowerInteraction: FlowerInteraction
      Selected: Flower Id option
      SerialPort: SerialPort option }

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
    | ActionError of ActionError
    | SerialPortOpened of SerialPort
    | SerialPortClosed of SerialPort
    | SendCommand of Command
    | CouldNotSendCommand of exn
    | SerialPortReceivedData of string

    // Msg Mapping
    | SimulationEvent of SimulationEvent
    | MenuMsg of Menu.Msg
    | IconDockMsg of IconDock.Msg
    | FlowerPanelMsg of FlowerPanel.Msg


// ---- Init ----

let init () =
    { CanvasSize = Size.create Length.zero Length.zero
      Flowers = Map.empty
      FlowerInteraction = NoInteraction
      Selected = None
      SerialPort = None },
    Cmd.batch []


// ---- Update helper functions -----

let minMouseMovement = Length.pixels 10.
let minMouseMovementSquared = Length.square minMouseMovement


// ---- Flower Functions ----

let getFlower id flowers : Flower option = Map.tryFind id flowers

let newFile (state: State) (flowers: Flower seq) : State =
    let flowerMap =
        Seq.fold (fun map (flower: Flower) -> Map.add flower.Id flower map) Map.empty flowers

    { state with
          Flowers = flowerMap
          FlowerInteraction = NoInteraction
          Selected = None }

let addFlower (flower: Flower) (state: State) : State =
    { state with
          Flowers = Map.add flower.Id flower state.Flowers }

let addFlowers (flowers: Flower seq) (state: State) : State =
    let flowerMap =
        Seq.fold (fun map (flower: Flower) -> Map.add flower.Id flower map) Map.empty flowers

    { state with Flowers = flowerMap }

let addNewFlower (state: State) : State =
    let flower =
        Flower.basic $"Flower {Map.count state.Flowers + 1}"
        |> Flower.setPosition (Point2D.pixels 100. 100.)

    addFlower flower state

let updateFlower (id: Flower Id) (property: string) (f: 'a -> Flower -> Flower) (value: 'a) (state: State) : State =
    if Option.contains id state.Selected then
        Log.verbose $"Updated flower '{Id.shortName id}' with new {property} '{value}'"

        { state with
              Flowers = Map.update id (f value) state.Flowers }
    else
        state


// ---- Update ----

let keyUpHandler (window: Window) _ =
    let sub dispatch =
        window.KeyUp.Add
            (fun eventArgs ->
                match eventArgs.Key with
                | Key.Escape -> Action.DeselectFlower |> Action |> dispatch
                | Key.Delete -> Action.DeleteFlower |> Action |> dispatch
                | _ -> ())

    Cmd.ofSub sub

let updateAction (action: Action) (state: State) (window: Window) : State * Cmd<Msg> =
    match action with
    // ---- File Actions ----

    | Action.NewFile -> newFile state Seq.empty, Cmd.none

    | Action.SaveAsDialog -> state, Cmd.OfTask.perform FlowerFile.saveFileDialog window (Action.SaveAs >> Action)


    | Action.SaveAs path ->
        state,
        Cmd.OfTask.attempt
            FlowerFile.writeFlowerFile
            (path, Map.values state.Flowers)
            (ActionError.ErrorSavingFile >> ActionError)


    | Action.OpenFileDialog ->
        state,
        Cmd.OfTask.perform
            FlowerFile.openFileDialog
            window
            (Option.split (Action.OpenFile >> Action) (ActionError.ErrorPickingFileToOpen |> ActionError))


    | Action.OpenFile path ->
        state,
        Cmd.OfTask.either
            FlowerFile.loadFlowerFile
            path
            (Action.FileOpened >> Action)
            (ActionError.CouldNotOpenFile >> ActionError)

    | Action.FileOpened flowers -> newFile state flowers, Cmd.none

    // ---- Flower Actions ----

    | Action.NewFlower -> addNewFlower state, Cmd.none

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


let updateActionError (error: ActionError) (state: State) : State * Cmd<Msg> =
    match error with
    | ActionError.ErrorPickingSaveFile ->
        Log.error "Could not pick file to save as"
        state, Cmd.none

    | ActionError.ErrorSavingFile exn ->
        match exn with
        // No save file was selected, do not report an error on this exception
        | :? AggregateException -> state, Cmd.none
        | _ ->
            Log.error $"Could not save file \n{exn}"
            state, Cmd.none

    | ActionError.ErrorPickingFileToOpen ->
        Log.error "Could not pick file to open"
        state, Cmd.none

    | ActionError.CouldNotOpenFile exn ->
        Log.error $"Could not open file\n{exn}"
        state, Cmd.none


let updateMenu (msg: Menu.Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | Menu.FileMsg msg ->
        match msg with
        | File.NewFile -> state, Cmd.ofMsg (Action.NewFile |> Action)
        | File.SaveAs -> state, Cmd.ofMsg (Action.SaveAsDialog |> Action)
        | File.OpenFile -> state, Cmd.ofMsg (Action.OpenFileDialog |> Action)

let updateIconDock (msg: IconDock.Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | IconDock.NewFile -> state, Cmd.ofMsg (Action.NewFile |> Action)
    | IconDock.SaveAs -> state, Cmd.ofMsg (Action.SaveAsDialog |> Action)
    | IconDock.Open -> state, Cmd.ofMsg (Action.OpenFileDialog |> Action)
    | IconDock.NewFlower -> state, Cmd.ofMsg (Action.NewFlower |> Action)

let sendCommand (command: Command) (state: State) : Cmd<Msg> =
    let selectedFlowerOption =
        Option.bind (fun flowerId -> getFlower flowerId state.Flowers) state.Selected

    match state.SerialPort, selectedFlowerOption with
    | Some serialPort, Some flower ->
        Log.debug $"Sending command through serial port '{command}'"
        Cmd.OfTask.attempt (Command.sendCommand serialPort flower.I2cAddress) command CouldNotSendCommand

    | None, Some _ ->
        Log.warning "Serial port is not selected, cannot send command."
        Cmd.none

    | Some _, None ->
        Log.warning "Flower is not selected, cannot send command."
        Cmd.none

    | None, None ->
        Log.error "An unknown error occured when trying to send command."
        Cmd.none


let updateFlowerPanel (msg: FlowerPanel.Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | FlowerPanel.FlowerPropertiesMsg flowerPropertiesMsg ->
        match flowerPropertiesMsg with
        | FlowerProperties.ChangeName (id, newName) -> updateFlower id "Name" Flower.setName newName state, Cmd.none

        | FlowerProperties.ChangeI2cAddress (id, i2cAddressString) ->
            match String.parseByte i2cAddressString with
            | Some i2cAddress -> updateFlower id "I2C Address" Flower.setI2cAddress i2cAddress state, Cmd.none

            | None ->
                // Todo: handle invalid I2C Address
                state, Cmd.none

    | FlowerPanel.FlowerCommandsMsg flowerCommandsMsg ->
        match flowerCommandsMsg with
        | FlowerCommands.ChangePort newPort ->
            match state.SerialPort with
            | Some serialPort when newPort = FlowerCommands.noPort ->
                Log.verbose $"Disconnecting from serial port '{serialPort.PortName}'"

                state,
                Cmd.batch [
                    Cmd.OfTask.perform Command.closeSerialPort serialPort SerialPortClosed
                ]

            | Some serialPort ->
                Log.verbose $"Changing from serial port '{serialPort.PortName}' to '{newPort}'"

                state,
                Cmd.batch [
                    Cmd.OfTask.perform Command.closeSerialPort serialPort SerialPortClosed
                    Cmd.OfTask.perform Command.openSerialPort newPort SerialPortOpened
                ]

            | None when newPort = FlowerCommands.noPort -> state, Cmd.none

            | None ->
                Log.verbose $"Selected serial port '{newPort}'"
                state, Cmd.OfTask.perform Command.openSerialPort newPort SerialPortOpened

        | FlowerCommands.ChangePercentage (id, percentage) ->
            updateFlower id "Open Percentage" Flower.setOpenPercent percentage state, Cmd.none

        | FlowerCommands.ChangeSpeed (id, speed) -> updateFlower id "Speed" Flower.setSpeed speed state, Cmd.none

        | FlowerCommands.ChangeAcceleration (id, acceleration) ->
            updateFlower id "Acceleration" Flower.setAcceleration acceleration state, Cmd.none

        | FlowerCommands.SendCommand command -> state, Cmd.ofMsg (SendCommand command)


let updateSimulationEvent (msg: SimulationEvent) (state: State) : State * Cmd<Msg> =
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

            { state with
                  FlowerInteraction = Hovering flowerId },
            Cmd.none

        | FlowerPointerEvent.OnLeave (flowerId, _) ->
            Log.verbose $"Flower: Pointer Left {Id.shortName flowerId}"

            { state with
                  FlowerInteraction = NoInteraction },
            Cmd.none

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
                let newPosition = e.Position + draggingData.DraggingDelta

                { state with
                      Flowers = Map.update draggingData.Id (Flower.setPosition newPosition) state.Flowers },
                Cmd.none

            // Take no action
            | _ -> state, Cmd.none


        | FlowerPointerEvent.OnPressed (flowerId, e) ->
            if InputTypes.isPrimary e.MouseButton then
                let maybeFlower = getFlower flowerId state.Flowers

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

                    { state with
                          FlowerInteraction = Hovering flowerId },
                    Cmd.none

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
    | ActionError error -> updateActionError error state

    | SerialPortOpened serialPort ->
        Log.debug $"Connected to serial port '{serialPort.PortName}'"

        { state with
              SerialPort = Some serialPort },
        Command.onReceived serialPort SerialPortReceivedData

    | SerialPortClosed serialPort ->
        Log.verbose $"Closed serial port '{serialPort.PortName}'"
        state, Cmd.none

    | SendCommand command -> state, sendCommand command state

    | SerialPortReceivedData str ->
        Log.info $"Received message over serial{Environment.NewLine}{str}"
        state, Cmd.none

    | CouldNotSendCommand exn ->
        Log.error $"Could not send command over the serial port{Environment.NewLine}{exn}"
        state, Cmd.none

    // Msg Mapping
    | MenuMsg menuMsg -> updateMenu menuMsg state
    | IconDockMsg iconDockMsg -> updateIconDock iconDockMsg state
    | FlowerPanelMsg flowerPanelMsg -> updateFlowerPanel flowerPanelMsg state
    | SimulationEvent event -> updateSimulationEvent event state


// ---- View Functions ----

open Avalonia.FuncUI.Types
open Avalonia.FuncUI.DSL

let drawFlower (state: State) (dispatch: FlowerPointerEvent -> unit) (flower: Flower) : IView =
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

let simulationSpace state (dispatch: SimulationEvent -> unit) : IView =
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
            Events.pointerReleased Constants.CanvasId
            >> Option.map (
                BackgroundEvent.OnReleased
                >> SimulationEvent.BackgroundEvent
                >> dispatch
            )
            >> Option.defaultValue ()
        )
    ]
    :> IView


let view (state: State) (dispatch: Msg -> unit) =
    let selected =
        Option.bind (fun id -> getFlower id state.Flowers) state.Selected

    DockPanel.create [
        DockPanel.children [
            DockPanel.child Dock.Top (IconDock.view (IconDockMsg >> dispatch))
            DockPanel.child Dock.Left (FlowerPanel.view selected state.SerialPort (FlowerPanelMsg >> dispatch))
            simulationSpace state (Msg.SimulationEvent >> dispatch)
        ]
    ]

// ---- Main Window Creation ----


type MainWindow() as this =
    inherit HostWindow()

    do
        base.Title <- Theme.title
        base.Width <- Theme.window.width
        base.Height <- Theme.window.height
        base.MinHeight <- Theme.window.height
        base.MinWidth <- Theme.window.width
        base.Icon <- Theme.icon ()
        base.SystemDecorations <- SystemDecorations.Full

        // Can be turned on during debug
        // this.VisualRoot.VisualRoot.Renderer.DrawFps <- true
        // this.VisualRoot.VisualRoot.Renderer.DrawDirtyRects <- true

        Menu.setMenu this

        let updateWithServices (msg: Msg) (state: State) = update msg state this

        Program.mkProgram init updateWithServices view
        |> Program.withSubscription (Menu.subscription MenuMsg)
        |> Program.withSubscription (keyUpHandler this)
        |> Program.withHost this
        |> Program.run
