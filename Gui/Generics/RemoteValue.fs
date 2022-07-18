namespace Gui.DataTypes


type RemoteValue<'a> =
    | Local of 'a
    | Both of Both<'a>

and Both<'a> = { Local: 'a; Remote: 'a }

module RemoteValue =

    let local (value: RemoteValue<'a>) : 'a =
        match value with
        | Local local -> local
        | Both both -> both.Local

    let remote (value: RemoteValue<'a>) : 'a option =
        match value with
        | Local _ -> None
        | Both both -> Some both.Remote

    let setLocal (value: 'a) (remoteValue: RemoteValue<'a>) : RemoteValue<'a> =
        match remoteValue with
        | Local _ -> Local value
        | Both both -> Both { both with Local = value }

    let setRemote (value: 'a) (remoteValue: RemoteValue<'a>) : RemoteValue<'a> =
        match remoteValue with
        | Local local -> Both { Local = local; Remote = value }
        | Both both -> Both { both with Remote = value }
