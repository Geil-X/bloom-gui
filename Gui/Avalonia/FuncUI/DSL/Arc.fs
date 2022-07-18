namespace Avalonia.FuncUI.DSL

[<AutoOpen>]
module ArcSegment =
    open Avalonia
    open Avalonia.Controls.Shapes
    open System
    open Avalonia.Media
    open Avalonia.Controls
    open Avalonia.Controls.Templates
    open Avalonia.FuncUI.Types
    open Avalonia.FuncUI.Builder

    let create (attrs: IAttr<ArcSegment> list) : IView<ArcSegment> = ViewBuilder.Create<ArcSegment>(attrs)

[<AutoOpen>]
module Arc =
    open Avalonia
    open Avalonia.Controls.Shapes
    open System
    open Avalonia.Media
    open Avalonia.Controls
    open Avalonia.Controls.Templates
    open Avalonia.FuncUI.Types
    open Avalonia.FuncUI.Builder

    let create (attrs: IAttr<Arc> list) : IView<Arc> = ViewBuilder.Create<Arc>(attrs)

    type Arc with
        static member startAngle<'t when 't :> Arc>(angle: float) : IAttr<'t> =
            AttrBuilder<'t>
                .CreateProperty<float>(Arc.StartAngleProperty, angle, ValueNone)

        static member sweepAngle<'t when 't :> Arc>(angle: float) : IAttr<'t> =
            AttrBuilder<'t>
                .CreateProperty<float>(Arc.SweepAngleProperty, angle, ValueNone)