module Extensions.Guid

open System

let shortName (guid: Guid) : string = guid.ToString("N").[..6]
