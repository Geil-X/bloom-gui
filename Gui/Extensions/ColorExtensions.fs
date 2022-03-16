module Extensions.Color

open Avalonia.Media

/// HSLA Color space with the values ranging from 0 to 1
type Hsla =
    { Hue: float
      Saturation: float
      Lightness: float
      Alpha: float }

// ---- Private Helpers ----

let limit (n: float) = n |> min 1. |> max 0.
let private byteToFloat (b: byte) = float b / 255.
let private floatToByte (f: float) = f * 255. |> byte

// ---- Builders ----

let hex (hex: string) : Color = Color.Parse(hex)

let rgba255 (r: byte) (g: byte) (b: byte) (a: byte) = Color(a, r, g, b)

let rgb255 (r: byte) (g: byte) (b: byte) = Color(r, g, b, 255uy)

let rgba (r: float) (g: float) (b: float) (a: float) : Color =
    rgba255 (floatToByte r) (floatToByte g) (floatToByte b) (floatToByte a)

let rgb (r: float) (g: float) (b: float) = rgba r g b 1.


// https://github.com/avh4/elm-color/blob/1.0.0/src/Color.elm#L197
let hsla (h: float) (s: float) (l: float) (a: float) : Color =
    let m2 =
        if l <= 0.5 then
            l * (s + 1.)

        else
            l + s - l * s

    let m1 = l * 2. - m2

    let hueToRgb (h__: float) =
        let h_ =
            if h__ < 0. then
                h__ + 1.

            else if h__ > 1. then
                h__ - 1.

            else
                h__

        if h_ * 6. < 1. then
            m1 + (m2 - m1) * h_ * 6.

        else

        if h_ * 2. < 1. then
            m2

        else if h_ * 3. < 2. then
            m1 + (m2 - m1) * (2. / 3. - h_) * 6.

        else
            m1

    let r = hueToRgb (h + 1. / 3.)
    let g = hueToRgb h
    let b = hueToRgb (h - 1. / 3.)

    rgba r g b a

let withHsla (color: Hsla) : Color =
    hsla color.Hue color.Saturation color.Lightness color.Alpha

// ---- Accessors ----

let toHsla (color: Color) : Hsla =
    let r = byteToFloat color.R
    let g = byteToFloat color.G
    let b = byteToFloat color.B
    let a = byteToFloat color.A
    let minColor = min r (min g b)
    let maxColor = max r (max g b)

    let h1 =
        if maxColor = r then
            (g - b) / (maxColor - minColor)

        else if maxColor = g then
            2. + (b - r) / (maxColor - minColor)

        else
            4. + (r - g) / (maxColor - minColor)

    let h2 = h1 * (1. / 6.)

    let h3 =
        if h2 < 0. then
            h2 + 1.

        else
            h2

    let l = (minColor + maxColor) / 2.

    let s =
        if minColor = maxColor then
            0.

        else if l < 0.5 then
            (maxColor - minColor) / (maxColor + minColor)

        else
            (maxColor - minColor) / (2. - maxColor - minColor)

    { Hue = h3
      Saturation = s
      Lightness = l
      Alpha = a }

// ---- Modifiers ----

let lighten (n: float) (color: Color) : Color =
    let ofHsla = toHsla color
    hsla ofHsla.Hue ofHsla.Saturation (limit (ofHsla.Lightness + n)) ofHsla.Alpha

let darken (n: float) : Color -> Color = lighten -n

let saturate (n: float) (color: Color) : Color =
    let ofHsla = toHsla color
    hsla ofHsla.Hue (limit (ofHsla.Saturation + n)) ofHsla.Lightness ofHsla.Alpha

let desaturate (n: float) : Color -> Color = saturate -n

let fadeIn (n: float) (color: Color) : Color =
    let ofHsla = toHsla color
    hsla ofHsla.Hue ofHsla.Saturation ofHsla.Lightness (limit (ofHsla.Alpha + n))

let fadeOut (n: float) : Color -> Color = fadeIn -n
