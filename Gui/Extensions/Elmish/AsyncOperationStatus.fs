namespace Elmish

type AsyncOperationStatus<'input, 'output> =
    | Start of 'input
    | Finished of 'output

module AsyncOperationStatus =

    /// Start an async operation with no input
    let start = Start()

    let startWith = Start
