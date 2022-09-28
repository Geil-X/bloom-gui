module Gui.Log

type Level =
    | Verbose
    | Debug
    | Info
    | Warning
    | Error
    | Fatal

let levelToString level =
    match level with
    | Verbose -> "Verbose"
    | Debug -> "Debug  "
    | Info -> "Info   "
    | Warning -> "Warn   "
    | Error -> "Error  "
    | Fatal -> "Fatal  "

/// The current log level.
let mutable LogLevel = Debug

/// The interface loggers need to implement.
type ILogger =
    abstract Log: Level -> Printf.StringFormat<'a, unit> -> unit

/// Writes to console.
let ConsoleLogger =
    { new ILogger with
        member _.Log (level: Level) (format: Printf.StringFormat<'a, unit>) : unit =
            if level >= LogLevel then
                Printf.kprintf (printfn "[%s] %s" (levelToString level)) format
                |> ignore }

/// Defines which logger to use.
let mutable private DefaultLogger =
    ConsoleLogger

/// Logs a message with the specified logger.
let logUsing (logger: ILogger) = logger.Log

/// Logs a message using the default logger.
let log level message = logUsing DefaultLogger level message

// ---- Log Levels ----

let verbose s = log Verbose s
let debug s = log Debug s
let info s = log Info s
let warning s = log Warning s
let error s = log Error s
let fatal s = log Fatal s
