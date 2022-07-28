module Gui.Generics.Serializer


open System
open MBrace.FsPickler.Json

let private jsonSerializer =
    FsPickler.CreateJsonSerializer(indent = false)
    
    
let serialize (stream: IO.TextWriter) data  : unit =
    jsonSerializer.Serialize(stream, data)

let deserialize (stream: IO.TextReader) : 'a =
    jsonSerializer.Deserialize<'a>(stream)
