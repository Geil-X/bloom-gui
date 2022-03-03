module Extensions.Map

let update (key: 'Key) (f: 'T -> 'T) (map: Map<'Key, 'T>) : Map<'Key, 'T> = Map.change key (Option.map f) map