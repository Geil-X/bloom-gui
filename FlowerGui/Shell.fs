namespace FlowerGui

module Shell =
    open Elmish
    open Avalonia
    open Avalonia.Controls
    open Avalonia.Input
    open Avalonia.FuncUI.DSL
    open Avalonia.FuncUI
    open Avalonia.FuncUI.Builder
    open Avalonia.FuncUI.Components.Hosts
    open Avalonia.FuncUI.Elmish

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
        | AboutMsg bpmsg ->
            let aboutState, cmd = About.update bpmsg state.aboutState

            { state with aboutState = aboutState }, Cmd.map AboutMsg cmd

        | CounterMsg countermsg ->
            let counterMsg =
                Counter.update countermsg state.counterState

            { state with counterState = counterMsg },
            Cmd.none

    let view (state: State) (dispatch) =
        DockPanel.create [
            DockPanel.children [
                TabControl.create [
                    TabControl.tabStripPlacement Dock.Top
                    TabControl.viewItems [
                        TabItem.create [
                            TabItem.header "Counter Sample"
                            TabItem.content (Counter.view state.counterState (CounterMsg >> dispatch))
                        ]
                        TabItem.create [
                            TabItem.header "About"
                            TabItem.content (About.view state.aboutState (AboutMsg >> dispatch))
                        ]
                    ]
                ]
            ]
        ]

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
