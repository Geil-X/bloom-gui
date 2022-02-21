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
      Flowers: Map<FlowerId, Flower>
      Selected: FlowerId option }

type Action =
    | Save
    | Load
    | Open
    | Undo
    | Redo
    | NewFlower

type Msg =
    | ChangeName of (FlowerId * string)
    | Action of Action

// State accessors

let selectedFlower state : Flower option =
    state.Selected
    |> Option.bind (fun id -> Map.tryFind id state.Flowers)

let init =
    { CanvasSize = Size.create Length.zero Length.zero
      Flowers = Map.empty
      Selected = None },
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


let menu =
    let fileItems : IView list =
        [ MenuItem.create [ MenuItem.header "Open" ]
          MenuItem.create [ MenuItem.header "Save" ]
          MenuItem.create [ MenuItem.header "Save As" ] ]

    let editItems : IView list =
        [ MenuItem.create [ MenuItem.header "Undo" ]
          MenuItem.create [ MenuItem.header "Redo" ] ]


    let menuItems : IView list =
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
    let buttons : IView list =
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
    let flowers =
        Map.values state.Flowers
        |> Seq.map Flower.draw
        |> Seq.toList

    Canvas.create [
        Canvas.children flowers
        Canvas.background Theme.palette.canvasBackground
    ]


let view (state: State) (dispatch: Msg -> unit) =
    let panels : IView list =
        [ View.withAttr (Menu.dock Dock.Top) menu
          View.withAttr (StackPanel.dock Dock.Top) (iconDock dispatch)
          View.withAttr (StackPanel.dock Dock.Left) (flowerProperties state dispatch)
          simulationSpace state dispatch ]

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

        Elmish.Program.mkProgram (fun () -> init) update view
        |> Program.withHost this
        |> Program.run
