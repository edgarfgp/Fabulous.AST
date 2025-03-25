namespace Playground

module Main =

    [<EntryPoint>]
    let main _ =
        Generator.source $"{__SOURCE_DIRECTORY__}/output.fs" |> Async.RunSynchronously

        0 // return an integer exit code
