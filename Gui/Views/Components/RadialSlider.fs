[<AutoOpen>]
module Gui.Views.Components.RadialSlider

open System
open Avalonia
open Avalonia.Controls.Primitives
open Avalonia.FuncUI.Builder
open Avalonia.Input
open Avalonia.Controls
open Avalonia.FuncUI.Types
open Avalonia.Media


type RadialSlider() as this =
    inherit RangeBase()

    do
        RadialSlider.MaximumProperty.Changed.Subscribe(this.CalibrateAngles)
        |> ignore

        RadialSlider.MinimumProperty.Changed.Subscribe(this.CalibrateAngles)
        |> ignore

        RadialSlider.ValueProperty.Changed.Subscribe(this.CalibrateAngles)
        |> ignore

        RadialSlider.ClipToBoundsProperty.OverrideDefaultValue<RadialSlider>(false)

        RadialSlider.BoundsProperty.Changed.Subscribe(this.UpdateRadius)
        |> ignore

        // TODO: Try to fix this. Don't know how important it is though
//        RadialSlider.StrokeWidthProperty.Changed.Subscribe(this.UpdateRadius)
//        |> ignore

        RadialSlider.MaximumProperty.OverrideMetadata<RadialSlider>(DirectPropertyMetadata<float>(100.))
        RadialSlider.MinimumProperty.OverrideMetadata<RadialSlider>(DirectPropertyMetadata<float>(0.))
        RadialSlider.ValueProperty.OverrideMetadata<RadialSlider>(DirectPropertyMetadata<float>(25.))

    //        RadialSlider.AffectsRender<RadialSlider>(this.XAngleProperty, this.YAngleProperty)

    // ---- Math Helper Functions ----------------------------------------------

    let angleFromMinMaxValues value min max =
        let range = max - min
        let vm = value - min
        360. * vm / range

    let valueFromMinMaxAngle angle min max =
        let range = max - min
        let anglePercent = angle / 360.
        min + (range * anglePercent)

    let angleOf (pos: Point) (radius: float) =
        let center = Point(radius, radius)
        let xDiff = center.X - pos.X
        let yDiff = center.Y - pos.Y

        let r =
            xDiff * xDiff + yDiff * yDiff |> sqrt

        if pos.X < radius then
            (acos (center.Y - pos.Y) / r)
        else
            (2. * Math.PI) - (acos (center.Y - pos.Y) / r)

    // ---- Properties ---------------------------------------------------------

    member val pressed: bool = false with get, set
    member val radius: float = 0.

    member this.Radius
        with get () = this.radius
        and set value =
            ``base``.SetAndRaise(this.RadiusProperty, ref this.radius, value)
            |> ignore

    member this.RadiusProperty: DirectProperty<RadialSlider, float> =
        AvaloniaProperty.RegisterDirect<RadialSlider, float>(nameof this.Radius, (fun o -> o.Radius))

    member this.StrokeWidth
        with get () = this.GetValue(this.StrokeWidthProperty)
        and set value =
            this.SetValue(this.StrokeWidthProperty, value)
            |> ignore

    member this.StrokeWidthProperty: StyledProperty<int> =
        AvaloniaProperty.Register<RadialSlider, int>(nameof this.StrokeWidth, 20)

    member this.ForegroundColor
        with get () = this.GetValue(this.ForegroundColorProperty)
        and set value =
            this.SetValue(this.ForegroundColorProperty, value)
            |> ignore

    member this.ForegroundColorProperty: StyledProperty<Color> =
        AvaloniaProperty.Register<RadialSlider, Color>(nameof this.ForegroundColor)

    member this.BackgroundColor
        with get () = this.GetValue(this.BackgroundColorProperty)
        and set value =
            this.SetValue(this.BackgroundColorProperty, value)
            |> ignore

    member this.BackgroundColorProperty: StyledProperty<Color> =
        AvaloniaProperty.Register<RadialSlider, Color>(nameof this.BackgroundColor)

    member val x_angle = 0.

    member this.XAngle
        with get () = this.x_angle
        and set value =
            this.SetAndRaise(this.XAngleProperty, ref this.x_angle, value)
            |> ignore

    member this.XAngleProperty: DirectProperty<RadialSlider, float> =
        AvaloniaProperty.RegisterDirect<RadialSlider, float>(nameof this.XAngle, (fun o -> o.XAngle))

    member val y_angle = 0.

    member this.YAngle
        with get () = this.y_angle
        and set value =
            this.SetAndRaise(this.YAngleProperty, ref this.y_angle, value)
            |> ignore

    member this.YAngleProperty: DirectProperty<RadialSlider, float> =
        AvaloniaProperty.RegisterDirect<RadialSlider, float>(nameof this.YAngle, (fun o -> o.YAngle))

    member this.RoundDigits
        with get () = this.GetValue(this.RoundDigitsProperty)
        and set value =
            this.SetValue(this.RoundDigitsProperty, value)
            |> ignore

    member this.RoundDigitsProperty: StyledProperty<int> =
        AvaloniaProperty.Register<RadialSlider, int>(nameof this.RoundDigits, 0)


    member val child: Control = null

    member this.Child
        with get () = this.child
        and set value =
            this.SetAndRaise(this.ChildProperty, ref this.child, value)
            |> ignore

    member this.ChildProperty: DirectProperty<RadialSlider, Control> =
        let getter (o: RadialSlider) : Control = o.Child
        let setter (o: RadialSlider) (v: Control) = o.Child <- v

        AvaloniaProperty.RegisterDirect<RadialSlider, Control>(nameof this.Child, getter, setter)

    // ---- Events ---------------------------------------------------------------------------

    override this.OnPointerMoved(e: PointerEventArgs) =
        ``base``.OnPointerMoved(e)

        if this.pressed then
            this.UpdateValueFromPoint(e.GetCurrentPoint(this).Position)

    override this.OnPointerPressed(e) =
        ``base``.OnPointerPressed(e)
        this.pressed <- true

    override this.OnPointerReleased(e) =
        ``base``.OnPointerReleased(e)
        this.pressed <- false


    // ---- Updates --------------------------------------------------------------------------

    member this.UpdateValueFromPoint(p: Point) =
        let angle = angleOf p this.Radius
        this.Value <- round (valueFromMinMaxAngle angle this.Minimum this.Maximum)

    member this.UpdateRadius(e: AvaloniaPropertyChangedEventArgs) =
        match e.Sender with
        | :? RadialSlider as r -> r.Radius <- (r.Bounds.Width - (float r.StrokeWidth * 2.)) / 2.
        | _ -> ()

    member this.CalibrateAngles(e: AvaloniaPropertyChangedEventArgs) =
        match e.Sender with
        | :? RadialSlider as pr ->
            pr.XAngle <- -90.
            pr.YAngle <- angleFromMinMaxValues pr.Value pr.Minimum pr.Maximum
            pr.InvalidateVisual()
        | _ -> ()


let create (attrs: IAttr<RadialSlider> list) : IView<RadialSlider> = ViewBuilder.Create<RadialSlider>(attrs)




//// ---- Elmish Stuff ---------------------------------------------------------------------
//
//type Model = { Percentage: ClampedPercentage }
//
//type Msg = ValueChanged of float
//
//let init: Model * Cmd<Msg> =
//    { Percentage = ClampedPercentage.zero }, Cmd.none
//
//let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
//    match msg with
//    | ValueChanged newValue ->
//        { model with
//              Percentage = ClampedPercentage.decimal newValue },
//        Cmd.none
//
//
//let radialSlider =
//    Component
//        (fun ctx ->
//            let model, dispatch = ctx.useElmish (init, update)
//
//            let size = 64. // px
//            let radius = size / 2. // px
//            let sliderThickness = 5. // px
//            let thumbSize = 10. //px
//
//            let handle =
//                let angleRad =
//                    Math.PI
//                    + (Math.PI
//                       * ClampedPercentage.inDecimal model.Percentage)
//
//                let xOffset =
//                    (radius - sliderThickness / 2.) * cos angleRad
//
//                let yOffset =
//                    (radius - sliderThickness / 2.) * sin angleRad
//
//                let x = (size / 2.) + xOffset
//                let y = (size / 2.) + yOffset
//
//                Ellipse.create [
//                    Ellipse.width thumbSize
//                    Ellipse.height thumbSize
//                    Ellipse.top (y - thumbSize / 2.)
//                    Ellipse.left (x - thumbSize / 2.)
//                    Ellipse.fill Theme.colors.blue
//                ]
//
//            let label =
//                let percentage =
//                    ClampedPercentage.inPercentage model.Percentage
//                    |> round
//
//                TextBlock.create [ TextBlock.text $"{percentage}%%" ]
//
//            let slider =
//                Slider.create [
//                    Slider.minimum 0.
//                    Slider.maximum 1.
//                    Slider.value (ClampedPercentage.inDecimal model.Percentage)
//                    Slider.onValueChanged (fun newPercentage -> ValueChanged newPercentage |> dispatch)
//                ]
//
//
//
//            let currentPercentage =
//                Arc.create [
//                    Arc.strokeThickness sliderThickness
//                    Arc.stroke Theme.palette.primary
//                    Arc.width size
//                    Arc.height size
//                    Arc.startAngle 180.
//                    Arc.sweepAngle (
//                        180.
//                        * ClampedPercentage.inDecimal model.Percentage
//                    )
//                ]
//
//            let backgroundArc =
//                Arc.create [
//                    Arc.strokeThickness sliderThickness
//                    Arc.stroke Theme.palette.canvasBackground
//                    Arc.width size
//                    Arc.height size
//                    Arc.startAngle 180.
//                    Arc.sweepAngle 180.
//                ]
//
//            let radial =
//                Canvas.create [
//                    Canvas.height size
//                    Canvas.width size
//                    Canvas.children [ backgroundArc; currentPercentage; handle ]
//                ]
//
//            StackPanel.create [
//                StackPanel.children [ label; slider; radial ]
//            ]
//            :> IView)
//
//
//
//let create a =
//    ContentControl.create [
//        ContentControl.content radialSlider
//    ]
