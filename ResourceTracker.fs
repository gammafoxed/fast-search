module fast_search.ResourceTracker

open System.Diagnostics
open System.Threading
open Microsoft.FSharp.Core
open Microsoft.VisualBasic.Devices

module private data =
    let performanceCounterCpu =
    new PerformanceCounter("Processor", "% Processor Time", "_Total")
    let performanceCounterMem = new PerformanceCounter("Memory", "Available MBytes")

    let mutable cpuThreshold = 80.0
    let mutable memThreshold = 1024.0

    let computerInfo = ComputerInfo()

open data

let startTrackingAsync
    (cpuUsageCallback: float -> unit)
    (memoryUsageCallback: float -> unit)
    (canselCallback: unit -> unit)
    (ct: CancellationToken)
    =
    async {
        while not ct.IsCancellationRequested do
            let cpuUsage = performanceCounterCpu.NextValue() |> float
            let memoryAvailable = performanceCounterMem.NextValue() |> float
            let totalMemory = computerInfo.TotalPhysicalMemory / (1024UL * 1024UL) |> float
            let memoryUsage = (totalMemory - memoryAvailable) |> float

            cpuUsageCallback cpuUsage
            memoryUsageCallback memoryUsage

            if cpuUsage > cpuThreshold || memoryUsage < memThreshold then
                canselCallback ()

            do! Async.Sleep(1000)
    }
