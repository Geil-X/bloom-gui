namespace LibVLCSharp.Shared


[<AutoOpen>]
module VideoView =
    open Avalonia.FuncUI.Builder
    open Avalonia.FuncUI.Types

    open LibVLCSharp.Avalonia.FuncUI


    type VideoView with
        static member create(attrs: IAttr<VideoView> list) : IView<VideoView> = ViewBuilder.Create<VideoView>(attrs)

        static member mediaPlayer<'t when 't :> VideoView>(value: MediaPlayer) : IAttr<'t> =
            AttrBuilder<'t>
                .CreateProperty<MediaPlayer>(VideoView.MediaPlayerProperty, value, ValueNone)

        static member media<'t when 't :> VideoView>(value: Media) : IAttr<'t> =
            AttrBuilder<'t>
                .CreateProperty<Media>(VideoView.MediaProperty, value, ValueNone)

        static member state<'t when 't :> VideoView>(value: VideoState) : IAttr<'t> =
            AttrBuilder<'t>
                .CreateProperty<VideoState>(VideoView.VideoStateProperty, value, ValueNone)

        static member onLoad<'t when 't :> VideoView>(func: MediaPlayer -> unit, ?subPatchOptions) : IAttr<'t> =
            AttrBuilder.CreateSubscription<MediaPlayer>(
                VideoView.MediaPlayerProperty,
                func,
                ?subPatchOptions = subPatchOptions
            )
