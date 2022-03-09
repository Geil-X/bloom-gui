namespace Gui.DataTypes

type ClampedPercentage = ClampedPercentage of float

module ClampedPercentage =

    // ---- Builders ----


    let decimal (n: float) : ClampedPercentage =
        n |> max 0. |> min 1. |> ClampedPercentage

    let percent (n: float) : ClampedPercentage = n / 100. |> decimal

    let zero = percent 0.
    let oneHundred = percent 100.


    // ---- Modifiers ----

    /// Get the percentage out of 1
    let inDecimal (ClampedPercentage p: ClampedPercentage) = p

    /// Get the percentage out of 100
    let inPercentage (ClampedPercentage p: ClampedPercentage) = p * 100.
