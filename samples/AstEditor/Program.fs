namespace AstEditor

open System
open Avalonia

module Program =

    [<CompiledName "BuildAvaloniaApp">]
    let buildAvaloniaApp() =
        App.create().UsePlatformDetect().LogToTrace(areas = Array.empty)

    [<EntryPoint; STAThread>]
    let main argv =
        // ASTEDITOR_TRACE=1 also logs every exception as it is thrown (even ones a framework
        // later swallows) — the quickest way to find out why the UI stopped updating.
        if not(String.IsNullOrEmpty(Environment.GetEnvironmentVariable "ASTEDITOR_TRACE")) then
            AppDomain.CurrentDomain.FirstChanceException.Add(fun e ->
                eprintfn
                    "first-chance: %s: %s\n%s"
                    (e.Exception.GetType().Name)
                    e.Exception.Message
                    e.Exception.StackTrace)

        buildAvaloniaApp().StartWithClassicDesktopLifetime(argv)
