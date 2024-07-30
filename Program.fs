module fast_search.Program

open System
open System.IO
open System.Threading.Tasks
open System.Collections.Concurrent
open System.Diagnostics

let maxDegreeOfParallelism = 10  // Максимальное количество параллельных задач
let memoryLimit = 1000000000L   // Ограничение по памяти в байтах (1 ГБ)

// Исключение для превышения лимита памяти
type MemoryLimitExceededException() = inherit Exception("Memory limit exceeded")

// Функция для форматирования размера в удобные единицы
let formatSize (size: int64) =
    let units = [| "bytes"; "KB"; "MB"; "GB"; "TB"; "PB"; "EB"; "ZB"; "YB" |]
    let mutable size = float size
    let mutable unitIndex = 0
    while size >= 1024.0 && unitIndex < units.Length - 1 do
        size <- size / 1024.0
        unitIndex <- unitIndex + 1
    $"%.2f{size} %s{units[unitIndex]}"

// Функция для поиска файлов в заданном каталоге и его подкаталогах
let searchFiles (rootDirectory: string) (fileMask: string) =
    let results = ConcurrentBag<string>()  // Потокобезопасная коллекция для хранения найденных файлов
    let totalSize = ref 0L  // Счетчик общего объема найденных файлов
    let totalFiles = ref 0  // Счетчик общего количества найденных файлов
    let maxDepth = ref 0    // Максимальная глубина каталогов

    let totalStorageSize = ref 0L  // Счетчик общего объема всех файлов
    let totalStorageFiles = ref 0  // Счетчик общего количества всех файлов
    let totalStorageDirs = ref 0   // Счетчик общего количества каталогов

    let rec search directory currentDepth =
        // Проверка текущего использования памяти и выброс исключения, если лимит превышен
        let memoryUsage = Process.GetCurrentProcess().PrivateMemorySize64
        if memoryUsage > memoryLimit then
            raise (MemoryLimitExceededException())

        try
            // Обновление максимальной глубины
            if currentDepth > maxDepth.Value then
                maxDepth.Value <- currentDepth

            // Добавление подходящих файлов в текущем каталоге в результаты
            let files = Directory.GetFiles(directory, fileMask)
            for file in files do
                results.Add(file)
                totalSize.Value <- totalSize.Value + FileInfo(file).Length
                totalFiles.Value <- totalFiles.Value + 1

            // Подсчет всех файлов в текущем каталоге
            let allFiles = Directory.GetFiles(directory)
            for file in allFiles do
                totalStorageSize.Value <- totalStorageSize.Value + FileInfo(file).Length
                totalStorageFiles.Value <- totalStorageFiles.Value + 1

            // Рекурсивный поиск в подкаталогах
            let directories = Directory.GetDirectories(directory)
            totalStorageDirs.Value <- totalStorageDirs.Value + directories.Length
            let tasks = directories |> Array.map (fun dir -> Task.Factory.StartNew(fun () -> search dir (currentDepth + 1)) :> Task)
            Task.WaitAll(tasks)
        with
        | :? UnauthorizedAccessException -> ()  // Игнорирование каталогов, к которым нет доступа
        | :? PathTooLongException -> ()         // Игнорирование каталогов с слишком длинными путями
        | :? MemoryLimitExceededException -> raise (MemoryLimitExceededException())  // Повторное выбрасывание исключения при превышении лимита памяти

    try
        // Запуск поиска в корневых каталогах
        let rootDirectories = Directory.GetDirectories(rootDirectory)
        totalStorageDirs.Value <- totalStorageDirs.Value + rootDirectories.Length
        let rootTasks = rootDirectories |> Array.map (fun dir -> Task.Factory.StartNew(fun () -> search dir 1))
        Task.WaitAll(rootTasks)
    with
    | :? MemoryLimitExceededException ->
        printfn "Memory limit exceeded during file search."

    results, totalSize.Value, totalFiles.Value, maxDepth.Value, totalStorageSize.Value, totalStorageFiles.Value, totalStorageDirs.Value  // Возвращение всех статистических данных

// Точка входа в программу
[<EntryPoint>]
let main argv =
    let rootDir = "/run/media/aleksei/STORAGE"  // Корневой каталог для начала поиска
    let mask = "upi*.json"           // Маска файлов для поиска

    // Замер времени выполнения
    let stopwatch = Stopwatch.StartNew()
    let foundFiles, totalSize, totalFiles, maxDepth, totalStorageSize, totalStorageFiles, totalStorageDirs = searchFiles rootDir mask
    stopwatch.Stop()

    // Печать найденных файлов
    for file in foundFiles do
        printfn $"%s{file}"

    // Печать статистики по найденным файлам
    printfn $"Total size of found files: {formatSize totalSize}"
    printfn $"Total number of found files: {totalFiles}"
    printfn $"Maximum directory depth: {maxDepth}"

    // Печать общей статистики по всем файлам и каталогам
    printfn $"Total storage size: {formatSize totalStorageSize}"
    printfn $"Total number of files: {totalStorageFiles}"
    printfn $"Total number of directories: {totalStorageDirs}"
    printfn $"Execution time: {stopwatch.Elapsed}"

    0  // Возврат 0 для указания успешного выполнения