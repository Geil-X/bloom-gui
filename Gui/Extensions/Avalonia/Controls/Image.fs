namespace Avalonia.Controls

open Avalonia.Controls
open Avalonia.Media
open Avalonia.Media.Imaging


module ImageBrush =
    let fromString source = Bitmap.create source |> ImageBrush

[<AutoOpen>]
module Image =
    type Image with
        static member FromString(s: string) : Image =
            let img = Image()
            img.Source <- Bitmap.create s
            img
