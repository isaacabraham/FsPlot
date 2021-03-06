﻿module FsPlot.Google.Js

open FsPlot.Config
open FsPlot.Data
open FsPlot.Highcharts.Options
open FsPlot.Quote
open FunScript
open FunScript.Compiler
open FunScript.TypeScript
open System
open FsPlot.Google.Options

[<JSEmitInline("google.visualization.data.join({0}, {1}, 'full', [[0,0]], [1], [1])")>]
let join dt1 dt2 : google.visualization.DataTable = failwith ""

[<JSEmitInline "{ showTip: true }">]
let mapChartOptions() : obj = failwith ""

[<JSEmitInline "new google.visualization.Map(document.getElementById('chart'))">]
let mapChart() : obj = failwith ""

[<JSEmitInline "{0}.draw({1}, {2})">]
let drawMap (map:obj) (data:google.visualization.DataTable) (options:obj) : unit = failwith ""

[<ReflectedDefinition>]
module Chart =
    
    type pack = {packages : string []}

    let _type typeCode =
        match typeCode with
        | TypeCode.Boolean -> "boolean"
        | TypeCode.DateTime -> "datetime"
        | TypeCode.String -> "string"
        | _ -> "number"

    let dataTables (config:ChartConfig) =
        config.Data
        |> Array.mapi(fun idx series ->
            let dataTable = google.visualization.DataTable.Create()  
            let xType = series.XType      
            dataTable.addColumn(_type xType) |> ignore
            dataTable.addColumn(_type series.YType, series.Name) |> ignore
            // TODO: find better way to handle dates
            let values =
                match xType with
                | TypeCode.DateTime ->
                    series.Values
                    |> Array.map(fun arr ->
                        box [|
                            (arr :?> obj []).[0]
                            :?> float
                            |> Globals.Date.Create
                            |> box
                            (arr :?> obj []).[1]
                        |]
                    )
                | _ -> series.Values
            dataTable.addRows values |> ignore
            dataTable
        )
        |> Array.toList

    let rec joinDataTables dts =
        match dts with
        | [dt] -> dt
        | [dt1; dt2] -> join dt1 dt2
        | x ->
            dts
            |> Seq.skip 2
            |> Seq.toList
            |> List.append [join x.[0] x.[1]]
            |> joinDataTables

    let drawOnLoad (drawChart:unit -> unit) =
        google.Globals.load("visualization", "1", {packages = [|"corechart"|]})
        google.Globals.setOnLoadCallback drawChart

    let barChartOptions config =
        let options = createEmpty<google.visualization.BarChartOptions>()
            
        match config.Title with
        | None -> ()
        | Some x -> options.title <- x

        match config.XTitle with
        | None -> ()
        | Some x ->
            let xAxis = createEmpty<google.visualization.ChartAxis>()
            xAxis.title <- x
            options.hAxis <- xAxis 

        match config.YTitle with
        | None -> ()
        | Some x ->
            let yAxis = createEmpty<google.visualization.ChartAxis>()
            yAxis.title <- x
            
            options.vAxis <- yAxis 

        let legend = createEmpty<google.visualization.ChartLegend>()
        match config.Legend with
        | false -> legend.position <- "none"
        | true -> legend.position <- "bottom"
        options.legend <- legend

        options

    let columnChartOptions config =
        let options = createEmpty<google.visualization.ColumnChartOptions>()
        
        match config.Title with
        | None -> ()
        | Some x -> options.title <- x

        match config.XTitle with
        | None -> ()
        | Some x ->
            let xAxis = createEmpty<google.visualization.ChartAxis>()
            xAxis.title <- x
            options.hAxis <- xAxis 

        match config.YTitle with
        | None -> ()
        | Some x ->
            let yAxis = createEmpty<google.visualization.ChartAxis>()
            yAxis.title <- x
            options.vAxis <- yAxis 

        let legend = createEmpty<google.visualization.ChartLegend>()
        match config.Legend with
        | false -> legend.position <- "none"
        | true -> legend.position <- "bottom"
        options.legend <- legend

        options

    let lineChartOptions config =
        let options = createEmpty<google.visualization.LineChartOptions>()
            
        match config.Title with
        | None -> ()
        | Some x -> options.title <- x

        match config.XTitle with
        | None -> ()
        | Some x ->
            let xAxis = createEmpty<google.visualization.ChartAxis>()
            xAxis.title <- x
            options.hAxis <- xAxis 

        match config.YTitle with
        | None -> ()
        | Some x ->
            let yAxis = createEmpty<google.visualization.ChartAxis>()
            yAxis.title <- x
            options.vAxis <- yAxis 

        let legend = createEmpty<google.visualization.ChartLegend>()
        match config.Legend with
        | false -> legend.position <- "none"
        | true -> legend.position <- "bottom"
        options.legend <- legend

        options

    let bar (config:ChartConfig) =
        let drawChart() =
            let options = barChartOptions config

            let data =
                dataTables config
                |> joinDataTables

            let chart = google.visualization.BarChart.Create(Globals.document.getElementById("chart"))
            chart.draw(data, options)

        drawOnLoad drawChart

    let column (config:ChartConfig) =
        let drawChart() =
            let options = columnChartOptions config

            let data =
                dataTables config
                |> joinDataTables

            let chart = google.visualization.ColumnChart.Create(Globals.document.getElementById("chart"))
            chart.draw(data, options)

        drawOnLoad drawChart

    let geo config region mode sizeAxis =
        let drawChart() =
            let options = createEmpty<google.visualization.GeoChartOptions>()

            match region with
            | None -> ()
            | Some x -> options.region <- x

            match mode with
            | None -> ()
            | Some x -> options.displayMode <- x

            match sizeAxis with
            | None -> ()
            | Some x ->
                let geoChartAxis = createEmpty<google.visualization.GeoChartAxis>()
                geoChartAxis.minValue <- x.MinValue
                geoChartAxis.maxValue <- x.MaxValue
                options.sizeAxis <- geoChartAxis

            let data =
                dataTables config
                |> joinDataTables
            
            let chart = google.visualization.GeoChart.Create(Globals.document.getElementById("chart"))
            chart.draw(data, options)

        google.Globals.load("visualization", "1", {packages = [|"geochart"|]})
        google.Globals.setOnLoadCallback drawChart

    let line (config:ChartConfig) =
        let drawChart() =
            let options = lineChartOptions config

            let data =
                dataTables config
                |> joinDataTables

            let chart = google.visualization.LineChart.Create(Globals.document.getElementById("chart"))
            chart.draw(data, options)

        drawOnLoad drawChart

    let spline (config:ChartConfig) =
        let drawChart() =
            let options = lineChartOptions config

            options.curveType <- "function"

            let data =
                dataTables config
                |> joinDataTables

            let chart = google.visualization.LineChart.Create(Globals.document.getElementById("chart"))
            chart.draw(data, options)

        drawOnLoad drawChart

    let stackedBar (config:ChartConfig) =
        let drawChart() =
            let options = barChartOptions config

            options.isStacked <- true

            let data =
                dataTables config
                |> joinDataTables

            let chart = google.visualization.BarChart.Create(Globals.document.getElementById("chart"))
            chart.draw(data, options)

        drawOnLoad drawChart

    let stackedColumn (config:ChartConfig) =
        let drawChart() =
            let options = columnChartOptions config

            options.isStacked <- true

            let data =
                dataTables config
                |> joinDataTables

            let chart = google.visualization.ColumnChart.Create(Globals.document.getElementById("chart"))
            chart.draw(data, options)

        drawOnLoad drawChart

    let map (config:ChartConfig) =
        let drawChart() =
            let options =  mapChartOptions()

            let data =
                dataTables config
                |> joinDataTables

            let map = mapChart()
            
            drawMap map data options

        google.Globals.load("visualization", "1", {packages = [|"map"|]})
        google.Globals.setOnLoadCallback drawChart
    
let inline compile expr =
    Compiler.Compile(
        expr,
        noReturn = true,
        shouldCompress = true)

let bar config =
    let configExpr = quoteChartConfig config
    compile <@ Chart.bar %%configExpr @>

let column config =
    let configExpr = quoteChartConfig config
    compile <@ Chart.column %%configExpr @>

let geo config region mode sizeAxis =
    let configExpr = quoteChartConfig config
    let regionExpr = quoteStrOption region
    let modeExpr = quoteStrOption mode
    let sizeAxisExpr = quoteSizeAxis sizeAxis
    compile <@ Chart.geo %%configExpr %%regionExpr %%modeExpr %%sizeAxisExpr @>

let line config =
    let configExpr = quoteChartConfig config
    compile <@ Chart.line %%configExpr @>

let spline config =
    let configExpr = quoteChartConfig config
    compile <@ Chart.spline %%configExpr @>

let stackedBar config =
    let configExpr = quoteChartConfig config
    compile <@ Chart.stackedBar %%configExpr @>

let stackedColumn config =
    let configExpr = quoteChartConfig config
    compile <@ Chart.stackedColumn %%configExpr @>

let map config =
    let configExpr = quoteChartConfig config
    compile <@ Chart.map %%configExpr @>
