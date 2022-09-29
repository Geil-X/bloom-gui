module Gui.DataTypes.FlowerManager

open Avalonia.Input
open Math.Geometry
open Math.Units

open Extensions
open Gui
open Gui.DataTypes


// ---- Types ------------------------------------------------------------------


type State =
    { CanvasSize: Size2D<Meters>
      Flowers: Map<Flower Id, Flower>
      FlowerInteraction: FlowerInteraction
      Selected: Flower Id option
      NextI2c: I2cAddress
      FlowerStartPosition: Point2D<Meters, ScreenSpace> }

and FlowerInteraction =
    | Hovering of Flower Id
    | Pressing of PressedData
    | Dragging of DraggingData
    | NoInteraction

and PressedData =
    { Id: Flower Id
      MousePressedLocation: Point2D<Meters, ScreenSpace>
      InitialFlowerPosition: Point2D<Meters, ScreenSpace> }

and DraggingData =
    { Id: Flower Id
      DraggingDelta: Vector2D<Meters, ScreenSpace> }


type Msg =
    | OnFlowerEnter of Flower Id * MouseEvent<ScreenSpace>
    | OnFlowerLeave of Flower Id * MouseEvent<ScreenSpace>
    | OnFlowerMoved of Flower Id * MouseEvent<ScreenSpace>
    | OnFlowerPressed of Flower Id * MouseButtonEvent<ScreenSpace>
    | OnFlowerReleased of Flower Id * MouseButtonEvent<ScreenSpace>
    | OnBackgroundReleased of MouseButtonEvent<ScreenSpace>

// ---- Constants --------------------------------------------------------------

let private minMouseMovement =
    Length.cssPixels 10.

let private minMouseMovementSquared =
    Quantity.squared minMouseMovement


// ---- Init ---------------------------------------------------------------

let init () =
    { CanvasSize = Size2D.create Quantity.zero Quantity.zero
      Flowers = Map.empty
      FlowerInteraction = NoInteraction
      Selected = None
      NextI2c = I2cAddress.first
      FlowerStartPosition = Point2D.pixels 100. 100. }

let clear (manager: State) : State =
    { manager with
        Flowers = Map.empty
        FlowerInteraction = NoInteraction
        Selected = None
        NextI2c = I2cAddress.first }


// ---- Accessors ----------------------------------------------------------

let flowerFromId (id: Flower Id) (manger: State) : Flower option = Map.tryFind id manger.Flowers

let getFlowers (manager: State) : Flower seq = Map.values manager.Flowers

let getFlower (id: Flower Id) (manager: State) : Flower option = Map.tryFind id manager.Flowers

let getSelected (manager: State) : Flower option =
    Option.bind (fun id -> flowerFromId id manager) manager.Selected

let addFlower (flower: Flower) (manager: State) : State =
    { manager with Flowers = Map.add flower.Id flower manager.Flowers }

// ---- Modifiers --------------------------------------------------------------

let addFlowers (flowers: Flower seq) (manager: State) : State =
    let flowerMap =
        Seq.fold (fun map (flower: Flower) -> Map.add flower.Id flower map) Map.empty flowers

    { manager with Flowers = flowerMap }

let addNewFlower (manager: State) : State * Flower =
    let flower =
        Flower.basic $"Flower {Map.count manager.Flowers + 1}" manager.NextI2c
        |> Flower.setPosition manager.FlowerStartPosition

    let offset: Vector2D<Meters, ScreenSpace> =
        Vector2D.pixels 20. 20.

    let nextState =
        { manager with
            NextI2c = I2cAddress.next manager.NextI2c
            FlowerStartPosition = manager.FlowerStartPosition + offset }

    addFlower flower nextState, flower

/// Delete the flower with the given Id. If the Id doesn't exist then
/// nothing happens.
let deleteFlower (id: Flower Id) (manager: State) : State =
    { manager with Flowers = Map.remove id manager.Flowers }

/// Delete the flower that is selected. If nothing is selected, then nothing
/// happens.
let deleteSelected (manager: State) : State =
    match manager.Selected with
    | Some id -> deleteFlower id manager
    | None -> manager

let updateFlowers (f: Flower -> Flower) (manager: State) : State =
    { manager with Flowers = Map.map (fun _ -> f) manager.Flowers }

let updateFlowerSimulation (elapsedTime: Duration) (manager: State) : State =
    updateFlowers (Flower.tick elapsedTime) manager

let flowersFromI2cAddress (i2cAddress: I2cAddress) (flowers: Map<Flower Id, Flower>) : Flower seq =
    Map.filter (fun _ flower -> Flower.i2cAddress flower = i2cAddress) flowers
    |> Map.values

/// Try to select the flower with the given Id. If that flower does not
/// exist, nothing happens.
let select (id: Flower Id) (manager: State) : State =
    if Map.containsKey id manager.Flowers then
        { manager with Selected = Some id }
    else
        manager

/// Deselect all flowers.
let deselect (manager: State) : State = { manager with Selected = None }

// ---- Update -------------------------------------------------------------

let updateFlowerFromResponse (response: Response) (manager: State) : State =
    let flowersToUpdate =
        flowersFromI2cAddress response.I2cAddress manager.Flowers

    let updateFromResponse =
        Flower.connect
        >> Flower.setOpenPercent response.Position
        >> Flower.setTargetPercent response.Position
        >> Flower.setMaxSpeed response.MaxAngularSpeed
        >> Flower.setAcceleration response.AngularAcceleration

    let updatedFlowerMap =
        Seq.fold
            (fun flowerMap (flower: Flower) -> Map.update flower.Id updateFromResponse flowerMap)
            manager.Flowers
            flowersToUpdate

    { manager with Flowers = updatedFlowerMap }

// let updatedFlowerMap =
//     Seq.fold
//         (fun flowerMap (flower: Flower) -> Map.update flower.Id updateFromResponse flowerMap)
//         state.Flowers
//         flowersToUpdate


let updateFlower (id: Flower Id) (property: string) (f: 'a -> Flower -> Flower) (value: 'a) (manager: State) : State =
    if Option.contains id manager.Selected then
        Log.verbose $"Updated flower '{Id.shortName id}' with new {property} '{value}'"

        { manager with Flowers = Map.update id (f value) manager.Flowers }
    else
        manager

let updateMsg (msg: Msg) (state: State) : State =
    match msg with
    | OnFlowerEnter (flowerId, _) ->
        Log.verbose $"Flower: Hovering {Id.shortName flowerId}"
        { state with FlowerInteraction = Hovering flowerId }

    | OnFlowerLeave (flowerId, _) ->
        Log.verbose $"Flower: Pointer Left {Id.shortName flowerId}"

        { state with FlowerInteraction = NoInteraction }

    | OnFlowerMoved (flowerId, e) ->
        match state.FlowerInteraction with
        | Pressing pressing when
            pressing.Id = flowerId
            && Point2D.distanceSquaredTo pressing.MousePressedLocation e.Position > minMouseMovementSquared
            ->
            Log.verbose $"Flower: Start Dragging {Id.shortName flowerId}"

            let delta =
                pressing.InitialFlowerPosition
                - pressing.MousePressedLocation

            let newPosition = e.Position + delta

            { state with
                Flowers = Map.update pressing.Id (Flower.setPosition newPosition) state.Flowers
                FlowerInteraction =
                    Dragging
                        { Id = pressing.Id
                          DraggingDelta = delta } }

        | Dragging draggingData ->
            // Continue dragging
            let newPosition =
                e.Position + draggingData.DraggingDelta

            { state with Flowers = Map.update draggingData.Id (Flower.setPosition newPosition) state.Flowers }

        // Take no action
        | _ -> state


    | OnFlowerPressed (flowerId, e) ->
        if InputTypes.isPrimary e.MouseButton then
            match getFlower flowerId state with
            | Some pressed ->
                Log.verbose $"Flower: Pressed {Id.shortName pressed.Id}"

                { state with
                    FlowerInteraction =
                        Pressing
                            { Id = flowerId
                              MousePressedLocation = e.Position
                              InitialFlowerPosition = pressed.Position } }

            | None ->
                Log.error "Could not find the flower that was pressed"
                state
        else
            state


    | OnFlowerReleased (flowerId, e) ->
        if InputTypes.isPrimary e.MouseButton then
            match state.FlowerInteraction with
            | Dragging _ ->
                Log.verbose $"Flower: Dragging -> Hovering {Id.shortName flowerId}"

                { state with FlowerInteraction = Hovering flowerId }

            | Pressing _ ->
                Log.verbose $"Flower: Selected {Id.shortName flowerId}"

                { state with
                    FlowerInteraction = Hovering flowerId
                    Selected = Some flowerId }


            | flowerEvent ->
                Log.warning $"Unhandled event {flowerEvent}"
                state

        // Non primary button pressed
        else
            state

    | OnBackgroundReleased _ ->
        Log.verbose "Background: Pointer Released"

        { state with
            FlowerInteraction = NoInteraction
            Selected = None }
