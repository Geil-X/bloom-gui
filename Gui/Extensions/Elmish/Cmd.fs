module Elmish.Cmd

open System
open Avalonia.Threading
open Elmish

let repeatedInMillis (ms: float) (msg: 'Msg) (_: 'State) : Cmd<'Msg> =
    let sub dispatch =
        let invoke () =
            msg |> dispatch
            true

        DispatcherTimer.Run(Func<_>(invoke), TimeSpan.FromMilliseconds ms)
        |> ignore

    Cmd.ofSub sub
