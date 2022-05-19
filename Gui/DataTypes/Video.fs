namespace Gui.DataTypes

open System
open LibVLCSharp.Shared

type Video =
    { Vlc: LibVLC
      MediaPlayer: MediaPlayer }

module Video =
    let create =
        let vlc = new LibVLC(enableDebugLogs = true)

        Core.Initialize()

        { Vlc = vlc
          MediaPlayer = new MediaPlayer(vlc) }

    let play (video: Video) =
        if video.Vlc <> null || video.MediaPlayer <> null then
            let media =
                new Media(
                    video.Vlc,
                    Uri("http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4")
                )

            video.MediaPlayer.Play(media) |> ignore

        video

    let stop (video: Video) =
        if video.MediaPlayer <> null then
            video.MediaPlayer.Stop()

        video

    let dispose (video: Video) =
        if video.MediaPlayer <> null then
            video.MediaPlayer.Dispose()

        if video.Vlc <> null then
            video.Vlc.Dispose()

        video
