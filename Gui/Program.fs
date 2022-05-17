namespace Gui

open Avalonia
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.FuncUI

/// This is your application you can ose the initialize method to load styles
/// or handle Life Cycle events of your application
type App() =
    inherit Application()

    override this.Initialize() =
        this.Styles.Load "avares://Avalonia.Themes.Default/DefaultTheme.xaml"
        this.Styles.Load "avares://Avalonia.Themes.Default/Accents/BaseDark.xaml"
        this.Styles.Load "avares://Gui/Styles.xaml"
//        this.Styles.Add(FluentTheme(baseUri = null, Mode = FluentThemeMode.Dark))

        this.Name <- Theme.program
        
        Log.LogLevel <- Log.Verbose

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
