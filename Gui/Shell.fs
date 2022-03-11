module Gui.Shell

open Avalonia.Controls
open Elmish
open Avalonia.FuncUI.Elmish
open Avalonia.FuncUI.Components.Hosts

open Geometry
open Gui
open Gui.Menu
open Gui.Panels
open Gui.Widgets
open Extensions


// ---- States ----

type State =
    { CanvasSize: Size<Pixels>
      Flowers: Map<Flower.Id, Flower.State>
      FlowerInteraction: FlowerInteraction
      Selected: Flower.Id option }

and FlowerInteraction =
    | Hovering of Flower.Id
    | Pressing of PressedData
    | Dragging of DraggingData
    | NoInteraction

and PressedData =
    { Id: Flower.Id
      MousePressedLocation: Point2D<Pixels, UserSpace>
      InitialFlowerPosition: Point2D<Pixels, UserSpace> }

and DraggingData =
    { Id: Flower.Id
      DraggingDelta: Vector2D<Pixels, UserSpace> }


// ---- Messaging ----

[<RequireQualifiedAccess>]
type Action =
    | NewFile
    | SaveAsDialog
    | ErrorPickingSaveFile
    | SaveAs of string
    | ErrorSavingFile of exn
    | OpenFileDialog
    | ErrorPickingFileToOpen
    | OpenFile of string
    | FileOpened of Flower.State seq
    | CouldNotOpenFile of exn
    | NewFlower

[<RequireQualifiedAccess>]
type BackgroundEvent = OnReleased of MouseButtonEvent<Pixels, UserSpace>

[<RequireQualifiedAccess>]
type public FlowerPointerEvent =
    | OnEnter of Flower.Id * MouseEvent<Pixels, UserSpace>
    | OnLeave of Flower.Id * MouseEvent<Pixels, UserSpace>
    | OnMoved of Flower.Id * MouseEvent<Pixels, UserSpace>
    | OnPressed of Flower.Id * MouseButtonEvent<Pixels, UserSpace>
    | OnReleased of Flower.Id * MouseButtonEvent<Pixels, UserSpace>

type SimulationEvent =
    | BackgroundEvent of BackgroundEvent
    | FlowerEvent of FlowerPointerEvent


type Msg =
    | IconDockMsg of IconDock.Msg
    | FlowerPanelMsg of FlowerPanel.Msg
    | Action of Action
    | SimulationEvent of SimulationEvent
    | MenuMsg of Menu.Msg


// ---- Init ----

let init () =
    { CanvasSize = Size.create Length.zero Length.zero
      Flowers = Map.empty
      FlowerInteraction = NoInteraction
      Selected = None },
    Cmd.batch []


// ---- Update helper functions -----

let minMouseMovement = Length.pixels 10.
let minMouseMovementSquared = Length.square minMouseMovement

let selectedFlower id flowers : Flower.State option = Map.tryFind id flowers

let newFile (state: State) (flowers: Flower.State seq) : State =
    let flowerMap =
        Seq.fold (fun map (flower: Flower.State) -> Map.add flower.Id flower map) Map.empty flowers

    { state with
          Flowers = flowerMap
          FlowerInteraction = NoInteraction
          Selected = None }

let addFlower (flower: Flower.State) (state: State) : State =
    { state with
          Flowers = Map.add flower.Id flower state.Flowers }

let addFlowers (flowers: Flower.State seq) (state: State) : State =
    let flowerMap =
        Seq.fold (fun map (flower: Flower.State) -> Map.add flower.Id flower map) Map.empty flowers

    { state with Flowers = flowerMap }

let addNewFlower (state: State) : State =
    let flower =
        Flower.basic $"Flower {Map.count state.Flowers + 1}"
        |> Flower.setPosition (Point2D.pixels 100. 100.)

    addFlower flower state


// ---- Update ----

let update (msg: Msg) (state: State) (window: Window) : State * Cmd<Msg> =
    match msg with
    | Action action ->
        match action with
        | Action.NewFile -> newFile state Seq.empty, Cmd.none

        | Action.SaveAsDialog -> state, Cmd.OfTask.perform FlowerFile.saveFileDialog window (Action.SaveAs >> Action)

        | Action.ErrorPickingSaveFile ->
            Log.error "Could not pick file to save as"
            state, Cmd.none

        | Action.SaveAs path ->
            state,
            Cmd.OfTask.attempt
                FlowerFile.writeFlowerFile
                (path, Map.values state.Flowers)
                (Action.ErrorSavingFile >> Action)

        | Action.ErrorSavingFile exn ->
            Log.error $"Could not save file {exn}"
            state, Cmd.none

        | Action.OpenFileDialog ->
            state,
            Cmd.OfTask.perform
                FlowerFile.openFileDialog
                window
                (Option.split Action.OpenFile Action.ErrorPickingFileToOpen
                 >> Action)

        | Action.ErrorPickingFileToOpen ->
            Log.error "Could not pick file to open"
            state, Cmd.none

        | Action.OpenFile path ->
            state,
            Cmd.OfTask.either
                FlowerFile.loadFlowerFile
                path
                (Action.FileOpened >> Action)
                (Action.CouldNotOpenFile >> Action)

        | Action.FileOpened flowers -> newFile state flowers, Cmd.none

        | Action.CouldNotOpenFile exn ->
            Log.error $"Could not open file\n{exn}"
            state, Cmd.none

        | Action.NewFlower -> addNewFlower state, Cmd.none

    | MenuMsg menuMsg ->
        match menuMsg with
        | Menu.FileMsg msg ->
            match msg with
            | File.NewFile -> state, Cmd.ofMsg (Action.NewFile |> Action)
            | File.SaveAs -> state, Cmd.ofMsg (Action.SaveAsDialog |> Action)
            | File.OpenFile -> state, Cmd.ofMsg (Action.OpenFileDialog |> Action)

    | IconDockMsg iconDockMsg ->
        match iconDockMsg with
        | IconDock.NewFile -> state, Cmd.ofMsg (Action.NewFile |> Action)
        | IconDock.SaveAs -> state, Cmd.ofMsg (Action.SaveAsDialog |> Action)
        | IconDock.Open -> state, Cmd.ofMsg (Action.OpenFileDialog |> Action)
        | IconDock.NewFlower -> state, Cmd.ofMsg (Action.NewFlower |> Action)

    | FlowerPanelMsg flowerPanelMsg ->
        match flowerPanelMsg with
        | FlowerPanel.FlowerPropertiesMsg flowerPropertiesMsg ->
            match flowerPropertiesMsg with
            | FlowerProperties.ChangeName (id, newName) ->
                if Option.contains id state.Selected then
                    Log.verbose $"Updated flower '{Guid.shortName id}' with new name '{newName}'"

                    { state with
                          Flowers = Map.update id (Flower.setName newName) state.Flowers },
                    Cmd.none
                else
                    state, Cmd.none

            | FlowerProperties.ChangeI2cAddress (id, i2cAddressString) ->
                if Option.contains id state.Selected then
                    match String.parseUint i2cAddressString with
                    | Some i2cAddress ->
                        Log.verbose $"Updated flower '{Guid.shortName id}' with new I2C Address '{i2cAddress}'"

                        { state with
                              Flowers = Map.update id (Flower.setI2cAddress i2cAddress) state.Flowers },
                        Cmd.none
                    | None ->
                        // Todo: handle invalid I2C Address
                        state, Cmd.none
                else
                    state, Cmd.none
                    
        | FlowerPanel.FlowerCommandsMsg flowerCommandsMsg ->
            match flowerCommandsMsg with
            | FlowerCommands.ChangePercentage (id, percentage) ->
                { state with
                      Flowers = Map.update id (Flower.setOpenPercent percentage) state.Flowers },
                Cmd.none


    | SimulationEvent event ->
        match event with
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
                Log.verbose $"Flower: Hovering {Guid.shortName flowerId}"

                { state with
                      FlowerInteraction = Hovering flowerId },
                Cmd.none

            | FlowerPointerEvent.OnLeave (flowerId, _) ->
                Log.verbose $"Flower: Pointer Left {Guid.shortName flowerId}"

                { state with
                      FlowerInteraction = NoInteraction },
                Cmd.none

            | FlowerPointerEvent.OnMoved (flowerId, e) ->
                match state.FlowerInteraction with
                | Pressing pressing when
                    pressing.Id = flowerId
                    && Point2D.distanceSquaredTo pressing.MousePressedLocation e.Position > minMouseMovementSquared
                    ->
                    Log.verbose $"Flower: Start Dragging {Guid.shortName flowerId}"

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
                    let maybeFlower = selectedFlower flowerId state.Flowers

                    match maybeFlower with
                    | Some pressed ->
                        Log.verbose $"Flower: Pressed {Guid.shortName pressed.Id}"

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
                        Log.verbose $"Flower: Dragging -> Hovering {Guid.shortName flowerId}"

                        { state with
                              FlowerInteraction = Hovering flowerId },
                        Cmd.none

                    | Pressing _ ->
                        Log.verbose $"Flower: Selected {Guid.shortName flowerId}"

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


// ---- View Functions ----

open Avalonia.FuncUI.Types
open Avalonia.FuncUI.DSL

let drawFlower (state: State) (dispatch: FlowerPointerEvent -> Unit) (flower: Flower.State) : IView =
    let flowerState (flower: Flower.State) : Flower.Attribute option =
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
        Option.bind (fun id -> selectedFlower id state.Flowers) state.Selected

    DockPanel.create [
        DockPanel.children [
            DockPanel.child Dock.Top (IconDock.view (IconDockMsg >> dispatch))
            DockPanel.child Dock.Left (FlowerPanel.view selected (FlowerPanelMsg >> dispatch))
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
        base.Icon <- Theme.icon
        this.HasSystemDecorations <- true

        // Can be turned on during debug
        // this.VisualRoot.VisualRoot.Renderer.DrawFps <- true
        // this.VisualRoot.VisualRoot.Renderer.DrawDirtyRects <- true

        Menu.setMenu this

        let updateWithServices (msg: Msg) (state: State) = update msg state this

        Program.mkProgram init updateWithServices view
        |> Program.withSubscription (Menu.subscription MenuMsg)
        |> Program.withHost this
        |> Program.run
