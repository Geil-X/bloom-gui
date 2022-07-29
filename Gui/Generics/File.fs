module Gui.Generics.File

open System
open System.IO
open System.Threading.Tasks
open Elmish
open FSharp.Json

open Extensions



// ---- File Operations --------------------------------------------------------

// TODO: Convert file writing operations to use Error messages


[<RequireQualifiedAccess>]
type ReadError =
    | DirectoryDoesNotExist of DirectoryInfo
    | FileDoesNotExist of FileInfo
    | FileAlreadyOpened of FileInfo
    | InvalidFilePermissions of FileInfo
    | UnknownException of exn


let readJsonTask<'a> (fileInfo: FileInfo) : Task<Result<'a, ReadError>> =
    task {
        try
            let fileStream =
                new StreamReader(fileInfo.OpenRead())

            let deserialized =
                Json.deserialize<'a> (fileStream.ReadToEnd())

            return Ok deserialized

        with
        | :? FileNotFoundException -> return ReadError.FileDoesNotExist fileInfo |> Error

        | :? DirectoryNotFoundException ->
            return
                ReadError.DirectoryDoesNotExist fileInfo.Directory
                |> Error

        | :? UnauthorizedAccessException -> return ReadError.InvalidFilePermissions fileInfo |> Error

        | :? IOException -> return ReadError.FileAlreadyOpened fileInfo |> Error


        | exn -> return ReadError.UnknownException exn |> Error
    }


[<RequireQualifiedAccess>]
type WriteError =
    | DirectoryDoesNotExist of DirectoryInfo
    | FileDoesNotExist of FileInfo
    | FileAlreadyOpened of FileInfo
    | InvalidFilePermissions of FileInfo
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
        | :? FileNotFoundException -> return WriteError.FileDoesNotExist fileInfo |> Error

        | :? DirectoryNotFoundException ->
            return
                WriteError.DirectoryDoesNotExist fileInfo.Directory
                |> Error

        | :? UnauthorizedAccessException -> return WriteError.InvalidFilePermissions fileInfo |> Error

        | :? IOException -> return WriteError.FileAlreadyOpened fileInfo |> Error
        | exn -> return Error(WriteError.UnknownException exn)
    }

// ---- Elmish Commands --------------------------------------------------------

let read (fileInfo: FileInfo) (msg: Result<'a, ReadError> -> 'Msg) : Cmd<'Msg> =
    Cmd.OfTask.perform readJsonTask<'a> fileInfo msg

let write (fileInfo: FileInfo) (data: 'a) (msg: Result<FileInfo, WriteError> -> 'Msg) : Cmd<'Msg> =
    Cmd.OfTask.perform (writeJsonTask<'a> fileInfo) data msg
