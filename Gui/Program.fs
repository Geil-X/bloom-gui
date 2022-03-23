namespace Gui

open System
open Avalonia
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.FuncUI
open Avalonia.Themes.Default

open AvaloniaStyles
open Gui.Views.Styles

/// This is your application you can ose the initialize method to load styles
/// or handle Life Cycle events of your application
type App() =
    inherit Application()

    override this.Initialize() =
        //        this.Styles.Load "avares://Avalonia.Themes.Default/DefaultTheme.xaml"
//        this.Styles.Load "avares://Avalonia.Themes.Default/Accents/BaseDark.xaml"
//        this.Styles.Load "avares://Gui/Styles.xaml"
//        this.Styles.Add(DefaultTheme())
        this.Styles.Add(Theme(Uri "avares://ControlCatalog/Styles"))

        this.Name <- Theme.program

    override this.OnFrameworkInitializationCompleted() =
        match this.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as desktopLifetime ->
            desktopLifetime.MainWindow <- Shell.MainWindow()
        | _ -> ()

module Program =

    [<EntryPoint>]
    let main (args: string []) =
        AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .UseSkia()
            .StartWithClassicDesktopLifetime(args)
