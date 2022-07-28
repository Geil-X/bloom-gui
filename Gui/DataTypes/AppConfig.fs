module Gui.DataTypes.AppConfig

open Avalonia.FuncUI.Elmish.ElmishHook
open Gui
open Gui.DataTypes
open Gui.Generics
open Gui.Extensions

type AppConfig = { RecentFiles: PathName list }

let setRecentFiles (recentFiles: PathName list) (appConfig: AppConfig) : AppConfig =
    { appConfig with RecentFiles = recentFiles }

let addRecentFile (recentFile: PathName) (appConfig: AppConfig) : AppConfig =
    setRecentFiles (recentFile :: appConfig.RecentFiles) appConfig


let configPath =
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

    match OperatingSystem.get with
    | OSX ->
        let userDirectory = getEnv "HOME"

        userDirectory
        ./ ".config"
        ./ bloomDirectory
        ./ configFileName


    | Linux ->
        let userDirectory = getEnv "HOME"

        userDirectory
        ./ ".config"
        ./ bloomDirectory
        ./ configFileName


    | Windows ->
        // On windows we are using the '.../AppData/Roaming' directory to hold user configuration. This folder is where
        // user settings are generally stored and can be transferred from machine to machine.
        let appdata = getEnv "APPDATA"

        appdata ./ bloomDirectory ./ "config.json"

// Elmish links

let loadConfigFile pathName (msg: AppConfig -> 'Msg) : Cmd<'Msg> = ()

let writeConfigFile pathName appConfig : Cmd<'Msg> = ()
