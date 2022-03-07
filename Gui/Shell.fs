module Gui.Shell

open Avalonia.Controls
open Elmish
open Avalonia.FuncUI.Elmish
open Avalonia.FuncUI.Components.Hosts

open Geometry
open Gui
open Gui.Menu
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
    | Save
    | Load
    | Open
    | Undo
    | Redo
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
    | FlowerPropertiesMsg of FlowerProperties.Msg
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

let addNewFlower (state: State) : State =
    let flower =
        Flower.basic $"Flower {Map.count state.Flowers}"
        |> Flower.setPosition (Point2D.pixels 100. 100.)

    { state with
          Flowers = Map.add flower.Id flower state.Flowers }

// ---- Update ----

let update (msg: Msg) (state: State) (window: Window) : State * Cmd<Msg> =
    match msg with
    | Action action ->
        match action with
        | Action.Save -> state, Cmd.none
        | Action.Load -> state, Cmd.none
        | Action.Undo -> state, Cmd.none
        | Action.Redo -> state, Cmd.none
        | Action.Open -> state, Cmd.none
        | Action.NewFlower -> addNewFlower state, Cmd.none

    | MenuMsg menuMsg ->
        let menuCmd, menuExternal =
            Menu.update menuMsg (Map.values state.Flowers) window

        let newState =
            match menuExternal with
            | Menu.FileExternal fileExternal ->
                match fileExternal with
                | File.External.NewFile -> newFile state Seq.empty

                | File.External.FileLoaded fileResult ->
                    match fileResult with
                    | Ok flowers -> newFile state flowers

                    | Error fileResult ->
                        Log.error $"Error loading flower file {fileResult}"
                        state

                | File.External.SavedFile -> state

                | File.External.ErrorSavingFile fileError ->
                    Log.error $"Error saving flower file {fileError}"
                    state

                | File.External.DoNothing -> state

        newState, Cmd.map MenuMsg menuCmd

    | FlowerPropertiesMsg msg ->
        match msg with
        | FlowerProperties.ChangeName (id, newName) ->
            if Option.contains id state.Selected then
                Log.verbose $"Updated flower '{id}' with new name '{newName}'"

                { state with
                      Flowers = Map.update id (Flower.setName newName) state.Flowers },
                Cmd.none
            else
                state, Cmd.none

        | FlowerProperties.ChangeI2cAddress (id, i2cAddressString) ->
            if Option.contains id state.Selected then
                match String.parseUint i2cAddressString with
                | Some i2cAddress ->
                    Log.verbose $"Updated flower '{id}' with new I2C Address '{i2cAddress}'"

                    { state with
                          Flowers = Map.update id (Flower.setI2cAddress i2cAddress) state.Flowers },
                    Cmd.none
                | None ->
                    // Todo: handle invalid I2C Address
                    state, Cmd.none
            else
                state, Cmd.none


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
                Log.verbose $"Flower: Hovering {flowerId}"

                { state with
                      FlowerInteraction = Hovering flowerId },
                Cmd.none

            | FlowerPointerEvent.OnLeave (flowerId, _) ->
                Log.verbose $"Flower: Pointer Left {flowerId}"

                { state with
                      FlowerInteraction = NoInteraction },
                Cmd.none

            | FlowerPointerEvent.OnMoved (flowerId, e) ->
                match state.FlowerInteraction with
                | Pressing pressing when
                    pressing.Id = flowerId
                    && Point2D.distanceSquaredTo pressing.MousePressedLocation e.Position > minMouseMovementSquared
                    ->
                    Log.verbose $"Flower: Start Dragging {flowerId}"

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
                        Log.verbose $"Flower: Pressed {pressed.Id}"

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
                        Log.verbose $"Flower: Dragging -> Hovering {flowerId}"

                        { state with
                              FlowerInteraction = Hovering flowerId },
                        Cmd.none

                    | Pressing _ ->
                        Log.verbose $"Flower: Selected {flowerId}"

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
open Avalonia.Layout
open Avalonia.FuncUI.DSL

let iconDock (dispatch: Msg -> Unit) =
    let buttons: IView list =
        [ Icons.save Theme.colors.offWhite, Action.Save
          Icons.load Theme.colors.offWhite, Action.Load
          Icons.newIcon Theme.colors.offWhite, Action.NewFlower ]
        |> List.map
            (fun (icon, action) -> Form.imageButton icon (Event.handleEvent (Action action) >> dispatch) :> IView)

    StackPanel.create [ StackPanel.orientation Orientation.Horizontal
                        StackPanel.children buttons ]

let drawFlower (state: State) (dispatch: FlowerPointerEvent -> Unit) (flower: Flower.State) : IView =
    let flowerState (flower: Flower.State) : Flower.Attribute<Pixels, UserSpace> option =
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

    Canvas.create [ Canvas.children flowers
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
                    ) ]
    :> IView


let view (state: State) (dispatch: Msg -> unit) =
    let selected =
        Option.bind (fun id -> selectedFlower id state.Flowers) state.Selected

    let panels: IView list =
        [ View.withAttr (Menu.dock Dock.Top) (Menu.view (MenuMsg >> dispatch))
          View.withAttr (StackPanel.dock Dock.Top) (iconDock dispatch)
          View.withAttr (StackPanel.dock Dock.Left) (FlowerProperties.view selected (FlowerPropertiesMsg >> dispatch))
          simulationSpace state (Msg.SimulationEvent >> dispatch) ]

    DockPanel.create [ DockPanel.children panels ]

// ---- Main Window Creation ----


type MainWindow() as this =
    inherit HostWindow()

    do
        base.Title <- Theme.title
        base.Width <- Theme.window.width
        base.Height <- Theme.window.height
        base.MinHeight <- Theme.window.height
        base.MinWidth <- Theme.window.width
        this.HasSystemDecorations <- true

        //this.VisualRoot.VisualRoot.Renderer.DrawFps <- true
        //this.VisualRoot.VisualRoot.Renderer.DrawDirtyRects <- true

        let updateWithServices (msg: Msg) (state: State) = update msg state this


        Program.mkProgram init updateWithServices view
        |> Program.withHost this
        |> Program.run
