module LibVLCSharp.Shared.MediaPlayer

open LibVLCSharp.Shared

let fromVlc (vlc: LibVLC) : MediaPlayer = new MediaPlayer(vlc)

let create () : MediaPlayer = fromVlc (new LibVLC())

let play (media: Media) (player: MediaPlayer) : MediaPlayer =
    player.Play(media) |> ignore

    player
