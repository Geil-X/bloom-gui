namespace FlowerGui

open Avalonia.Media

type Flower =
    { Name: string
      I2cAddress: uint
      Color: Color }

module Flower =
    let setName name flower : Flower = { flower with Name = name }
    let setI2cAddress i2CAddress flower : Flower = { flower with I2cAddress = i2CAddress }
    let setColor color flower : Flower = { flower with Color = color }
