[<AutoOpen>]
module Gui.Extensions.Path

open System.IO

/// Custom operator for combining paths
let (./) path1 path2 = Path.Combine(path1, path2)