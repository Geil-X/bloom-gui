module Gui.Widgets.Form

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open Avalonia.Interactivity
open Avalonia.Layout
open Avalonia.Media

open Gui
open Utilities.Extensions

let formElement
    (state: {| Name: string
               Orientation: Orientation
               Element: IView |})
    : IView<StackPanel> =
    StackPanel.create
    <| [ StackPanel.orientation state.Orientation
         StackPanel.margin Theme.spacing.medium
         StackPanel.spacing Theme.spacing.small
         StackPanel.children
         <| [ TextBlock.create [ TextBlock.text state.Name ]
              state.Element ] ]

let textItem
    (state: {| Name: string
               Value: string
               OnChange: string -> unit
               LabelPlacement: Orientation |})
    : IView<StackPanel> =

    formElement
        {| Name = state.Name
           Orientation = state.LabelPlacement
           Element =
               TextBox.create [
                   TextBox.text state.Value
                   TextBox.onTextChanged state.OnChange
               ] |}

let multiline
    (state: {| Name: string
               Value: string
               OnSelected: string -> unit |})
    : IView<StackPanel> =

    formElement
        {| Name = state.Name
           Orientation = Orientation.Vertical
           Element =
               TextBox.create
               <| [ TextBox.acceptsReturn true
                    TextBox.textWrapping TextWrapping.Wrap
                    TextBox.text state.Value
                    TextBox.onTextChanged state.OnSelected ] |}

let dropdownSelection
    (state: {| Name: string
               Selected: 'a
               OnSelected: 'a -> unit |})
    : IView<StackPanel> =

    formElement
        {| Name = state.Name
           Orientation = Orientation.Vertical
           Element =
               ComboBox.create
               <| [ ComboBox.dataItems (Seq.map DiscriminatedUnion.toString DiscriminatedUnion.allCases<'a>)
                    ComboBox.selectedItem (DiscriminatedUnion.toString state.Selected)
                    ComboBox.onSelectedItemChanged (tryUnbox >> Option.iter state.OnSelected) ] |}


let imageButton (icon: IView) (color: string) (onClick: RoutedEventArgs -> unit) : IView<Button> =
    Button.create
    <| [ Button.padding Theme.spacing.small
         Button.margin Theme.spacing.small
         Button.foreground color
         Button.onClick onClick
         Button.content icon ]

let iconTextButton (icon: IView) (text: string) (color: string) (onClick: RoutedEventArgs -> unit) : IView<Button> =
    Button.create
    <| [ Button.margin Theme.spacing.small
         Button.onClick onClick
         Button.content (
             StackPanel.create [
                 StackPanel.orientation Orientation.Horizontal
                 StackPanel.spacing Theme.spacing.small
                 StackPanel.children [
                     icon
                     TextBlock.create [
                         TextBlock.fontSize Theme.font.h2
                         TextBlock.foreground color
                         TextBlock.text text
                         TextBlock.verticalAlignment VerticalAlignment.Center
                         TextBlock.horizontalAlignment HorizontalAlignment.Center
                     ]
                 ]
             ]
         ) ]
