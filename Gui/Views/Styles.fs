namespace Gui.Views.Styles

open System
open Avalonia
open Avalonia.Controls
open Avalonia.Markup.Xaml
open Avalonia.Markup.Xaml.Styling
open Avalonia.Styling

open AvaloniaStyles

type Theme(baseUri: Uri) =
    inherit SimpleTheme(baseUri)
    
    let theme =
        let button =
            Button()
            
        button.Height <- 40
        
        let style =
            Style(fun (x: Selector) ->
              x.OfType<Button>() |> ignore
              x
              )
            
        style.TryAttach(button, button) |> ignore
        
        style
        
    