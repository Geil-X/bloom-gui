module Gui.DataTypes.File

open System
open System.IO
open System.Threading.Tasks
open Avalonia.Media
open Elmish
open Thoth.Json.Net
open Math.Units

// ---- Encoders & Decoders -----------------------------------------------

module Quantity =
    let encoder<'Units> (q: Quantity<'Units>) : JsonValue = Encode.float q.Value

    let decoder<'Units> : Decoder<Quantity<'Units>> =
        fun path value ->
            if Decode.Helpers.isNumber value then
                Decode.Helpers.asFloat value
                |> Quantity.create<'Units>
                |> Ok
            else
                (path, ErrorReason.BadPrimitive("a quantity value", value))
                |> Error

module FileInfo =
    let encoder (fileInfo: FileInfo) : JsonValue = Encode.string fileInfo.FullName

    let decoder: Decoder<FileInfo> =
        fun path value ->
            if Decode.Helpers.isString value then
                Decode.Helpers.asString value |> FileInfo |> Ok

            else
                (path, ErrorReason.BadPrimitive("a file path", value))
                |> Error

module Color =
    let encoder (color: Color) : JsonValue = Encode.string (string color)

    let decoder: Decoder<Color> =
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
    |> Extra.withCustom FileInfo.encoder FileInfo.decoder
    |> Extra.withCustom Color.encoder Color.decoder
    |> Extra.withCustom Quantity.encoder<Meters> Quantity.decoder<Meters>
    |> Extra.withCustom Quantity.encoder<Radians> Quantity.decoder<Radians>
    |> Extra.withCustom Quantity.encoder<Percentage> Quantity.decoder<Percentage>
    |> Extra.withCustom Quantity.encoder<Rate<Radians, Seconds>> Quantity.decoder<Rate<Radians, Seconds>>
    |> Extra.withCustom
        Quantity.encoder<Rate<Rate<Radians, Seconds>, Seconds>>
        Quantity.decoder<Rate<Rate<Radians, Seconds>, Seconds>>


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
