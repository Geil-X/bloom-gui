module FlowerGui.Shell

open Avalonia.FuncUI.Types
open Avalonia.Layout
open Elmish
open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI
open Avalonia.FuncUI.Components.Hosts
open Avalonia.FuncUI.Elmish

open Extensions

type State =
    { aboutState: About.State
      counterState: Counter.State }

type Msg =
    | AboutMsg of About.Msg
    | CounterMsg of Counter.Msg

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

let iconDock =
    StackPanel.create [
        StackPanel.orientation Orientation.Horizontal
        StackPanel.children [
            TextBlock.create [ TextBlock.text "Text 1" ]
            TextBlock.create [ TextBlock.text "Text 2" ]
            TextBlock.create [ TextBlock.text "Text 3" ]
        ]
    ]

let flowerProperties =
    StackPanel.create [
        StackPanel.children [
            TextBlock.create [ TextBlock.text "Flower Name" ]
        ]
    ]
    
let simulationSpace =
    Canvas.create [
        Canvas.background "#383838"
    ]


let view (state: State) (dispatch: Msg -> unit) =
    let panels : IView list =
        [ View.withAttr (Menu.dock Dock.Top) menu
          View.withAttr (StackPanel.dock Dock.Top) iconDock
          View.withAttr (StackPanel.dock Dock.Left) flowerProperties
          simulationSpace
          ]

    DockPanel.create [ DockPanel.children panels ]

type MainWindow() as this =
    inherit HostWindow()

    do
        base.Title <- "Full App"
        base.Width <- 800.0
        base.Height <- 600.0
        base.MinWidth <- 800.0
        base.MinHeight <- 600.0

        //this.VisualRoot.VisualRoot.Renderer.DrawFps <- true
        //this.VisualRoot.VisualRoot.Renderer.DrawDirtyRects <- true

        Elmish.Program.mkProgram (fun () -> init) update view
        |> Program.withHost this
        |> Program.run
