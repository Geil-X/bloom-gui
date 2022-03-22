namespace Avalonia.FuncUI.DSL

[<AutoOpen>]
module Svg =
    open Avalonia.FuncUI.Builder
    open Avalonia.FuncUI.Types
    open Avalonia.Svg.Skia

    let create (attrs: IAttr<Svg> list) : IView<Svg> = ViewBuilder.Create<Svg>(attrs)
    
    type Svg with
        static member path<'t when 't :> Svg>(path: string) : IAttr<'t> =
            AttrBuilder<'t>
                .CreateProperty<string>(Svg.PathProperty, path, ValueNone)