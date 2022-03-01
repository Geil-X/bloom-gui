module Extensions.Map

let update (key: 'Key) (f: 'T -> 'T) (map: Map<'Key, 'T>) : Map<'Key, 'T> = Map.change key (Option.map f) map

let keys map = Map.toSeq map |> Seq.map fst

let values map = Map.toSeq map |> Seq.map snd
