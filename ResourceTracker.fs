module fast_search.ResourceTracker

open System.Diagnostics
open System.Windows.Forms
open Microsoft.FSharp.Core
open Microsoft.VisualBasic.Devices

module private data =
    let performanceCounterCpu = new PerformanceCounter("Processor", "% Processor Time", "_Total")
    let performanceCounterMem = new PerformanceCounter("Memory", "Available MBytes")
    
    let mutable cpuThreshold = 80.0;
    let mutable memThreshold = 1024.0
    
    let computerInfo = ComputerInfo()

open data
let startTracking(cpuUsageCallback: float -> unit)(memoryUsageCallback: float -> unit) (canselCallback: unit -> unit) =
    let timer = new Timer(Interval = 1000)
    timer.Tick.Add(fun _ ->
        let cpuUsage = performanceCounterCpu.NextValue() |> float
        let memoryAvailable = performanceCounterMem.NextValue() |> float
        let totalMemory = computerInfo.TotalPhysicalMemory / (1024UL * 1024UL) |> float
        let memoryUsage = (totalMemory - memoryAvailable) |> float

        cpuUsageCallback cpuUsage
        memoryUsageCallback memoryUsage

        if cpuUsage > cpuThreshold || memoryUsage < memThreshold then canselCallback()
)