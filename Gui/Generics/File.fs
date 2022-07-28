module Gui.Generics.File

open System
open System.IO
open System.Threading.Tasks
open Elmish
open MBrace.FsPickler.Json

open Extensions



// ---- File Operations --------------------------------------------------------

let readTask<'a> (path: string) (deserializer: StreamReader -> 'a) : Task<'a> =
    task {
        use reader =
            new StreamReader(File.OpenRead(string path))

        return deserializer reader
    }


// Note: Can throw an exception
let writeTask<'a> (path: string) (serializer: StreamWriter -> 'a -> unit) (data: 'a) : Task<unit> =
    task {
        let fileDirectory =
            Path.parentDirectory path

        if not <| Directory.exists fileDirectory then
            Directory.createDirectory fileDirectory |> ignore

        use writer =
            new StreamWriter(File.OpenWrite(path))


        serializer writer data
    }

// ---- Serialization & Deserialization Types ----------------------------------

let private jsonSerializer =
    FsPickler.CreateJsonSerializer(indent = false)


let serializer<'a> (stream: TextWriter) (data: 'a) : unit = jsonSerializer.Serialize(stream, data)

let deserializer<'a> (stream: TextReader) : 'a = jsonSerializer.Deserialize<'a>(stream)


// ---- Elmish Commands --------------------------------------------------------

//type ReadingError =
//    | PathDoesNotExist
//
//type WritingError =
//    | DirectoryDoesNotExist

let load<'a> (path: string) (msg: Result<'a, exn> -> 'Msg) : Cmd<'Msg> =
    Cmd.OfTask.either (readTask<'a> path) deserializer<'a> (Ok >> msg) (Error >> msg)

let write<'a> (path: string) (data: 'a) (onError: exn -> 'Msg) : Cmd<'Msg> =
    Cmd.OfTask.attempt (writeTask<'a> path serializer<'a>) data onError
