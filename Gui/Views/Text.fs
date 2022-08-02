module Gui.Views.Text

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open Avalonia.Layout
open Gui.DataTypes

let iconTitle (icon: IView) (name: string) (color: string) =
    let titleBlock =
        TextBlock.create [
            TextBlock.classes [ "h1" ]
            TextBlock.fontSize Theme.font.h1
            TextBlock.foreground color
            TextBlock.verticalAlignment VerticalAlignment.Center
            TextBlock.text name
            TextBlock.margin (Theme.spacing.medium, 0., 0., 0.)
        ]

    StackPanel.create [
        StackPanel.verticalAlignment VerticalAlignment.Center
        StackPanel.horizontalAlignment HorizontalAlignment.Center
        StackPanel.orientation Orientation.Horizontal
        StackPanel.children [ icon; titleBlock ]
        StackPanel.margin (0., Theme.spacing.large)
    ]
