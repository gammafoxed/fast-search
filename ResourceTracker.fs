module fast_search.ResourceTracker

open System.Diagnostics
open System.Windows.Forms

module private data =
    let performanceCounterCpu = new PerformanceCounter("Processor", "% Processor Time", "_Total")
    let performanceCounterMem = new PerformanceCounter("Memory", "Available MBytes")
    
    let mutable cpuThreshold = 80f;
    let mutable memThreshold = 1024f;

open data
let startTracking(cpuUsageCallback: string -> unit)(memoryUsageCallback: string -> unit) =
    let timer = new Timer(Interval = 1000)
    timer.Tick.Add(fun _ ->
        let cpuUsage = performanceCounterCpu.NextValue()
        let memoryUsage = performanceCounterMem.NextValue()
        let cpuThreshold = cpuThreshold
        let memoryThreshold = memThreshold

        sprintf "CPU Usage: %.2f%%" cpuUsage |> cpuUsageCallback
        $"Memory Usage: %.2f{float(SystemInfo.TotalPhysicalMemory - SystemInfo.AvailablePhysicalMemory) / (1024.0 * 1024.0)} MB" |> memoryUsageCallback

        if cpuUsage > cpuThreshold || memoryUsage < memoryThreshold then
            cts.Cancel()
            MessageBox.Show("Resource usage exceeded the threshold. Search was canceled.") |> ignore
)