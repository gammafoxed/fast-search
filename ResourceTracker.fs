module fast_search.ResourceTracker

open System.Diagnostics
open System.Windows.Forms

module private tracker =
    let performanceCounterCpu = new PerformanceCounter("Processor", "% Processor Time", "_Total")
    let performanceCounterMem = new PerformanceCounter("Memory", "Available MBytes")
    let timer = new Timer(Interval = 1000)
    timer.Tick.Add(fun _ ->
        let cpuUsage = performanceCounterCpu.NextValue()
        let memoryUsage = performanceCounterMem.NextValue()
        let cpuThreshold = float(cpuThresholdTextBox.Text)
        let memoryThreshold = float(memoryThresholdTextBox.Text)

        cpuUsageLabel.Text <- sprintf "CPU Usage: %.2f%%" cpuUsage
        memoryUsageLabel.Text <- sprintf "Memory Usage: %.2f MB" (float(SystemInfo.TotalPhysicalMemory - SystemInfo.AvailablePhysicalMemory) / (1024.0 * 1024.0))

        if cpuUsage > cpuThreshold || memoryUsage < memoryThreshold then
            cts.Cancel()
            MessageBox.Show("Resource usage exceeded the threshold. Search was canceled.") |> ignore
)