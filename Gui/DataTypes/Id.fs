namespace Gui.DataTypes

open System

type Id<'a> = Id of Guid

module Id =
    let create () = Id <| Guid.NewGuid()

    let shortName (Id guid: 'a Id) : string = guid.ToString("N").[..6]
