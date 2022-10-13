module Extensions.Int

/// Takes a number and returns 1 if the number is zero or positive
/// and -1 if the number is negative.
let sign (x: int) : int = if x >= 0 then 1 else 0
