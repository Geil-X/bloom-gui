namespace LibVLCSharp.Avalonia.FuncUI

open System
open System.Runtime.InteropServices
open Avalonia
open Avalonia.Controls
open Avalonia.Data
open Avalonia.FuncUI.Builder
open Avalonia.FuncUI.Types
open Avalonia.Platform
open LibVLCSharp.Shared

type VideoState =
    | Play
    | Pause
    | Stop

type VideoView() =
    inherit NativeControlHost()
    with
        // ---- Private Properties ----

        let mutable _mediaPlayer: MediaPlayer = null

        let mutable _platformHandle: IPlatformHandle =
            null

        let mutable _videoState: VideoState = Stop


        // ---- Member Properties ----

        /// <summary>
        ///     Gets or sets the MediaPlayer that will be displayed.
        /// </summary>
        member this.MediaPlayer
            with get (): MediaPlayer = _mediaPlayer
            and set value = _mediaPlayer <- value

        member this.Media
            with get (): Media = _mediaPlayer.Media
            and set value =
                if not <| isNull _mediaPlayer then
                    _mediaPlayer.Media <- value

        member this.VideoState
            with get (): VideoState = _videoState
            and set value =
                match value with
                | Play -> this.Play()
                | Pause -> this.Pause()
                | Stop -> this.Stop()


        // ---- Avalonia Properties ----
        static member MediaProperty: DirectProperty<VideoView, Media> =
            AvaloniaProperty.RegisterDirect<VideoView, Media>
                ( nameof Media
                ,  fun (o: VideoView) -> o.Media
                ,  fun (o: VideoView) (v: Media) ->
                    o.Media <- v
                ,  defaultBindingMode = BindingMode.TwoWay
                )
                
        static member MediaPlayerProperty: DirectProperty<VideoView, MediaPlayer> =
            AvaloniaProperty.RegisterDirect<VideoView, MediaPlayer>
                ( nameof MediaPlayer
                ,  fun (o: VideoView) -> o.MediaPlayer
                ,  fun (o: VideoView) (v: MediaPlayer) ->
                    o.MediaPlayer <- v
                ,  defaultBindingMode = BindingMode.TwoWay
                )
                
        static member VideoStateProperty: DirectProperty<VideoView, VideoState> =
            AvaloniaProperty.RegisterDirect<VideoView, VideoState>
                ( nameof VideoState
                ,  fun (o: VideoView) -> o.VideoState
                ,  fun (o: VideoView) (v: VideoState) ->
                    o.VideoState <- v
                ,  defaultBindingMode = BindingMode.TwoWay
                )


        // ---- Action Functions ----

        member this.Play() =
            if isNull this.MediaPlayer || isNull this.Media then
                ()

            this.Attach()
            this.MediaPlayer.Play this.Media |> ignore
            _videoState <- VideoState.Play

        member this.Pause() =
            if not <| isNull this.Media
               && not <| isNull this.Media
               && this.MediaPlayer.IsPlaying then
                this.MediaPlayer.Pause()
                _videoState <- VideoState.Pause

        member this.Stop() =
            if not <| isNull this.Media
               && not <| isNull this.Media
               && this.MediaPlayer.IsPlaying then
                this.MediaPlayer.Stop()
                _videoState <- VideoState.Stop


        // ---- Core ----

        member this.Attach() =
            if isNull _mediaPlayer
               || isNull _platformHandle
               || not this.IsInitialized then
                ()
            else if RuntimeInformation.IsOSPlatform OSPlatform.Windows then
                _mediaPlayer.Hwnd <- _platformHandle.Handle
            else if RuntimeInformation.IsOSPlatform OSPlatform.Linux then
                _mediaPlayer.XWindow <- uint _platformHandle.Handle
            else if RuntimeInformation.IsOSPlatform OSPlatform.OSX then
                _mediaPlayer.NsObject <- _platformHandle.Handle
            else
                failwith "Unsupported platform for the LibVLC VideoView component"


        member this.Detach() =
            if isNull _mediaPlayer then
                ()

            else if RuntimeInformation.IsOSPlatform OSPlatform.Windows then
                _mediaPlayer.Hwnd <- IntPtr.Zero
            else if RuntimeInformation.IsOSPlatform OSPlatform.Linux then
                _mediaPlayer.XWindow <- 0u
            else if RuntimeInformation.IsOSPlatform OSPlatform.OSX then
                _mediaPlayer.NsObject <- IntPtr.Zero
            else
                failwith "Unsupported platform for the LibVLC VideoView component"


        /// <inheritdoc />
        override this.CreateNativeControlCore(parent: IPlatformHandle) =
            _platformHandle <- ``base``.CreateNativeControlCore(parent)

            if not <| isNull _mediaPlayer then
                this.Attach()

            _platformHandle


        /// <inheritdoc />
        override this.DestroyNativeControlCore(control: IPlatformHandle) =
            this.Detach()
            ``base``.DestroyNativeControlCore(control)
            _platformHandle <- null
            
            
        // ---- FuncUI Bindings ----
        
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
