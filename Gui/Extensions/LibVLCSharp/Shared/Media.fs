module LibVLCSharp.Shared.Media

open System
open LibVLCSharp.Shared

let fromUri (libVlc: LibVLC) (uri: Uri) = new Media(libVlc, uri)
