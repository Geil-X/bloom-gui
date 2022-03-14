module Extensions.String


/// Try parsing a string into an integer. Will return None on failure
let parseInt (s: string) =
    try
        Some(int s)
    with
    | _ -> None
    
/// Try parsing a string into an integer. Will return None on failure
let parseByte (s: string) =
    try
        Some(byte s)
    with
    | _ -> None
    
/// Try parsing a string into a floating point number. Will return None on failure
let parseFloat (s: string) =
    try
        Some(float s)
    with
    | _ -> None