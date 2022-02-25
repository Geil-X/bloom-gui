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
      MousePressedLocation: Point2D<Pixels, UserSpace> option
      Selected: Flower.Id option }

and FlowerInteraction =
    | Hovering of Flower.Id
    | Pressing of Flower.Id
    | Dragging of Flower.Id
    | NoInteraction

type Msg =
    | ChangeName of (Flower.Id * string)
    | Action of Action
    | FlowerEvent of FlowerEvent

and Action =
    | Save
    | Load
    | Open
    | Undo
    | Redo
    | NewFlower

and FlowerEvent =
    | OnEnter of Flower.Id * MouseEvent<Pixels, UserSpace>
    | OnLeave of Flower.Id * MouseEvent<Pixels, UserSpace>
    | OnMoved of Flower.Id * MouseEvent<Pixels, UserSpace>
    | OnPressed of Flower.Id * MouseButtonEvent<Pixels, UserSpace>
    | OnReleased of Flower.Id * MouseButtonEvent<Pixels, UserSpace>

// ---- Init ----

let init () =
    { CanvasSize = Size.create Length.zero Length.zero
      Flowers = Map.empty
      FlowerInteraction = NoInteraction
      MousePressedLocation = None
      Selected = None },
    Cmd.batch []


// ---- Update helper functions -----

let selectedFlower state : Flower.State option =
    state.Selected
    |> Option.bind (fun id -> Map.tryFind id state.Flowers)

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
        | Save -> state, Cmd.none
        | Load -> state, Cmd.none
        | Undo -> state, Cmd.none
        | Redo -> state, Cmd.none
        | Open -> state, Cmd.none
        | NewFlower ->
            let newFlower =
                Flower.basic $"Flower {Map.count state.Flowers}"
                |> Flower.setPosition (Point2D.pixels 100. 100.)

            { state with
                  Flowers = Map.add newFlower.Id newFlower state.Flowers },
            Cmd.none

    | FlowerEvent flowerEvent ->
        match flowerEvent with
        | OnEnter (flowerId, _) ->
            printfn "Hovering"

            { state with
                  FlowerInteraction = Hovering flowerId },
            Cmd.none

        | OnLeave _ ->
            printfn "Left"

            { state with
                  FlowerInteraction = NoInteraction },
            Cmd.none

        | OnMoved (flowerId, _) ->
            match state.FlowerInteraction with
            | Pressing pressingId when pressingId = flowerId->
                printfn "Dragging"
                { state with
                      FlowerInteraction = Dragging flowerId },
                Cmd.none
                
            // Take no action
            | _ -> state, Cmd.none
                

        | OnPressed (flowerId, e) ->
            if InputTypes.isPrimary e.MouseButton then
                printfn "Pressing"

                { state with
                      FlowerInteraction = Pressing flowerId },
                Cmd.none
            else
                state, Cmd.none

        | OnReleased (flowerId, e) ->
            if InputTypes.isPrimary e.MouseButton then
                printfn "Released"

                match state.FlowerInteraction with
                | Dragging _ ->
                    { state with
                          FlowerInteraction = Hovering flowerId },
                    Cmd.none

                | Pressing _ ->
                    { state with
                          FlowerInteraction = Hovering flowerId
                          Selected = Some flowerId },
                    Cmd.none

                | flowerEvent ->
                    printfn $"Unhandled event {flowerEvent}"
                    state, Cmd.none

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

let iconDock dispatch =
    let buttons: IView list =
        [ Icons.save Theme.colors.offWhite, Save
          Icons.load Theme.colors.offWhite, Load
          Icons.undo Theme.colors.offWhite, Undo
          Icons.redo Theme.colors.offWhite, Redo
          Icons.newIcon Theme.colors.offWhite, NewFlower ]
        |> List.map
            (fun (icon, action) -> Form.imageButton icon (Event.handleEvent (Action action) >> dispatch) :> IView)

    StackPanel.create [
        StackPanel.orientation Orientation.Horizontal
        StackPanel.children buttons
    ]

let flowerProperties state dispatch =
    let selected = selectedFlower state

    let selectedName =
        selected
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

let simulationSpace state dispatch =
    let flowerState (flower: Flower.State) : Flower.Attribute<Pixels, UserSpace> option =
        match state.FlowerInteraction with
        | Hovering id when id = flower.Id -> Flower.hovered |> Some
        | Pressing id when id = flower.Id -> Flower.pressed |> Some
        | Dragging id when id = flower.Id -> Flower.dragged |> Some
        | _ -> None


    let drawFlower flower : IView =
        Flower.draw
            flower
            [ if Option.contains flower.Id state.Selected then
                  Flower.selected
              yield! flowerState flower |> Option.toList

              Flower.onPointerEnter (fun e -> OnEnter(flower.Id, e) |> dispatch)
              Flower.onPointerLeave (fun e -> OnLeave(flower.Id, e) |> dispatch)
              Flower.onPointerMoved (fun e -> OnMoved(flower.Id, e) |> dispatch)
              Flower.onPointerPressed (fun e -> OnPressed(flower.Id, e) |> dispatch)
              Flower.onPointerReleased (fun e -> OnReleased(flower.Id, e) |> dispatch) ]
        :> IView

    let flowers: IView list =
        Map.values state.Flowers
        |> Seq.map drawFlower
        |> Seq.toList

    Canvas.create [
        Canvas.children flowers
        Canvas.background Theme.palette.canvasBackground
        Canvas.name Constants.CanvasId
    ]


let view (state: State) (dispatch: Msg -> unit) =
    let panels: IView list =
        [ View.withAttr (Menu.dock Dock.Top) menu
          View.withAttr (StackPanel.dock Dock.Top) (iconDock dispatch)
          View.withAttr (StackPanel.dock Dock.Left) (flowerProperties state dispatch)
          simulationSpace state (FlowerEvent >> dispatch) ]

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
