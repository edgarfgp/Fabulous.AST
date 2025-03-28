namespace Playground

module Common =
    open System.IO

    let writeIfChanged outputPath text =
        async {
            let writeToFile (path: string) (contents: string) = File.WriteAllTextAsync(path, contents)

            let! existingFile =
                async {
                    if File.Exists(outputPath) then
                        let! file = File.ReadAllTextAsync(outputPath) |> Async.AwaitTask

                        return Some file
                    else
                        return None
                }

            printfn $"Writing to %s{outputPath}"

            match existingFile with
            | Some existingFile when existingFile = text -> printfn "No changes"
            | _ -> do! text |> writeToFile outputPath |> Async.AwaitTask
        }
