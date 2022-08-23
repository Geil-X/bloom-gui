module Gui.Views.Panels.Webcam

open System
open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.Layout
open LibVLCSharp.Shared
open LibVLCSharp.Avalonia.FuncUI
open Gui.Views.Components


type State =
    { VideoState: VideoState
      Media: Media
      MediaPlayer: MediaPlayer }

type Msg = ChangeVideoState of VideoState


// ---- Initialization ---------------------------------------------------------

let libVlc = new LibVLC()


let init () =
    let media: Media =
        let exampleVideo =
            "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4"
            |> Uri

        Media.fromUri libVlc exampleVideo

    { VideoState = VideoState.Stop
      Media = media
      MediaPlayer = MediaPlayer.fromVlc libVlc }

// ---- Update -----------------------------------------------------------------

let update (state: State) (msg: Msg) : State =
    match msg with
    | ChangeVideoState videoState -> { state with VideoState = videoState }

// ---- View -------------------------------------------------------------------

let view (state: State) (dispatch: Msg -> unit) =

    let button =
        Button.create [
            Button.content "Play"
            Button.onClick (fun _ -> ChangeVideoState VideoState.Play |> dispatch)
        ]

    let webcam =
        VideoView.create [
            VideoView.horizontalAlignment HorizontalAlignment.Stretch
            VideoView.verticalAlignment VerticalAlignment.Stretch
            VideoView.mediaPlayer state.MediaPlayer
            VideoView.media state.Media
            VideoView.state state.VideoState
        ]


    // DSL View
    DockPanel.create [
        DockPanel.children [
            DockPanel.child Dock.Top button
            webcam
        ]
    ]
