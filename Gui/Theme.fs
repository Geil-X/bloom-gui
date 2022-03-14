module Gui.Theme

open Avalonia.Controls
open Extensions

let program = "Bloom"
let title = "Bloom: Flower Simulation"

let icon: WindowIcon =
    Bitmap.create "avares://Gui/Assets/Geil Logo 32.ico"
    |> WindowIcon

/// Color palette is based off of the yellow color with a 150° tetradic color scheme
/// https://paletton.com/#uid=70I2m0krBw0hcHpmp-MvIrjzmlr
let colors =
    {| darkGray = "#202020"
       gray = "#303030"
       lightGray = "#393939"
       lighterGray = "#828282"
       lightOffWhite = "#f0f0f0"
       offWhite = "#e6e6e6"
       darkerYellow = "#AB6200"
       darkYellow = "#DA7E02"
       yellow = "#FFA123"
       lightYellow = "#E4AE67"
       lighterYellow = "#FFC476"
       blue = "#206BA4"
       lightBlue = "#3D7FB2"
       green = "#18AF6E" |}

let palette =
    let darkest = Color.darken 0.2
    let darken = Color.darken 0.1
    let lighten = Color.lighten 0.1
    let lightest = Color.lighten 0.2

    let primary = Color.hex colors.yellow
    let secondary = Color.hex colors.green
    let tertiary = Color.hex colors.blue


    {| foreground = colors.offWhite
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

let window = {| height = 600.; width = 800. |}


let font = {| h1 = 16.; h2 = 14.; normal = 12. |}

let border = {| thickness = 1.; cornerRadius = 5. |}

let spacing =
    {| small = 4.
       medium = 8.
       large = 16. |}

let size = {| small = 150. |}

let drawing =
    {| strokeWidth = 1.
       dashArray = [ 3.; 3. ] |}
