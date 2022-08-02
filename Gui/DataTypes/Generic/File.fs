module Gui.DataTypes.File

open System
open System.IO
open System.Threading.Tasks
open Avalonia.Media
open Elmish
open Thoth.Json.Net

// ---- File Encoders & Decoders -----------------------------------------------


let fileInfoEncoder (fileInfo: FileInfo) : JsonValue =
    printfn $"{fileInfo.FullName}"
    Encode.string fileInfo.FullName

let fileInfoDecoder: Decoder<FileInfo> =
    fun path value ->
        if Decode.Helpers.isString value then
            Decode.Helpers.asString value |> FileInfo |> Ok

        else
            (path, ErrorReason.BadPrimitive("a file path", value))
            |> Error


let colorEncoder (color: Color) : JsonValue = Encode.string (string color)

let colorDecoder: Decoder<Color> =
    fun path value ->
        if Decode.Helpers.isString value then
            let mutable color: Color = Color()

            if Color.TryParse(Decode.Helpers.asString value, &color) then
                Ok color

            else
                (path, ErrorReason.BadPrimitive("Could not parse the string into a color value", value))
                |> Error

        else
            (path, ErrorReason.BadPrimitive("a color value", value))
            |> Error



// ---- All Coders ----

let extraCoders =
    Extra.empty
    |> Extra.withCustom fileInfoEncoder fileInfoDecoder
    |> Extra.withCustom colorEncoder colorDecoder


// ---- File Operations --------------------------------------------------------

[<RequireQualifiedAccess>]
type ReadError =
    | DirectoryDoesNotExist of DirectoryInfo
    | FileDoesNotExist of FileInfo
    | FileAlreadyOpened of FileInfo
    | InvalidFilePermissions of FileInfo
    | JsonDeserializationError of FileInfo * string
    | UnknownException of exn


let readJsonTask<'a> (fileInfo: FileInfo) : Task<Result<'a, ReadError>> =
    task {
        try
            let fileStream =
                new StreamReader(fileInfo.OpenRead())

            return
                Decode.Auto.fromString<'a> (fileStream.ReadToEnd(), extra = extraCoders)
                |> Result.mapError (fun errorString -> ReadError.JsonDeserializationError(fileInfo, errorString))

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

            writer.Write(Encode.Auto.toString (2, data, extra = extraCoders))

            return Ok fileInfo
        with
        | :? FileNotFoundException -> return WriteError.FileDoesNotExist fileInfo |> Error

        | :? DirectoryNotFoundException ->
            return
                WriteError.DirectoryDoesNotExist fileInfo.Directory
                |> Error

        | :? UnauthorizedAccessException ->
            return
                WriteError.InvalidFilePermissions fileInfo
                |> Error

        | :? IOException -> return WriteError.FileAlreadyOpened fileInfo |> Error
        | exn -> return Error(WriteError.UnknownException exn)
    }

// ---- Elmish Commands --------------------------------------------------------

let read (fileInfo: FileInfo) (msg: Result<'a, ReadError> -> 'Msg) : Cmd<'Msg> =
    Cmd.OfTask.perform readJsonTask<'a> fileInfo msg

let write (fileInfo: FileInfo) (data: 'a) (msg: Result<FileInfo, WriteError> -> 'Msg) : Cmd<'Msg> =
    Cmd.OfTask.perform (writeJsonTask<'a> fileInfo) data msg
