[<AutoOpen>]
module Avalonia.FuncUI.DSL.Border

open Avalonia.Collections
open Avalonia.Controls
open Avalonia.FuncUI.Builder
open Avalonia.FuncUI.Types

type Border with

    static member borderDashArray<'t when 't :> Border>(array: float list) : IAttr<'t> =
        AttrBuilder<'t>
            .CreateProperty<AvaloniaList<float>>(Border.BorderDashArrayProperty, AvaloniaList(array), ValueNone)

    static member borderDashOffset<'t when 't :> Border>(offset: float) : IAttr<'t> =
        AttrBuilder<'t>
            .CreateProperty<float>(Border.BorderDashOffsetProperty, offset, ValueNone)
