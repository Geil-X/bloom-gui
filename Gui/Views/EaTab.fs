/// Tab containing all the tracking information for running the evolutionary
/// algorithm.
module Gui.Views.EaTab

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.Layout
open Elmish
open System

open Gui
open Gui.DataTypes
open Gui.Views.Components

// ---- Types ------------------------------------------------------------------

type BloomModel = int array

type State =
    { EvolutionaryAlgorithm: EvolutionaryAlgorithm<BloomModel>
      Fitness: float }

type Msg =
    | ChangeFitness of float
    | SetFitness



// ---- EA Functions -----------------------------------------------------------

let maxValue = 10
let private random = Random()

let eaInitialization () : BloomModel =
    let bloomModelLength = 7
    Array.replicate bloomModelLength 0

let mutator (model: BloomModel) : BloomModel =
    if model.Length <= 0 then
        model

    else
        let mutationIndex =
            random.Next(0, model.Length - 1)

        Array.updateAt mutationIndex (random.Next(0, maxValue)) model

/// Performing crossover take the first chunk of the first array and the last
/// chunk of the second array. The chunk size is is randomly picked based on
/// the size of the array.
let crossover (first: BloomModel) (second: BloomModel) : BloomModel =
    if first.Length <> second.Length then
        let errorMsg =
            "Cannot perform crossover: Length of the two children models are not the same."

        Log.error $"{errorMsg}"
        failwith errorMsg

    else if first.Length < 2 then
        let errorMsg =
            "Cannot perform crossover: Length of BloomModel is invalid."

        Log.error $"{errorMsg}"
        failwith errorMsg

    else
        let crossoverIndex =
            random.Next(1, first.Length - 1)

        Array.append
            (Array.take crossoverIndex first)
            (Array.sub second crossoverIndex (second.Length - crossoverIndex))

// ---- Tab Functions ----------------------------------------------------------

let init () : State =
    { Fitness = 0.
      EvolutionaryAlgorithm =
        EvolutionaryAlgorithm.withInitialization eaInitialization
        |> EvolutionaryAlgorithm.withMutator mutator
        |> EvolutionaryAlgorithm.withCrossover crossover
        |> EvolutionaryAlgorithm.withPopulationSize 10
        |> EvolutionaryAlgorithm.withSurvivorCount 3
        |> EvolutionaryAlgorithm.start }

let update (msg: Msg) (state: State) : State * Cmd<Msg> =
    match msg with
    | ChangeFitness fitness -> { state with Fitness = fitness }, Cmd.none

    | SetFitness ->
        { state with
            EvolutionaryAlgorithm =
                EvolutionaryAlgorithm.setCurrentIndividualsFitness state.Fitness state.EvolutionaryAlgorithm },
        Cmd.none

let private fitnessSlider (state: State) (dispatch: Msg -> unit) =
    let slider =
        Slider.create [
            Slider.value state.Fitness
            Slider.minimum 0.
            Slider.maximum 1.
            Slider.width (Theme.size.medium - 50.)
            Slider.onValueChanged (ChangeFitness >> dispatch)
            Slider.dock Dock.Left
        ]

    let text =
        TextBlock.create [
            TextBlock.text $"%.2f{state.Fitness}"

            TextBlock.dock Dock.Right
            TextBlock.margin (Theme.spacing.small, 0.)
        ]

    Form.formElement
        {| Name = "Fitness"
           Orientation = Orientation.Vertical
           Element = DockPanel.create [ StackPanel.children [ slider; text ] ] |}

let private setFitnessButton (dispatch: Msg -> unit) =
    Form.iconTextButton
        (Icon.fitness Icon.Large Theme.palette.info)
        "Set Fitness"
        Theme.palette.foreground
        (fun _ -> SetFitness |> dispatch)
        SubPatchOptions.Always

let generation (gen: int) =
    Text.withIcon (Icon.generation Icon.Small Theme.palette.info) $"Generation: {gen}" Theme.palette.foreground

let sidePanel (state: State) (dispatch: Msg -> unit) =
    let header =
        Text.iconTitle
            (Icon.evolutionaryAlgorithm Icon.medium Theme.palette.primary)
            "Evolutionary Algorithm"
            Theme.palette.foreground

    StackPanel.create [
        StackPanel.minWidth Theme.size.medium
        StackPanel.children [
            header
            generation state.EvolutionaryAlgorithm.Generation
            fitnessSlider state dispatch
            setFitnessButton dispatch
        ]
    ]

let remainingIndividuals (state: State) (dispatch: Msg -> unit) =
    let selection =
        ListBox.create [
            ListBox.selectionMode SelectionMode.Single
            ListBox.dataItems (List.map (sprintf "%A") state.EvolutionaryAlgorithm.Remaining)
        ]

    Form.formElement
        {| Name = "Remaining"
           Orientation = Orientation.Vertical
           Element = selection |}



let currentIndividual (state: State) (dispatch: Msg -> unit) =
    let selection =
        ListBox.create [
            ListBox.selectionMode SelectionMode.Single
            ListBox.dataItems [
                $"%A{state.EvolutionaryAlgorithm.Current}"
            ]
        ]

    Form.formElement
        {| Name = "Current"
           Orientation = Orientation.Vertical
           Element = selection |}

let testedIndividuals (state: State) (dispatch: Msg -> unit) =
    let selection =
        ListBox.create [
            ListBox.selectionMode SelectionMode.Single
            ListBox.dataItems (List.map (sprintf "%A") state.EvolutionaryAlgorithm.Tested)
        ]


    Form.formElement
        {| Name = "Tested"
           Orientation = Orientation.Vertical
           Element = selection |}

let individualsPanel (state: State) (dispatch: Msg -> unit) =
    StackPanel.create [
        StackPanel.children [
            remainingIndividuals state dispatch
            currentIndividual state dispatch
            testedIndividuals state dispatch
        ]
    ]

let view (state: State) (dispatch: Msg -> unit) =
    DockPanel.create [
        DockPanel.children [
            DockPanel.child Dock.Left (sidePanel state dispatch)
            DockPanel.child Dock.Right (individualsPanel state dispatch)
        ]
    ]
