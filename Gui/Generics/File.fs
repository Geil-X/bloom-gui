module Gui.Generics.File

open System.IO
open System.Threading.Tasks
open Elmish
open FSharp.Json

open Extensions



// ---- File Operations --------------------------------------------------------

// TODO: Convert file writing operations to use Error messages


[<RequireQualifiedAccess>]
type ReadError = UnknownException of exn


let readJsonTask<'a> (fileInfo: FileInfo) : Task<Result<'a, ReadError>> =
    task {
        try
            let fileStream =
                new StreamReader(fileInfo.OpenRead())

            let deserialized =
                Json.deserialize<'a> (fileStream.ReadToEnd())

            return Ok deserialized

        with
        | exn -> return Error(ReadError.UnknownException exn)
    }


[<RequireQualifiedAccess>]
type WriteError =
    | UnknownException of exn

let writeJsonTask<'a> (fileInfo: FileInfo) (data: 'a) : Task<Result<FileInfo, WriteError>> =
    task {
        try
            // Ensure that the parent directory for the file we are writing exists
            fileInfo.Directory.Create()

            use fileStream = fileInfo.OpenWrite()
            use writer = new StreamWriter(fileStream)

            writer.Write(Json.serialize data)

            return Ok fileInfo
        with
        | exn -> return Error(WriteError.UnknownException exn)
    }

// ---- Elmish Commands --------------------------------------------------------

let read (fileInfo: FileInfo) (msg: Result<'a, ReadError> -> 'Msg) : Cmd<'Msg> =
    Cmd.OfTask.perform readJsonTask<'a> fileInfo msg

let write (fileInfo: FileInfo) (data: 'a) (msg: Result<FileInfo, WriteError> -> 'Msg) : Cmd<'Msg> =
    Cmd.OfTask.perform (writeJsonTask<'a> fileInfo) data msg
