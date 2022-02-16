module FlowerGui.Widgets.Form

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open Avalonia.Interactivity

open FlowerGui

let imageButton (icon: IView<'a>) (onClick: RoutedEventArgs -> unit) : IView<Button> =
    Button.create
    <| [ Button.padding Theme.spacing.small
         Button.margin Theme.spacing.small
         Button.onClick onClick
         Button.content icon ]
