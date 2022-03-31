module Gui.Views.Wip.SimpleTheme

open System
open Avalonia
open Avalonia.Controls
open Avalonia.Markup.Xaml
open Avalonia.Markup.Xaml.Styling
open Avalonia.Styling

type SimpleThemeMode =
    | Light
    | Dark

type SimpleTheme(baseUri: Uri) =
    inherit AvaloniaObject()

    let style path =
        let s = StyleInclude(baseUri)
        s.Source <- Uri path
        s :> IStyle

    let styles paths =
        let s = Styles()
        s.AddRange(List.map style paths)
        s

    new(serviceProvider: IServiceProvider) =
        let service =
            serviceProvider.GetService(typeof<IUriContext>)

        if isNull service then
            Exception("There is no service object of type IUriContext!")
            |> raise

        let baseUri = (service :?> IUriContext).BaseUri
        SimpleTheme(baseUri)

    interface IStyle with
        member this.Children =
            if not this.isLoading && not <| isNull this.Loaded then
                this.Loaded.Children
            else
                Array.empty

        member this.TryAttach(target, host) =
            if not this.isLoading
               && not <| isNull this.Loaded
               && not <| isNull host then
                this.Loaded.TryAttach(target, host)
            else
                SelectorMatchResult()


    interface IResourceProvider with

        member this.HasResources =
            if not this.isLoading && not <| isNull this.Loaded then
                (this.Loaded :?> IResourceProvider).HasResources
            else
                false

        member this.Owner =
            if not this.isLoading && not <| isNull this.Loaded then
                (this.Loaded :?> IResourceProvider).Owner
            else
                null


        [<CLIEvent>]
        override this.OwnerChanged =
            let e = Event<EventHandler, EventArgs>()

            e.Publish.AddHandler(
                EventHandler
                    (fun owner args ->
                        (this.Loaded :?> IResourceProvider)
                            .AddOwner(owner :?> IResourceHost))
            )

            e.Publish.RemoveHandler(
                EventHandler
                    (fun owner args ->
                        (this.Loaded :?> IResourceProvider)
                            .RemoveOwner(owner :?> IResourceHost))
            )

            e.Publish

        member this.AddOwner(owner) =
            if not this.isLoading && not <| isNull this.Loaded then
                (this.Loaded :?> IResourceProvider)
                    .AddOwner(owner)

        member this.RemoveOwner(owner) =
            if not this.isLoading && not <| isNull this.Loaded then
                (this.Loaded :?> IResourceProvider)
                    .RemoveOwner(owner)

        member this.TryGetResource(key, value: byref<obj>) =
            if not this.isLoading && not <| isNull this.Loaded then
                (this.Loaded :?> IResourceProvider)
                    .TryGetResource(key, ref value)
            else
                false

    static member val ModeProperty: StyledProperty<SimpleThemeMode> = AvaloniaProperty.Register<SimpleTheme, SimpleThemeMode>("Mode")

    member this.Mode
        with get () = this.GetValue(SimpleTheme.ModeProperty)
        and set value = this.SetValue(SimpleTheme.ModeProperty, value) |> ignore

    member val private _loaded: IStyle = null with get, set

    member private this.sharedStyles: Styles =
        styles [
            "avares://Avalonia.Themes.Default/DefaultTheme.xaml"
        ]


    member private this.simpleDark: Styles =
        styles [
            "avares://Avalonia.Themes.Default/Accents/BaseDark.xaml"
        ]

    member private this.simpleLight: Styles =
        styles [
            "avares://Avalonia.Themes.Default/Accents/BaseLight.xaml"
        ]

    member val private isLoading: bool = true with get, set

    member this.Loaded =
        if not <| isNull this._loaded then
            this._loaded
        else
            this.isLoading <- true

            this._loaded <-
                match this.Mode with
                | Light -> this.simpleLight
                | Dark -> this.simpleDark

            this.isLoading <- false
            this._loaded
