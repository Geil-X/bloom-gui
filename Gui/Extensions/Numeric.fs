namespace Extensions

module UInt16 =

    open System

    let inBytes (n: uint16) : byte [] =
        let bytes = BitConverter.GetBytes n

        if BitConverter.IsLittleEndian then
            bytes

        else
            Array.rev bytes


    let fromBytes (first: byte) (second: byte) : uint16 =
        let arr = [| first; second |]
        BitConverter.ToUInt16 arr

module UInt32 =

    open System

    let inBytes (n: uint32) : byte [] =
        let bytes = BitConverter.GetBytes n

        if BitConverter.IsLittleEndian then
            bytes

        else
            Array.rev bytes


    let fromBytes (first: byte) (second: byte) (third: byte) (fourth: byte) : uint32 =
        let arr = [| first; second; third; fourth |]
        BitConverter.ToUInt32 arr
