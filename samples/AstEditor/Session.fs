namespace AstEditor

open System
open System.IO
open System.Text.Json

/// Persists the editor session — every document (name, DSL source, open or closed) and the
/// active one — so your edits and new files survive a restart.
module Session =

    // CLIMutable + public so System.Text.Json can construct these records (a `private` record's
    // constructors are internal, which System.Text.Json can't use).
    [<CLIMutable>]
    type SavedDoc =
        { Name: string
          Source: string
          IsOpen: bool }

    [<CLIMutable>]
    type SessionJson =
        { Active: int
          Docs: SavedDoc[] }

    // v2: the v1 file held tabs for features that no longer exist (Quick Fix / Convert demos).
    let private path =
        let dir =
            Path.Combine(Environment.GetFolderPath Environment.SpecialFolder.ApplicationData, "Fabulous.AST.AstEditor")

        Directory.CreateDirectory dir |> ignore
        Path.Combine(dir, "session-v2.json")

    /// The saved documents and the active index, or None if absent/corrupt/empty.
    let tryLoad() : (SavedDoc[] * int) option =
        try
            if File.Exists path then
                let data = JsonSerializer.Deserialize<SessionJson>(File.ReadAllText path)

                if isNull(box data.Docs) || data.Docs.Length = 0 then
                    None
                else
                    let docs = data.Docs

                    Some(docs, data.Active)
            else
                None
        with ex ->
            eprintfn "AstEditor: could not load the session — %s" ex.Message
            None

    let save (docs: SavedDoc[]) (active: int) =
        try
            let data =
                { Active = active
                  Docs = docs }

            File.WriteAllText(path, JsonSerializer.Serialize data)
        with _ ->
            ()
