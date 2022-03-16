// Icons can be found here https://materialdesignicons.com/
module Gui.Icons

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open Avalonia.Controls.Shapes


// ---- Builders ----

let private icon (data: string) (color: string) : IView =
    Canvas.create [
        Canvas.width 24.0
        Canvas.height 24.0
        Canvas.children [
            Path.create [ Path.fill color; Path.data data ]
        ]
    ]
    :> IView


// ---- Actions ----

let newFile =
    icon
        "M13,9H18.5L13,3.5V9M6,2H14L20,8V20A2,2 0 0,1 18,22H6C4.89,22 4,21.1 4,20V4C4,2.89 4.89,2 6,2M11,15V12H9V15H6V17H9V20H11V17H14V15H11Z"

let save =
    icon
        "M15,9H5V5H15M12,19A3,3 0 0,1 9,16A3,3 0 0,1 12,13A3,3 0 0,1 15,16A3,3 0 0,1 12,19M17,3H5C3.89,3 3,3.9 3,5V19A2,2 0 0,0 5,21H19A2,2 0 0,0 21,19V7L17,3Z"

let load =
    icon
        "M19,20H4C2.89,20 2,19.1 2,18V6C2,4.89 2.89,4 4,4H10L12,6H19A2,2 0 0,1 21,8H21L4,8V18L6.14,10H23.21L20.93,18.5C20.7,19.37 19.92,20 19,20Z"


let undo =
    icon
        "M12.5,8C9.85,8 7.45,9 5.6,10.6L2,7V16H11L7.38,12.38C8.77,11.22 10.54,10.5 12.5,10.5C16.04,10.5 19.05,12.81 20.1,16L22.47,15.22C21.08,11.03 17.15,8 12.5,8Z"

let redo =
    icon
        "M18.4,10.6C16.55,9 14.15,8 11.5,8C6.85,8 2.92,11.03 1.54,15.22L3.9,16C4.95,12.81 7.95,10.5 11.5,10.5C13.45,10.5 15.23,11.22 16.62,12.38L13,16H22V7L18.4,10.6Z"


// ---- Iconography ----

let flower =
    icon
        "M3,13A9,9 0 0,0 12,22C12,17 7.97,13 3,13M12,5.5A2.5,2.5 0 0,1 14.5,8A2.5,2.5 0 0,1 12,10.5A2.5,2.5 0 0,1 9.5,8A2.5,2.5 0 0,1 12,5.5M5.6,10.25A2.5,2.5 0 0,0 8.1,12.75C8.63,12.75 9.12,12.58 9.5,12.31C9.5,12.37 9.5,12.43 9.5,12.5A2.5,2.5 0 0,0 12,15A2.5,2.5 0 0,0 14.5,12.5C14.5,12.43 14.5,12.37 14.5,12.31C14.88,12.58 15.37,12.75 15.9,12.75C17.28,12.75 18.4,11.63 18.4,10.25C18.4,9.25 17.81,8.4 16.97,8C17.81,7.6 18.4,6.74 18.4,5.75C18.4,4.37 17.28,3.25 15.9,3.25C15.37,3.25 14.88,3.41 14.5,3.69C14.5,3.63 14.5,3.56 14.5,3.5A2.5,2.5 0 0,0 12,1A2.5,2.5 0 0,0 9.5,3.5C9.5,3.56 9.5,3.63 9.5,3.69C9.12,3.41 8.63,3.25 8.1,3.25A2.5,2.5 0 0,0 5.6,5.75C5.6,6.74 6.19,7.6 7.03,8C6.19,8.4 5.6,9.25 5.6,10.25M12,22A9,9 0 0,0 21,13C16,13 12,17 12,22Z"

let newFlower =
    icon
        "M 12 1 A 2.5 2.5 0 0 0 9.5 3.5 L 9.5 3.6894531 C 9.12 3.4094531 8.6296094 3.25 8.0996094 3.25 A 2.5 2.5 0 0 0 5.5996094 5.75 C 5.5996094 6.74 6.1892969 7.6 7.0292969 8 C 6.1892969 8.4 5.5996094 9.25 5.5996094 10.25 A 2.5 2.5 0 0 0 8.0996094 12.75 C 8.6296094 12.75 9.12 12.580547 9.5 12.310547 L 9.5 12.5 A 2.5 2.5 0 0 0 12 15 A 2.5 2.5 0 0 0 12.914062 14.804688 A 5.9858322 5.9858322 0 0 1 14.384766 13.216797 A 2.5 2.5 0 0 0 14.5 12.5 L 14.5 12.310547 C 14.720984 12.467562 14.980101 12.588451 15.261719 12.664062 A 5.9858322 5.9858322 0 0 1 17.666016 12.017578 A 5.9858322 5.9858322 0 0 1 17.667969 12.017578 C 18.120469 11.565078 18.400391 10.94 18.400391 10.25 C 18.400391 9.25 17.810703 8.4 16.970703 8 C 17.810703 7.6 18.400391 6.74 18.400391 5.75 C 18.400391 4.37 17.280391 3.25 15.900391 3.25 C 15.370391 3.25 14.88 3.4094531 14.5 3.6894531 L 14.5 3.5 A 2.5 2.5 0 0 0 12 1 z M 12 5.5 A 2.5 2.5 0 0 1 14.5 8 A 2.5 2.5 0 0 1 12 10.5 A 2.5 2.5 0 0 1 9.5 8 A 2.5 2.5 0 0 1 12 5.5 z M 3 13 A 9 9 0 0 0 12 22 C 12 17 7.97 13 3 13 z M 12 22 A 9 9 0 0 0 13.4375 21.875 A 5.9858322 5.9858322 0 0 1 12.275391 19.767578 C 12.095938 20.480451 12 21.228487 12 22 z M 17 14 L 17 17 L 14 17 L 14 19 L 17 19 L 17 22 L 19 22 L 19 19 L 22 19 L 22 17 L 19 17 L 19 14 L 17 14 z "

let command =
    icon
        "M17,12L12,17V14H8V10H12V7L17,12M21,16.5C21,16.88 20.79,17.21 20.47,17.38L12.57,21.82C12.41,21.94 12.21,22 12,22C11.79,22 11.59,21.94 11.43,21.82L3.53,17.38C3.21,17.21 3,16.88 3,16.5V7.5C3,7.12 3.21,6.79 3.53,6.62L11.43,2.18C11.59,2.06 11.79,2 12,2C12.21,2 12.41,2.06 12.57,2.18L20.47,6.62C20.79,6.79 21,7.12 21,7.5V16.5M12,4.15L5,8.09V15.91L12,19.85L19,15.91V8.09L12,4.15Z"

let home =
    icon "M10,20V14H14V20H19V12H22L12,3L2,12H5V20H10Z"

let openIcon =
    icon
        "M6 11V13H4V11H6M22 5H17V19H22V5M7 5H2L2 19H7V5M22 3C23.11 3 24 3.89 24 5V21H0V5C0 3.89 .894 3 2 3H9V19H15V3H22M20 11H18V13H20V11Z"

let close =
    icon
        "M10 13H8V11H10V13M16 11H14V13H16V11M21 19V21H3V19H4V5C4 3.9 4.9 3 6 3H18C19.1 3 20 3.9 20 5V19H21M11 5H6V19H11V5M18 5H13V19H18V5Z"

let openTo =
    icon "M16,11H18V13H16V11M12,3H19C20.11,3 21,3.89 21,5V19H22V21H2V19H10V5C10,3.89 10.89,3 12,3M12,5V19H19V5H12Z"

let speed =
    icon
        "M12,16A3,3 0 0,1 9,13C9,11.88 9.61,10.9 10.5,10.39L20.21,4.77L14.68,14.35C14.18,15.33 13.17,16 12,16M12,3C13.81,3 15.5,3.5 16.97,4.32L14.87,5.53C14,5.19 13,5 12,5A8,8 0 0,0 4,13C4,15.21 4.89,17.21 6.34,18.65H6.35C6.74,19.04 6.74,19.67 6.35,20.06C5.96,20.45 5.32,20.45 4.93,20.07V20.07C3.12,18.26 2,15.76 2,13A10,10 0 0,1 12,3M22,13C22,15.76 20.88,18.26 19.07,20.07V20.07C18.68,20.45 18.05,20.45 17.66,20.06C17.27,19.67 17.27,19.04 17.66,18.65V18.65C19.11,17.2 20,15.21 20,13C20,12 19.81,11 19.46,10.1L20.67,8C21.5,9.5 22,11.18 22,13Z"

let acceleration =
    icon
        "M12.68 6H11.32L7 16H9L9.73 14H14.27L15 16H17L12.68 6M10.3 12.5L12 8L13.7 12.5H10.3M17.4 20.4L19 22H14V17L16 19C18.39 17.61 20 14.95 20 12C20 7.59 16.41 4 12 4S4 7.59 4 12C4 14.95 5.61 17.53 8 18.92V21.16C4.47 19.61 2 16.1 2 12C2 6.5 6.5 2 12 2S22 6.5 22 12C22 15.53 20.17 18.62 17.4 20.4Z"
