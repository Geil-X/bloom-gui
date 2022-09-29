namespace Gui.DataTypes

type I2cAddress = byte

module I2cAddress =

    /// The first 16 Addresses are reserved so the starting address must be the
    /// 17th address.
    let first: I2cAddress = 17uy

    let next (i2c: I2cAddress) : I2cAddress = i2c + 1uy
