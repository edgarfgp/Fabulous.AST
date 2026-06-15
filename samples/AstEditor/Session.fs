namespace AstEditor

open System
open System.IO
open System.Text.Json

/// Persists the editor session — every tab (name + DSL source) and the active tab — so your
/// edits *and* any scratch pads you added survive a restart. Keyed by tab name so adding,
/// removing or reordering the built-in samples in code doesn't scramble your saved edits.
module Session =

    // CLIMutable so System.Text.Json can round-trip these F# records without a custom converter.
    [<CLIMutable>]
    type private TabJson =
        { Name: string
          Source: string }

    [<CLIMutable>]
    type private SessionJson =
        { ActiveTab: int
          Tabs: TabJson[] }

    let private path =
        let dir =
            Path.Combine(Environment.GetFolderPath Environment.SpecialFolder.ApplicationData, "Fabulous.AST.AstEditor")

        Directory.CreateDirectory dir |> ignore
        Path.Combine(dir, "session.json")

    /// The saved tabs (name, source) and the active-tab index, or None if absent/corrupt.
    let tryLoad() : ((string * string)[] * int) option =
        try
            if File.Exists path then
                let data = JsonSerializer.Deserialize<SessionJson>(File.ReadAllText path)

                if isNull(box data.Tabs) then
                    None
                else
                    Some(data.Tabs |> Array.map(fun t -> t.Name, t.Source), data.ActiveTab)
            else
                None
        with _ ->
            None

    let save (tabs: (string * string)[]) (activeTab: int) =
        try
            let data =
                { ActiveTab = activeTab
                  Tabs =
                    tabs
                    |> Array.map(fun (name, source) ->
                        { Name = name
                          Source = source }) }

            File.WriteAllText(path, JsonSerializer.Serialize data)
        with _ ->
            ()
