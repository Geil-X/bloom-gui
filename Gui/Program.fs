namespace Gui

open Elmish
open Avalonia
open Avalonia.Controls
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.FuncUI
open Avalonia.FuncUI.Hosts
open Avalonia.FuncUI.Elmish
open LibVLCSharp.Shared

open Gui.DataTypes
open Gui.Views.Menu

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

        let updateWithServices (msg: Shell.Msg) (state: Shell.State) = Shell.update msg state this

        Program.mkProgram Shell.init updateWithServices Shell.view
        |> Program.withNativeMenu this (Menu.menuBar AppConfig.init) Shell.MenuMsg
        |> Program.withSubscription (Shell.keyUpHandler this)
        |> Program.withHost this
        |> Program.run

/// This is your application you can ose the initialize method to load styles
/// or handle Life Cycle events of your application
type App() =
    inherit Application()

    do Core.Initialize()

    override this.Initialize() =
        this.Styles.Load "avares://Avalonia.Themes.Default/DefaultTheme.xaml"
        this.Styles.Load "avares://Avalonia.Themes.Default/Accents/BaseDark.xaml"
        this.Styles.Load "avares://Gui/Styles.xaml"
        //        this.Styles.Add(FluentTheme(baseUri = null, Mode = FluentThemeMode.Dark))

        this.Name <- Theme.program

        Log.LogLevel <- Log.Debug

    override this.OnFrameworkInitializationCompleted() =
        match this.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as desktopLifetime -> desktopLifetime.MainWindow <- MainWindow()
        | _ -> ()

module Program =
    let skiaOptions = SkiaOptions()
    skiaOptions.MaxGpuResourceSizeBytes <- 8096000

    [<EntryPoint>]
    let main (args: string []) =
        AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .With(skiaOptions)
            .UseSkia()
            .StartWithClassicDesktopLifetime(args)
