module Gui.DataTypes.Theme

open Avalonia.Controls
open Avalonia.Media

open Extensions

let program = "Bloom"
let title = "Bloom"

let icon () : WindowIcon =
    Directory.currentApplication ./ "icon.ico"
    |> WindowIcon

/// Color palette is based off of the yellow color with a 150° tetradic color scheme
/// https://paletton.com/#uid=70I2m0krBw0hcHpmp-MvIrjzmlr
let colors =
    {| darkGray = "#202020"
       gray = "#303030"
       lightGray = "#393939"
       lighterGray = "#828282"
       lightOffWhite = "#F0F0F0"
       offWhite = "#E6E6E6"
       yellow = "#FFA123"
       blue = "#2375B3"
       green = "#18AF6E"
       red = "#FF6223" |}

let palette =

    // Color Operations
    let darkest = Color.darken 0.2
    let darken = Color.darken 0.1
    let lighten = Color.lighten 0.1
    let lightest = Color.lighten 0.2
    let fade = Color.fadeOut 0.5

    // Base Colors
    let foreground = Color.hex colors.offWhite
    let primary = Color.hex colors.yellow
    let secondary = Color.hex colors.green
    let tertiary = Color.hex colors.blue

    {| foreground = colors.offWhite
       foregroundFaded = fade foreground |> string
       primaryDarkest = darkest primary |> string
       primaryDark = darken primary |> string
       primary = primary |> string
       primaryLight = lighten primary |> string
       primaryLightest = lightest primary |> string
       secondaryDarkest = darkest secondary |> string
       secondaryDark = darken secondary |> string
       secondary = secondary |> string
       secondaryLight = lighten secondary |> string
       secondaryLightest = lightest secondary |> string
       tertiaryDarkest = darkest tertiary |> string
       tertiaryDark = darken tertiary |> string
       tertiary = tertiary |> string
       tertiaryLight = lighten tertiary |> string
       tertiaryLightest = lightest tertiary |> string
       success = colors.green
       warning = colors.yellow
       danger = colors.red
       info = colors.blue
       panelBackground = colors.darkGray
       panelTitle = colors.gray
       panelAccent = colors.lightGray
       canvasBackground = colors.gray
       shadowColor = colors.lightGray |}

let lighter = Color.lighten 0.075
let lightest = Color.lighten 0.15
let darker = Color.darken 0.075
let darkest = Color.darken 0.15
let fade = Color.fadeOut 0.15

let window =
    {| height = 800.; width = 1200. |}


let font =
    {| h1 = 16.; h2 = 14.; normal = 12. |}

let border =
    {| thickness = 1.; cornerRadius = 5. |}

let spacing =
    {| small = 4.
       medium = 8.
       large = 16. |}

let size =
    {| small = 150.
       medium = 250.
       large = 400. |}

let drawing =
    {| strokeWidth = 1.
       dashArray = [ 3.; 3. ] |}
