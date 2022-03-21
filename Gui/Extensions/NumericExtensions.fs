namespace Extensions

module UInt16 =

    let inBytes (n: uint16) = [| byte n; byte (n >>> 8) |]
