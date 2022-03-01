module Extensions.List

/// Filter out all the values in a list that are None and leave only real values in the list.
let filterNone l =
    List.fold
        (fun acc x ->
            match x with
            | Some n -> n :: acc
            | None -> acc)
        []
        l
