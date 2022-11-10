namespace Gui.DataTypes

open System.IO

type AppConfig = { RecentFiles: FileInfo list }

module AppConfig =
    open Gui.DataTypes
    open Extensions

    let init = { RecentFiles = [] }


    let setRecentFiles (recentFiles: FileInfo list) (appConfig: AppConfig) : AppConfig =
        { appConfig with RecentFiles = recentFiles }

    let addRecentFile (recentFile: FileInfo) (appConfig: AppConfig) : AppConfig =
        let containsFile =
            appConfig.RecentFiles
            |> List.exists (fun file -> file.FullName = recentFile.FullName)

        if not containsFile then
            setRecentFiles (recentFile :: appConfig.RecentFiles) appConfig
        else
            appConfig


    let configPath: FileInfo =
        let environmentVariables =
            System.Environment.GetEnvironmentVariables()
            |> Seq.cast<System.Collections.DictionaryEntry>
            |> Seq.map (fun d -> d.Key :?> string, d.Value :?> string)
            |> Map.ofSeq

        /// Get an environment variables contents. This fails with an error message if this operation is not completed.
        let getEnv key =
            match Map.tryFind key environmentVariables with
            | Some value -> value
            | None -> failwith $"Could not find the '{key}' variable in the environmental variables."

        let bloomDirectory = "bloom"
        let configFileName = "config.json"

        match OS.getOS with
        | OS.OSX
        | OS.Linux
        | OS.Raspbian ->
            let userDirectory = getEnv "HOME"

            userDirectory
            ./ ".config"
            ./ bloomDirectory
            ./ configFileName


        | OS.Windows ->
            // On windows we are using the '.../AppData/' directory to hold user configuration. This folder is
            // for user settings that stay on the local machine
            let appdata = getEnv "LOCALAPPDATA"

            appdata ./ bloomDirectory ./ "config.json"

        |> FileInfo
