namespace Gui.DataTypes.Bloom



/// The flowers have different behaviors that they can take on. The flowers
/// movement is based on their current behavior. If the flower is in the
/// UserControlled mode, then the flower's behavior is based on the commands
/// that the user sends to the flowers individually.
type Behavior =
    | UserControlled
    | Bloom
    | OpenClose

module Behavior =
    open Gui.DataTypes
    open Math.Units

    let userControlled (flower: Flower) : Flower = flower

    let bloom (flower: Flower) : Flower =
        if flower.OpenPercent = Percent.zero then
            flower
            |> Flower.setTargetPercent Percent.oneHundred
        else
            flower

    let openClose (flower: Flower) : Flower =
        if flower.OpenPercent = Percent.zero then
            flower
            |> Flower.setTargetPercent Percent.oneHundred

        else if flower.OpenPercent = Percent.oneHundred then
            flower |> Flower.setTargetPercent Percent.zero

        else
            flower


    let getBehavior (behavior: Behavior) : Flower -> Flower =
        match behavior with
        | UserControlled -> userControlled
        | Bloom -> bloom
        | OpenClose -> openClose
