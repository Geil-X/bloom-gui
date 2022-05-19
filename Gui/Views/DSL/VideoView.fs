namespace Avalonia.FuncUI.DSL

[<AutoOpen>]
module VideoView =
    open System
    open LibVLCSharp.Avalonia
    open LibVLCSharp.Shared
    open Avalonia.FuncUI.Builder
    open Avalonia.FuncUI.Types

    type VideoView with
        static member create(attrs: IAttr<VideoView> list) : IView<VideoView> = ViewBuilder.Create<VideoView>(attrs)

        static member video<'t when 't :> VideoView>() : IAttr<'t> =
            Core.Initialize()
            let vlc = new LibVLC(enableDebugLogs = true)
            let mediaPlayer = new MediaPlayer(vlc)

            let media =
                new Media(vlc, Uri("http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4"))

            mediaPlayer.Play(media) |> ignore

            AttrBuilder<'t>
                .CreateProperty<MediaPlayer>(VideoView.MediaPlayerProperty, mediaPlayer, ValueNone)
