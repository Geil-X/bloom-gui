module Gui.Log

let private print level (s: string) = printfn $"{level}: {s}"

// ---- Log Levels ----

let verbose = print "Verbose"

let debug = print "Debug"

let info = print "Info"

let warning = print "Warning"

let error = print "Error"

let fatal = print "Fatal"
