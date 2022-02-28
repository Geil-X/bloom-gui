module Gui.Shell

open Elmish
open Gui.Widgets
open Extensions

open Geometry
open Gui


// ---- Types ----

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


// ----

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
type public FlowerEvent =
    | OnEnter of Flower.Id * MouseEvent<Pixels, UserSpace>
    | OnLeave of Flower.Id * MouseEvent<Pixels, UserSpace>
    | OnMoved of Flower.Id * MouseEvent<Pixels, UserSpace>
    | OnPressed of Flower.Id * MouseButtonEvent<Pixels, UserSpace>
    | OnReleased of Flower.Id * MouseButtonEvent<Pixels, UserSpace>

type SimulationEvent =
    | BackgroundEvent of BackgroundEvent
    | FlowerEvent of FlowerEvent


type Msg =
    | ChangeName of (Flower.Id * string)
    | Action of Action
    | SimulationEvent of SimulationEvent


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

let addFlower (flower: Flower.State) (state: State) : State =
    { state with
          Flowers = Map.add flower.Id flower state.Flowers }

let addNewFlower (state: State) : State =
    let flower =
        Flower.basic $"Flower {Map.count state.Flowers}"

    { state with
          Flowers = Map.add flower.Id flower state.Flowers }

// ---- Update ----

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ChangeName (id, name) ->
        { state with
              Flowers =
                  state.Flowers
                  |> Map.update id (Flower.setName name) },
        Cmd.none

    | Action action ->
        match action with
        | Action.Save -> state, Cmd.none
        | Action.Load -> state, Cmd.none
        | Action.Undo -> state, Cmd.none
        | Action.Redo -> state, Cmd.none
        | Action.Open -> state, Cmd.none
        | Action.NewFlower ->
            let newFlower =
                Flower.basic $"Flower {Map.count state.Flowers}"
                |> Flower.setPosition (Point2D.pixels 100. 100.)

            { state with
                  Flowers = Map.add newFlower.Id newFlower state.Flowers },
            Cmd.none

    | SimulationEvent event ->
        match event with
        | BackgroundEvent backgroundEvent ->
            match backgroundEvent with
            | BackgroundEvent.OnReleased _ ->
                printfn "Background: Pointer Released"
                { state with
                      FlowerInteraction = NoInteraction
                      Selected = None }, Cmd.none

        | FlowerEvent flowerEvent ->
            match flowerEvent with
            | FlowerEvent.OnEnter (flowerId, _) ->
                printfn $"Flower: Hovering {flowerId}"

                { state with
                      FlowerInteraction = Hovering flowerId },
                Cmd.none

            | FlowerEvent.OnLeave (flowerId, _) ->
                printfn $"Flower: Pointer Left {flowerId}"

                { state with
                      FlowerInteraction = NoInteraction },
                Cmd.none

            | FlowerEvent.OnMoved (flowerId, e) ->
                match state.FlowerInteraction with
                | Pressing pressing when
                    pressing.Id = flowerId
                    && Point2D.distanceSquaredTo pressing.MousePressedLocation e.Position > minMouseMovementSquared
                    ->
                    printfn $"Flower: Start Dragging {flowerId}"

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


            | FlowerEvent.OnPressed (flowerId, e) ->
                if InputTypes.isPrimary e.MouseButton then
                    let maybeFlower = selectedFlower flowerId state.Flowers

                    match maybeFlower with
                    | Some pressed ->
                        printfn $"Flower: Pressed {pressed.Id}"

                        { state with
                              FlowerInteraction =
                                  Pressing
                                      { Id = flowerId
                                        MousePressedLocation = e.Position
                                        InitialFlowerPosition = pressed.Position } },
                        Cmd.none

                    | None ->
                        printfn "Could not find the flower that was pressed"
                        state, Cmd.none
                else
                    state, Cmd.none

            | FlowerEvent.OnReleased (flowerId, e) ->
                if InputTypes.isPrimary e.MouseButton then
                    match state.FlowerInteraction with
                    | Dragging _ ->
                        printfn $"Flower: Dragging -> Hovering {flowerId}"

                        { state with
                              FlowerInteraction = Hovering flowerId },
                        Cmd.none

                    | Pressing _ ->
                        printfn $"Flower: Selected {flowerId}"

                        { state with
                              FlowerInteraction = Hovering flowerId
                              Selected = Some flowerId },
                        Cmd.none


                    | flowerEvent ->
                        printfn $"Unhandled event {flowerEvent}"
                        state, Cmd.none

                // Non primary button pressed
                else
                    state, Cmd.none




// ---- View Functions ----

open Avalonia.FuncUI.Types
open Avalonia.Layout
open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI
open Avalonia.FuncUI.Components.Hosts
open Avalonia.FuncUI.Elmish

let menu =
    let fileItems: IView list =
        [ MenuItem.create [ MenuItem.header "Open" ]
          MenuItem.create [ MenuItem.header "Save" ]
          MenuItem.create [ MenuItem.header "Save As" ] ]

    let editItems: IView list =
        [ MenuItem.create [ MenuItem.header "Undo" ]
          MenuItem.create [ MenuItem.header "Redo" ] ]


    let menuItems: IView list =
        [ MenuItem.create [
            MenuItem.header "File"
            MenuItem.viewItems fileItems
          ]
          MenuItem.create [
              MenuItem.header "Edit"
              MenuItem.viewItems editItems
          ] ]

    Menu.create [ Menu.viewItems menuItems ]

let iconDock (dispatch: Msg -> Unit) =
    let buttons: IView list =
        [ Icons.save Theme.colors.offWhite, Action.Save
          Icons.load Theme.colors.offWhite, Action.Load
          Icons.undo Theme.colors.offWhite, Action.Undo
          Icons.redo Theme.colors.offWhite, Action.Redo
          Icons.newIcon Theme.colors.offWhite, Action.NewFlower ]
        |> List.map
            (fun (icon, action) -> Form.imageButton icon (Event.handleEvent (Action action) >> dispatch) :> IView)

    StackPanel.create [
        StackPanel.orientation Orientation.Horizontal
        StackPanel.children buttons
    ]

let flowerProperties state (dispatch: Msg -> Unit) =
    let selectedName =
        state.Selected
        |> Option.bind (fun id -> selectedFlower id state.Flowers)
        |> Option.map (fun flower -> flower.Name)
        |> Option.defaultValue "All"


    StackPanel.create [
        StackPanel.children [
            Form.textItem
                {| Name = "Name"
                   Value = selectedName
                   OnChange =
                       fun name ->
                           match state.Selected with
                           | Some id -> ChangeName(id, name) |> dispatch
                           | None -> ()
                   LabelPlacement = Orientation.Vertical |}
        ]
        StackPanel.minWidth 150.
    ]


let drawFlower (state: State) (dispatch: FlowerEvent -> Unit) (flower: Flower.State) : IView =
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

          Flower.onPointerEnter (fun e -> FlowerEvent.OnEnter(flower.Id, e) |> dispatch)
          Flower.onPointerLeave (fun e -> FlowerEvent.OnLeave(flower.Id, e) |> dispatch)
          Flower.onPointerMoved (fun e -> FlowerEvent.OnMoved(flower.Id, e) |> dispatch)
          Flower.onPointerPressed (fun e -> FlowerEvent.OnPressed(flower.Id, e) |> dispatch)
          Flower.onPointerReleased (fun e -> FlowerEvent.OnReleased(flower.Id, e) |> dispatch) ]
    :> IView

let simulationSpace state (dispatch: SimulationEvent -> Unit) : IView =
    let flowers: IView list =
        Map.values state.Flowers
        |> Seq.map (drawFlower state (SimulationEvent.FlowerEvent >> dispatch))
        |> Seq.toList

    Canvas.create [
        Canvas.children flowers
        Canvas.background Theme.palette.canvasBackground
        Canvas.name Constants.CanvasId
        // Todo: this event is triggering first and should trigger last
//        Canvas.onPointerReleased (
//            Events.pointerReleased Constants.CanvasId
//            >> BackgroundEvent.OnReleased
//            >> SimulationEvent.BackgroundEvent
//            >> dispatch
//        )
        ]
    :> IView


let view (state: State) (dispatch: Msg -> unit) =
    let panels: IView list =
        [ View.withAttr (Menu.dock Dock.Top) menu
          View.withAttr (StackPanel.dock Dock.Top) (iconDock dispatch)
          View.withAttr (StackPanel.dock Dock.Left) (flowerProperties state dispatch)
          simulationSpace state (Msg.SimulationEvent >> dispatch) ]

    DockPanel.create [ DockPanel.children panels ]

type MainWindow() as this =
    inherit HostWindow()

    do
        base.Title <- "Full App"
        base.Width <- Theme.window.width
        base.Height <- Theme.window.height
        base.MinWidth <- Theme.window.width
        base.MinHeight <- Theme.window.height

        //this.VisualRoot.VisualRoot.Renderer.DrawFps <- true
        //this.VisualRoot.VisualRoot.Renderer.DrawDirtyRects <- true

        Elmish.Program.mkProgram init update view
        |> Program.withHost this
        |> Program.run
