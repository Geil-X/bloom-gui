namespace Extensions

[<AutoOpen>]
module PathOperators =
    
    open System.IO

    /// Custom operator for combining paths
    let (./) path1 path2 = Path.Combine(path1, path2)

module Path =

    open System.IO

    /// Custom operator for combining paths
    let (./) path1 path2 = Path.Combine(path1, path2)
    
    let parentDirectory (path: string) = Path.GetDirectoryName path
    
module Directory =
    
    open System.IO
    
    let exists = Directory.Exists