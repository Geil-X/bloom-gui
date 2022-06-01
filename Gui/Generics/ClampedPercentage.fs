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


    /// Create a clamped percentage in the range 0 to 1
    let decimal (n: float) : ClampedPercentage =
        n
        |> max minimum
        |> min maxDecimal
        |> ClampedPercentage

    /// Create a clamped percentage in the range 0 to 100
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
    let toByte (ClampedPercentage p: ClampedPercentage): byte = p * float Byte.MaxValue |> byte

    /// Get the percentage as a range from 0 to 65536. This spreads out a
    /// percentage over the whole range of an 16 bit unsigned number.
    let toUint16 (ClampedPercentage p: ClampedPercentage): uint16 = p * float UInt16.MaxValue |> uint16

    /// Get the percentage as a range from 0 to 65536. This spreads out a
    /// percentage over the whole range of an 16 bit unsigned number.
    let toBytes16 (p: ClampedPercentage) : Byte [] = toUint16 p |> UInt16.inBytes
    
    let fromBytes (first: byte) (second: byte) : ClampedPercentage =
        float (UInt16.fromBytes [| first; second |]) / float UInt16.MaxValue
        |> decimal