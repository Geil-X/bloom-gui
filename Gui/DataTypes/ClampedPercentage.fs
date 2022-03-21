namespace Gui.DataTypes

open System
open Extensions

type ClampedPercentage = ClampedPercentage of float

module ClampedPercentage =

    // ---- Constants ----

    [<Literal>]
    let minimum = 0.

    [<Literal>]
    let maxPercentage = 100.

    [<Literal>]
    let maxDecimal = 1.

    // ---- Builders ----


    let decimal (n: float) : ClampedPercentage =
        n
        |> max minimum
        |> min maxDecimal
        |> ClampedPercentage

    let percent (n: float) : ClampedPercentage = n / maxPercentage |> decimal

    let zero = percent minimum
    let oneHundred = percent maxPercentage


    // ---- Accessors ----

    /// Get the percentage out of 1
    let inDecimal (ClampedPercentage p: ClampedPercentage) = p

    /// Get the percentage out of 100
    let inPercentage (ClampedPercentage p: ClampedPercentage) = p * maxPercentage

    /// Get the percentage as a range from 0 to 255. This spreads out a
    /// percentage over the whole range of an 8 bit unsigned number.
    let toByte (ClampedPercentage p: ClampedPercentage) = p * float Byte.MaxValue |> byte

    /// Get the percentage as a range from 0 to 65536. This spreads out a
    /// percentage over the whole range of an 16 bit unsigned number.
    let toBytes16 (ClampedPercentage p: ClampedPercentage): Byte array =
        p * float UInt16.MaxValue
        |> uint16
        |> UInt16.inBytes
