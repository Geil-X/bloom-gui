module Gui.Generics.File

open System.IO
open System.Threading.Tasks
open Elmish
open FSharp.Json

open Extensions



// ---- File Operations --------------------------------------------------------

let readJsonTask<'a> (path: string) : Task<'a> =
    task {
        use fileStream =
            new StreamReader(File.OpenRead(path))

        return Json.deserialize<'a> (fileStream.ReadToEnd())
    }


// Note: Can throw an exception
let writeJsonTask<'a> (path: string) (data: 'a) : Task<unit> =
    task {
        let fileDirectory =
            Path.parentDirectory path

        if not <| Directory.exists fileDirectory then
            Directory.createDirectory fileDirectory |> ignore

        use writer =
            new StreamWriter(File.OpenWrite(path))

        writer.Write(Json.serialize data)
    }

// ---- Elmish Commands --------------------------------------------------------

//type ReadingError =
//    | PathDoesNotExist
//
//type WritingError =
//    | DirectoryDoesNotExist

let load (path: string) (msg: Result<'a, exn> -> 'Msg) : Cmd<'Msg> =
    Cmd.OfTask.either readJsonTask<'a> path (Ok >> msg) (Error >> msg)

let write (path: string) (data: 'a) (msg: Result<unit, exn> -> 'Msg) : Cmd<'Msg> =
    Cmd.OfTask.either (writeJsonTask<'a> path) data (Ok >> msg) (Error >> msg)
