module Extensions.Option

let split (ofSome: 'a -> 'b) (ofNone: 'b) (o: 'a option) : 'b =
    match o with
    | Some a -> ofSome a
    | None -> ofNone
