namespace AstEditor

open System
open System.IO
open System.Text.Json

/// Persists the editor session — the per-tab DSL sources and the active tab — so your edits
/// survive a restart (the dock *arrangement* is persisted separately by LayoutStore).
module Session =

    let private path =
        let dir =
            Path.Combine(Environment.GetFolderPath Environment.SpecialFolder.ApplicationData, "Fabulous.AST.AstEditor")

        Directory.CreateDirectory dir |> ignore
        Path.Combine(dir, "session.json")

    // Encoded as a plain string[] — element 0 is the active tab index, the rest are the
    // sources. System.Text.Json round-trips string[] cleanly with no F#/converter fuss.
    let tryLoad() : (string[] * int) option =
        try
            if File.Exists path then
                let arr = JsonSerializer.Deserialize<string[]>(File.ReadAllText path)

                match arr with
                | null -> None
                | _ when arr.Length >= 1 ->
                    match Int32.TryParse arr[0] with
                    | true, active -> Some(arr[1..], active)
                    | _ -> None
                | _ -> None
            else
                None
        with _ ->
            None

    let save (sources: string[]) (activeTab: int) =
        try
            File.WriteAllText(path, JsonSerializer.Serialize(Array.append [| string activeTab |] sources))
        with _ ->
            ()
