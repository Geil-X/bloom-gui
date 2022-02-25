module Gui.Theme

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
    {|
       primaryDarkest = colors.darkerYellow
       primaryDark = colors.darkYellow
       primary = colors.yellow
       primaryLight = colors.lightYellow
       primaryLightest = colors.lighterYellow
       panelBackground = colors.darkGray
       panelAccent = colors.lightGray
       canvasBackground = colors.gray
       shadowColor = colors.lightGray |}

let window = {| height = 600.; width = 800. |}


let font = {| h1 = 16.; h2 = 14.; normal = 12. |}

let border = {| thickness = 1.; cornerRadius = 5. |}

let creasePattern = {| maxLength = 500. |}

let spacing =
    {| small = 4.
       medium = 8.
       large = 16. |}

let size = {| small = 150. |}

let drawing =
    {| strokeWidth = 1.
       dashArray = [ 3.; 3. ] |}