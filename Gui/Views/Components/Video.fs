namespace Gui.Views.Components


[<AutoOpen>]
module VideoView =
    open Avalonia.FuncUI.Builder
    open Avalonia.FuncUI.Types
    open LibVLCSharp.Avalonia
    open LibVLCSharp.Shared

    module VideoView =
        let create (attrs: IAttr<VideoView> list) : IView<VideoView> = ViewBuilder.Create<VideoView>(attrs)

    type VideoView with

        static member mediaPlayer<'t when 't :> VideoView>(value: MediaPlayer) : IAttr<'t> =
            AttrBuilder<'t>
                .CreateProperty<MediaPlayer>(VideoView.MediaPlayerProperty, value, ValueNone)

        static member onLoad<'t when 't :> VideoView>(func: MediaPlayer -> unit, ?subPatchOptions) : IAttr<'t> =
            AttrBuilder.CreateSubscription<MediaPlayer>(
                VideoView.MediaPlayerProperty,
                func,
                ?subPatchOptions = subPatchOptions
            )
