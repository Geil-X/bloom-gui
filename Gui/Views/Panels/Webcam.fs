module Gui.Views.Panels.Webcam

open System
open Avalonia.Controls
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open LibVLCSharp.Shared
open LibVLCSharp.Avalonia

open Gui.Views.Components

let libVlc =
    Core.Initialize()

    new LibVLC()

let mediaPlayer () = MediaPlayer.fromVlc libVlc


let media () : Media =
    let exampleVideo =
        "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4"
        |> Uri

    Media.fromUri libVlc exampleVideo

let view =
    Component.create (
        "Component",
        fun ctx ->
            // States
            let mediaPlayer =
                ctx.useState (mediaPlayer ())

            // Views
            let button =
                Button.create [
                    Button.content "Play"
                    Button.onClick (fun _ -> mediaPlayer.Current.Play(media ()) |> ignore)
                ]

            // DSL View
            DockPanel.create [
                DockPanel.children [
                    DockPanel.child Dock.Top button
                    VideoView.create [
                        VideoView.mediaPlayer mediaPlayer.Current
                    ]
                ]
            ]
    )
