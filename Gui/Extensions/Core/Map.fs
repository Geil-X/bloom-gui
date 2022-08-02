module Extensions.Map

let update (key: 'Key) (f: 'T -> 'T) (map: Map<'Key, 'T>) : Map<'Key, 'T> = Map.change key (Option.map f) map

let values (map: Map<'Key, 'T>) : 'T seq = Map.toSeq map |> Seq.map snd

let keys (map: Map<'Key, 'T>) : 'Key seq = Map.toSeq map |> Seq.map fst
