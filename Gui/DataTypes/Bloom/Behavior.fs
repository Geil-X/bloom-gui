namespace Gui.DataTypes.Bloom

open Gui.DataTypes


/// The flowers have different behaviors that they can take on. The flowers
/// movement is based on their current behavior. If the flower is in the
/// UserControlled mode, then the flower's behavior is based on the commands
/// that the user sends to the flowers individually.
type Behavior =
    | UserControlled
    | Bloom
    | OpenClose

type BehaviorCommand = Flower -> Command list

module Behavior =
    open Math.Units

    let userControlled (_: Flower) : Command list = []

    let bloom (flower: Flower) : Command list =
        if flower.OpenPercent = Percent.zero then
            [ Command.OpenTo Percent.oneHundred ]
        else
            []

    let openClose (flower: Flower) : Command list =
        if flower.OpenPercent = Percent.zero then
            [ Command.OpenTo Percent.oneHundred ]

        else if flower.OpenPercent = Percent.oneHundred then
            [ Command.OpenTo Percent.zero ]

        else
            []


    let getBehavior (behavior: Behavior) : BehaviorCommand =
        match behavior with
        | UserControlled -> userControlled
        | Bloom -> bloom
        | OpenClose -> openClose
