namespace Gui.DataTypes

open System

type Id<'a> =
    | Id of Guid
    override this.ToString() : string =
        match this with
        | Id guid -> guid.ToString("N").[..6]



module Id =
    let create () = Id <| Guid.NewGuid()

    let shortName (guid: 'a Id) : string = guid.ToString()
