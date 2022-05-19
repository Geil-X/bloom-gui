namespace Extensions

module UInt16 =

    open System

    let inBytes (n: uint16) =
        let bytes = BitConverter.GetBytes n

        if BitConverter.IsLittleEndian then
            bytes
            |> (fun s ->
                printfn $"{List.ofArray s}"
                s)

        else
            Array.rev bytes
