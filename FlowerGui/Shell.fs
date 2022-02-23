module FlowerGui.Shell

open Avalonia.FuncUI.Types
open Avalonia.Layout
open Elmish
open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI
open Avalonia.FuncUI.Components.Hosts
open Avalonia.FuncUI.Elmish
open Geometry

open FlowerGui.Widgets
open Extensions


type State =
    { CanvasSize: Size<Pixels>
      Flowers: Map<Flower.Id, Flower.State>
      Selected: Flower.Id option
      Hovered: Flower.Id option
      Pressed: Flower.Id option
      Dragging: Flower.Id option }

type Msg =
    | ChangeName of (Flower.Id * string)
    | Action of Action
    | FlowerInteraction of FlowerInteraction

and Action =
    | Save
    | Load
    | Open
    | Undo
    | Redo
    | NewFlower

and FlowerInteraction =
    | Hovered of Flower.Id
    | Unhovered of Flower.Id
    | Selected of Flower.Id
    | Pressed of Flower.Id
    | Dragged of Flower.Id

// State accessors

let selectedFlower state : Flower.State option =
    state.Selected
    |> Option.bind (fun id -> Map.tryFind id state.Flowers)

let init =
    { CanvasSize = Size.create Length.zero Length.zero
      Flowers = Map.empty
      Selected = None
      Hovered = None
      Pressed = None
      Dragging = None },
    Cmd.batch []

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
                  Flowers = Map.add newFlower.Id newFlower state.Flowers
                  Selected = Some newFlower.Id },
            Cmd.none

    | FlowerInteraction interaction ->
        match interaction with
        | Hovered id ->
            printfn "Hovered"
            { state with Hovered = Some id }, Cmd.none
        | Unhovered _ ->
            printfn "Unhovered"
            { state with Hovered = None }, Cmd.none
        | Selected id ->
            printfn "Selected"
            { state with Selected = Some id }, Cmd.none
        | Pressed id ->
            printfn "Pressed"
            { state with Pressed = Some id }, Cmd.none
        | Dragged id ->
            printfn "Dragged"
            { state with Dragging = Some id }, Cmd.none

let menu =
    let fileItems: IView list =
        [ MenuItem.create [
            MenuItem.header "Open"
          ]
          MenuItem.create [
              MenuItem.header "Save"
          ]
          MenuItem.create [
              MenuItem.header "Save As"
          ] ]

    let editItems: IView list =
        [ MenuItem.create [
            MenuItem.header "Undo"
          ]
          MenuItem.create [
              MenuItem.header "Redo"
          ] ]


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
    let drawFlower flower : IView =
        Flower.draw
            flower
            //            [ if Option.contains flower.Id state.Pressed then
//                  Flower.pressed
            [ if Option.contains flower.Id state.Hovered then
                  Flower.hovered
              //              if Option.contains flower.Id state.Selected then
//                  Flower.selected
//              if Option.contains flower.Id state.Dragging then
//                  Flower.dragged

              Flower.onHover (fun () -> FlowerInteraction.Hovered flower.Id |> dispatch)
              Flower.onUnhover (fun () -> FlowerInteraction.Unhovered flower.Id |> dispatch)
              Flower.onPressed (fun () -> FlowerInteraction.Pressed flower.Id |> dispatch)
              Flower.onSelected (fun () -> FlowerInteraction.Selected flower.Id |> dispatch) ]

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
          simulationSpace state (FlowerInteraction >> dispatch) ]

    DockPanel.create [
        DockPanel.children panels
    ]

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

        Elmish.Program.mkProgram (fun () -> init) update view
        |> Program.withHost this
        |> Program.run
