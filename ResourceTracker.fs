module fast_search.ResourceTracker

open System.Diagnostics
open System.Windows.Forms
open Microsoft.FSharp.Core
open Microsoft.VisualBasic.Devices

module private data =
    let performanceCounterCpu = new PerformanceCounter("Processor", "% Processor Time", "_Total")
    let performanceCounterMem = new PerformanceCounter("Memory", "Available MBytes")
    
    let mutable cpuThreshold = 80f;
    let mutable memThreshold = 1024f
    
    let computerInfo = new ComputerInfo()

open data
let startTracking(cpuUsageCallback: string -> unit)(memoryUsageCallback: string -> unit) =
    let timer = new Timer(Interval = 1000)
    let cts = new System.Threading.CancellationTokenSource()
    timer.Tick.Add(fun _ ->
        let cpuUsage = performanceCounterCpu.NextValue()
        let memoryAvailable = performanceCounterMem.NextValue() |> float
        let totalMemory = computerInfo.TotalPhysicalMemory / (1024UL * 1024UL) |> float
        let memoryUsage = totalMemory - memoryAvailable

        sprintf "CPU Usage: %.2f%%" cpuUsage |> cpuUsageCallback
        $"Memory Usage: %.2f{uint(ComputerInfo.TotalPhysicalMemory - ComputerInfo.AvailablePhysicalMemory) / (1024.0 * 1024.0)} MB" |> memoryUsageCallback

        if cpuUsage > cpuThreshold || memoryUsage < memoryThreshold then
            cts.Cancel()
            MessageBox.Show("Resource usage exceeded the threshold. Search was canceled.") |> ignore
)