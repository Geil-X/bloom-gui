namespace Math.Units



module Percent =

    open System
    open Extensions
    open Math.Units

    // ---- Constants ----

    [<Literal>]
    let minimum = 0.

    [<Literal>]
    let maxPercentage = 100.

    [<Literal>]
    let maxDecimal = 1.

    // ---- Builders ----


    /// Create a clamped percentage in the range 0 to 1
    let decimal (n: float) : Percent =
        n |> max minimum |> min maxDecimal |> Percent

    /// Create a clamped percentage in the range 0 to 100
    let percent (n: float) : Percent = n / maxPercentage |> decimal

    let zero = percent minimum
    let oneHundred = percent maxPercentage


    // ---- Accessors ----

    /// Get the percentage out of 1
    let inDecimal (p: Percent) : float = p.Value

    /// Get the percentage out of 100
    let inPercentage (p: Percent) : float = p.Value * maxPercentage

    /// Get the percentage as a range from 0 to 255. This spreads out a
    /// percentage over the whole range of an 8 bit unsigned number.
    let toByte (p: Percent) : byte =
        Percent.unwrap p * float Byte.MaxValue |> byte

    /// Get the percentage as a range from 0 to 65536. This spreads out a
    /// percentage over the whole range of an 16 bit unsigned number.
    let toUint16 (p: Percent) : uint16 =
        Percent.unwrap p * float UInt16.MaxValue |> uint16

    /// Get the percentage as a range from 0 to 65536. This spreads out a
    /// percentage over the whole range of an 16 bit unsigned number.
    let toBytes16 (p: Percent) : Byte [] = toUint16 p |> UInt16.inBytes

    let fromBytes (first: byte) (second: byte) : Percent =
        float (UInt16.fromBytes first second)
        / float UInt16.MaxValue
        |> decimal