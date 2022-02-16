module FlowerGui.Widgets.Form

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open Avalonia.Interactivity

open Avalonia.Layout
open Avalonia.Media
open FlowerGui
open Utilities.Extensions

let formElement
    (state: {| name: string
               orientation: Orientation
               element: IView |})
    : IView<StackPanel> =
    StackPanel.create
    <| [ StackPanel.orientation state.orientation
         StackPanel.margin Theme.spacing.medium
         StackPanel.spacing Theme.spacing.small
         StackPanel.children
         <| [ TextBlock.create [ TextBlock.text state.name ]
              state.element ] ]

let textItem
    (state: {| Name: string
               Value: string
               OnChange: string -> unit
               LabelPlacement: Orientation |})
    : IView<StackPanel> =

    formElement
        {| name = state.Name
           orientation = state.LabelPlacement
           element =
               TextBox.create [
                   TextBox.text state.Value
                   TextBox.onTextChanged state.OnChange
               ] |}

let multiline
    (state: {| name: string
               value: string
               onSelected: string -> unit |})
    : IView<StackPanel> =

    formElement
        {| name = state.name
           orientation = Orientation.Vertical
           element =
               TextBox.create
               <| [ TextBox.acceptsReturn true
                    TextBox.textWrapping TextWrapping.Wrap
                    TextBox.text state.value
                    TextBox.onTextChanged state.onSelected ] |}

let dropdownSelection
    (state: {| name: string
               selected: 'a
               onSelected: 'a -> unit |})
    : IView<StackPanel> =

    formElement
        {| name = state.name
           orientation = Orientation.Vertical
           element =
               ComboBox.create
               <| [ ComboBox.dataItems (Seq.map DiscriminatedUnion.toString DiscriminatedUnion.allCases<'a>)
                    ComboBox.selectedItem (DiscriminatedUnion.toString state.selected)
                    ComboBox.onSelectedItemChanged (tryUnbox >> Option.iter state.onSelected) ] |}


let imageButton (icon: IView<'a>) (onClick: RoutedEventArgs -> unit) : IView<Button> =
    Button.create
    <| [ Button.padding Theme.spacing.small
         Button.margin Theme.spacing.small
         Button.onClick onClick
         Button.content icon ]
