module fast_search.MainWindow

open System
open System.Drawing
open System.Windows.Forms

module private form =
    // Создание формы
    let form = new Form(Text = "Поиск файлов", Size = Size(400, 200), FormBorderStyle = FormBorderStyle.FixedSingle, MaximizeBox = false)

    // Создание элементов управления
    let pathLabel = new Label(Text = "Путь к папке:", Location = Point(10, 20), AutoSize = true)
    let pathTextBox = new TextBox(Location = Point(120, 20), Width = 200)
    pathTextBox.Anchor <- AnchorStyles.Top ||| AnchorStyles.Left ||| AnchorStyles.Right

    let regexLabel = new Label(Text = "Регулярное выражение:", Location = Point(10, 60), AutoSize = true)
    let regexTextBox = new TextBox(Location = Point(160, 60), Width = 200)
    regexTextBox.Anchor <- AnchorStyles.Top ||| AnchorStyles.Left ||| AnchorStyles.Right

    let folderButton = new Button(Text = "Выбрать папку", Location = Point(330, 18))
    folderButton.Anchor <- AnchorStyles.Top ||| AnchorStyles.Right

    let searchButton = new Button(Text = "Начать поиск", Location = Point(150, 100), Width = 100)
    searchButton.Anchor <- AnchorStyles.Top ||| AnchorStyles.Left ||| AnchorStyles.Right

    let progressBar = new ProgressBar(Location = Point(10, 140), Width = 360)
    progressBar.Anchor <- AnchorStyles.Top ||| AnchorStyles.Left ||| AnchorStyles.Right
    progressBar.Style <- ProgressBarStyle.Marquee
    progressBar.MarqueeAnimationSpeed <- 30
    progressBar.Visible <- false

    // Добавление элементов на форму
    form.Controls.Add(pathLabel)
    form.Controls.Add(pathTextBox)
    form.Controls.Add(regexLabel)
    form.Controls.Add(regexTextBox)
    form.Controls.Add(folderButton)
    form.Controls.Add(searchButton)
    form.Controls.Add(progressBar)
    
    // Обработчик кнопки выбора папки
    folderButton.Click.Add(fun _ ->
        PathChooser.clearSelection()
        PathChooser.showFolderBrowser(form))
    
    let mutable onSearchButtonClickAction: string -> string -> unit = fun _ _ -> ()
    
    searchButton.Click.Add(fun _ -> onSearchButtonClickAction pathTextBox.Text regexTextBox.Text)
        
let setOnClickAction action = form.onSearchButtonClickAction <- action

// Функция для выключения бесконечной загрузки
let startInfiniteProgressBar() = form.progressBar.Visible <- true

// Функция для выключения бесконечной загрузки
let stopInfiniteProgressBar() =
    form.progressBar.Value <- 0
    form.progressBar.Visible <- false

let runForm() =
    Application.EnableVisualStyles()
    Application.Run(form.form)