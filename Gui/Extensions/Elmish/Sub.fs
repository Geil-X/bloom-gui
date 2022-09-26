module Elmish.Sub

open System.Runtime.InteropServices
open System.Threading
open Elmish
open Math.Units

let timer (fps: int) (msg: Duration -> 'Msg) =
    let sub dispatch =
        let ms = 1000. / float fps |> int
        let duration = Duration.milliseconds ms

        new Timer(TimerCallback(fun _ -> msg duration |> dispatch), null, ms, ms)
        |> GCHandle.Alloc // Store the timer object handle to prevent garbage collection
        |> ignore

    Cmd.ofSub sub
