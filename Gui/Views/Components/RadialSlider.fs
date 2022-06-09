module Gui.Views.Components.RadialSlider

open System
open Avalonia
open Avalonia.Controls.Primitives
open Avalonia.Controls.Shapes
open Avalonia.Input
open Avalonia.Layout
open Avalonia.Controls
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.FuncUI.Types
open Avalonia.FuncUI.Elmish.ElmishHook
open Avalonia.Media

open Gui
open Gui.DataTypes


type RadialSlider() =
    inherit RangeBase()

    let angleFromMinMaxValues value min max =
        let range = max - min
        let vm = value - min
        360. * vm / range
        
    let valueFromMinMaxAngle angle min max=
        let range = max - min
        let anglePercent = angle / 360.
        min + (range * anglePercent)
    
        
        
    let angleOf(pos: Point, radius: float) =
        let center = Point(radius, radius)
        let xDiff = center.X - pos.X
        let yDiff = center.Y - pos.Y
        let r =
            xDiff * xDiff + yDiff * yDiff
            |> sqrt
            
        if pos.X < radius then
            (acos (center.Y - pos.Y) / r)
        else
            (2. * Math.PI) - (acos (center.Y - pos.Y) / r)
            

    let mutable pressed = false

    new() as this =
        RadialSlider.MaximumProperty.Changed.Subscribe(this.CalibrateAngles) |> ignore
        RadialSlider.MinimumProperty.Changed.Subscribe(this.CalibrateAngles)|> ignore
        RadialSlider.ValueProperty.Changed.Subscribe(this.CalibrateAngles) |> ignore
        RadialSlider.ClipToBoundsProperty.OverrideDefaultValue<RadialSlider>(false)
        
        RadialSlider.BoundsProperty.Changed.Subscribe(this.UpdateRadius) |> ignore
//        RadialSlider.StrokeWidthProperty.Changed.Subscribe(UpdateRadius)
        
        RadialSlider.MaximumProperty.OverrideMetadata<RadialSlider>(DirectPropertyMetadata<double>(100.));
        RadialSlider.MinimumProperty.OverrideMetadata<RadialSlider>(DirectPropertyMetadata<double>(0.));
        RadialSlider.ValueProperty.OverrideMetadata<RadialSlider>(DirectPropertyMetadata<double>(25.));

        RadialSlider.AffectsRender<RadialSlider>(XAngleProperty, YAngleProperty)
        
    override this.OnPointerMoved(e: PointerEventArgs ) =
        base.OnPointerMoved(e)
        if pressed then
            this.UpdateValueFromPoint(e.GetCurrentPoint(this).Position)
            
            
    member this.UpdateValueFromPoint(Point p)
        let angle = angleOf p this.Radius
        
        this.Value <- round (valueFromMinMaxAngle angle this.Minimum this.Maximum)

    member this.UpdateRadius(e: AvaloniaPropertyChangedEventArgs) =
        match e.Sender with
        | :? RadialSlider as r -> ()
        | _ -> ()
    
    member this.CalibrateAngles(e: AvaloniaPropertyChangedEventArgs) =
        match e.Sender with
        | null ->
            let pr = e.Sender :?> RadialSlider
            pr.XAngle = -90
            pr.YAngle
            pr.InvalidateVisual();

        | _ -> ()






type Model = { Percentage: ClampedPercentage }

type Msg = ValueChanged of double

let init : Model * Cmd<Msg> =
    { Percentage = ClampedPercentage.zero }, Cmd.none

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | ValueChanged newValue ->
        { model with
              Percentage = ClampedPercentage.decimal newValue },
        Cmd.none


let radialSlider =
    Component
        (fun ctx ->
            let model, dispatch = ctx.useElmish (init, update)

            let size = 64. // px
            let radius = size / 2. // px
            let sliderThickness = 5. // px
            let thumbSize = 10. //px

            let handle =
                let angleRad =
                    Math.PI
                    + (Math.PI
                       * ClampedPercentage.inDecimal model.Percentage)

                let xOffset =
                    (radius - sliderThickness / 2.) * cos angleRad

                let yOffset =
                    (radius - sliderThickness / 2.) * sin angleRad

                let x = (size / 2.) + xOffset
                let y = (size / 2.) + yOffset

                Ellipse.create [
                    Ellipse.width thumbSize
                    Ellipse.height thumbSize
                    Ellipse.top (y - thumbSize / 2.)
                    Ellipse.left (x - thumbSize / 2.)
                    Ellipse.fill Theme.colors.blue
                ]

            let label =
                let percentage =
                    ClampedPercentage.inPercentage model.Percentage
                    |> round

                TextBlock.create [ TextBlock.text $"{percentage}%%" ]

            let slider =
                Slider.create [
                    Slider.minimum 0.
                    Slider.maximum 1.
                    Slider.value (ClampedPercentage.inDecimal model.Percentage)
                    Slider.onValueChanged (fun newPercentage -> ValueChanged newPercentage |> dispatch)
                ]



            let currentPercentage =
                Arc.create [
                    Arc.strokeThickness sliderThickness
                    Arc.stroke Theme.palette.primary
                    Arc.width size
                    Arc.height size
                    Arc.startAngle 180.
                    Arc.sweepAngle (
                        180.
                        * ClampedPercentage.inDecimal model.Percentage
                    )
                ]

            let backgroundArc =
                Arc.create [
                    Arc.strokeThickness sliderThickness
                    Arc.stroke Theme.palette.canvasBackground
                    Arc.width size
                    Arc.height size
                    Arc.startAngle 180.
                    Arc.sweepAngle 180.
                ]

            let radial =
                Canvas.create [
                    Canvas.height size
                    Canvas.width size
                    Canvas.children [ backgroundArc; currentPercentage; handle ]
                ]

            StackPanel.create [
                StackPanel.children [ label; slider; radial ]
            ]
            :> IView)



let create a =
    ContentControl.create [
        ContentControl.content radialSlider
    ]
