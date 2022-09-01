module Gui.Views.Panels.Webcam

open System
open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.Layout
open Elmish
open Gui
open LibVLCSharp.Shared
open LibVLCSharp.Avalonia.FuncUI

open Gui.Views.Components
open Gui.DataTypes

type WebcamAddress = string

type State =
    { VideoState: VideoState
      Media: Media
      MediaPlayer: MediaPlayer
      WebcamDevices: WebcamAddress list
      SelectedWebcam: string option }

type Msg =
    | ChangeVideoState of VideoState
    | RefreshWebcamList
    | GotNewWebcamAddresses of WebcamAddress list
    | SelectWebcamIndex of int
    | DeselectWebcamAddress


// ---- Initialization ---------------------------------------------------------

let refreshWebcamCmd =
    Cmd.OfFunc.perform (fun () -> []) () GotNewWebcamAddresses

let libVlc = new LibVLC()

let init () : State * Cmd<Msg> =
    let media: Media =
        let exampleVideo =
            "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4"
            |> Uri

        Media.fromUri libVlc exampleVideo

    { VideoState = VideoState.Stop
      Media = media
      MediaPlayer = MediaPlayer.fromVlc libVlc
      WebcamDevices = []
      SelectedWebcam = None },
    refreshWebcamCmd

// ---- Update -----------------------------------------------------------------

let update (state: State) (msg: Msg) : State * Cmd<Msg> =
    match msg with
    | ChangeVideoState videoState -> { state with VideoState = videoState }, Cmd.none

    // TODO: This code is crashing and it needs investigation
    | RefreshWebcamList -> state, refreshWebcamCmd

    | GotNewWebcamAddresses addresses ->
        Log.debug $"Refreshed USB Webcam list and got {addresses.Length} items"
        { state with WebcamDevices = addresses }, Cmd.none

    // Need to init webcam
    | SelectWebcamIndex cameraIndex ->
        if cameraIndex < 0 then
            state, Cmd.none

        else
            { state with SelectedWebcam = None }, Cmd.none

    | DeselectWebcamAddress -> { state with SelectedWebcam = None }, Cmd.none

// ---- View -------------------------------------------------------------------

let private webcamDropdown (webcamAddresses: WebcamAddress list) (maybeWebcamAddress: string option) dispatch =
    let webcamIcon =
        Icon.connection Icon.small Theme.palette.info
        |> View.withAttr (Viewbox.dock Dock.Left)

    let dropdown =
        ComboBox.create [
            ComboBox.margin (Theme.spacing.small, 0.)
            ComboBox.dataItems webcamAddresses
            ComboBox.dock Dock.Left
            ComboBox.onPointerEnter (fun _ -> dispatch RefreshWebcamList)
            ComboBox.onSelectedIndexChanged (SelectWebcamIndex >> dispatch)

            if Option.isSome maybeWebcamAddress then
                ComboBox.selectedItem (Option.get maybeWebcamAddress)
        ]

    Form.formElement
        {| Name = "Webcam Video Input"
           Orientation = Orientation.Vertical
           Element =
            DockPanel.create [
                DockPanel.children [ webcamIcon; dropdown ]
            ] |}

let sidePanel (state: State) (dispatch: Msg -> unit) =
    let title =
        Text.iconTitle (Icon.flower Icon.large Theme.palette.primary) "Webcam" Theme.palette.foreground

    let button =
        Button.create [
            Button.content "Play"
            Button.onClick (fun _ -> ChangeVideoState VideoState.Play |> dispatch)
        ]

    StackPanel.create [
        StackPanel.minWidth Theme.size.medium
        StackPanel.children [
            title
            button
            webcamDropdown state.WebcamDevices state.SelectedWebcam dispatch
        ]
    ]

let view (state: State) (dispatch: Msg -> unit) =

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
            DockPanel.child Dock.Left (sidePanel state dispatch)
            webcam
        ]
    ]
