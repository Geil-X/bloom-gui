module FlowerGui.Shell

open Avalonia.FuncUI.Types
open Avalonia.Layout
open Elmish
open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI
open Avalonia.FuncUI.Components.Hosts
open Avalonia.FuncUI.Elmish

open FlowerGui.Widgets
open Extensions

type State =
    { aboutState: About.State
      counterState: Counter.State }

type Action =
    | Save
    | Load
    | Open
    | Undo
    | Redo
    | NewFlower

type Msg =
    | AboutMsg of About.Msg
    | CounterMsg of Counter.Msg
    | Action of Action

let init =
    let aboutState, aboutCmd = About.init
    let counterState = Counter.init

    { aboutState = aboutState
      counterState = counterState },
    Cmd.batch [ aboutCmd ]

let update (msg: Msg) (state: State) : State * Cmd<_> =
    match msg with
    | AboutMsg bpMsg ->
        let aboutState, cmd = About.update bpMsg state.aboutState

        { state with aboutState = aboutState }, Cmd.map AboutMsg cmd

    | CounterMsg counterMsg ->
        let counterMsg =
            Counter.update counterMsg state.counterState

        { state with counterState = counterMsg }, Cmd.none

    | Action action ->
        match action with
        | Save -> state, Cmd.none
        | Load -> state, Cmd.none
        | Open -> state, Cmd.none
        | NewFlower -> state, Cmd.none


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
          Icons.newIcon Theme.colors.offWhite, NewFlower
           ]
        |> List.map
            (fun (icon, action) -> Form.imageButton icon (Event.handleEvent (Action action) >> dispatch) :> IView)

    StackPanel.create [
        StackPanel.orientation Orientation.Horizontal
        StackPanel.children buttons
    ]

let flowerProperties =
    StackPanel.create [
        StackPanel.children [
            TextBlock.create [ TextBlock.text "Flower Name" ]
        ]
    ]

let simulationSpace =
    Canvas.create [ Canvas.background "#383838" ]


let view (state: State) (dispatch: Msg -> unit) =
    let panels : IView list =
        [ View.withAttr (Menu.dock Dock.Top) menu
          View.withAttr (StackPanel.dock Dock.Top) (iconDock dispatch)
          View.withAttr (StackPanel.dock Dock.Left) flowerProperties
          simulationSpace ]

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
