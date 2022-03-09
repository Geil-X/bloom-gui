module Gui.Log

let private print (level: string) s = printfn $"{level}: {s}"

// ---- Log Levels ----

let verbose s = print "Verbose" s
let debug s = print "Debug" s
let info s = print "Info" s
let warning s = print "Warning" s
let error s = print "Error" s
let fatal s = print "Fatal" s
