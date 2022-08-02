module Tests.Mock

open Avalonia.Controls
open Avalonia.Platform
open Moq

let IWindowImpl = Mock<IWindowImpl>().Object

let Window =
    Mock<Window>(IWindowImpl).Object
