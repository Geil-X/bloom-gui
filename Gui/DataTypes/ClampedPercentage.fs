namespace Gui.DataTypes

open System
open Extensions

type ClampedPercentage = ClampedPercentage of float

module ClampedPercentage =

    // ---- Builders ----


    let decimal (n: float) : ClampedPercentage =
        n |> max 0. |> min 1. |> ClampedPercentage

    let percent (n: float) : ClampedPercentage = n / 100. |> decimal

    let zero = percent 0.
    let oneHundred = percent 100.


    // ---- Accessors ----

    /// Get the percentage out of 1
    let inDecimal (ClampedPercentage p: ClampedPercentage) = p

    /// Get the percentage out of 100
    let inPercentage (ClampedPercentage p: ClampedPercentage) = p * 100.

    /// Get the percentage as a range from 0 to 255. This spreads out a
    /// percentage over the whole range of an 8 bit unsigned number.
    let toByte (ClampedPercentage p: ClampedPercentage) = p * float Byte.MaxValue |> byte

    /// Get the percentage as a range from 0 to 65536. This spreads out a
    /// percentage over the whole range of an 16 bit unsigned number.
    let toBytes16 (ClampedPercentage p: ClampedPercentage) =
        p * float UInt16.MaxValue
        |> uint16
        |> UInt16.inBytes
