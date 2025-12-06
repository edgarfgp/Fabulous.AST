namespace Playground

open Playground.Generated

module Main =

    [<EntryPoint>]
    let main _ =
        // Demonstrate using the generated types from JSON schema
        let address =
            { Address.street = "456 Oak Ave"
              city = "Portland"
              zipCode = "97201" }

        let item =
            { ItemsItem.sku = "WIDGET-001"
              quantity = 3
              price = 24.99 }

        let order =
            { OrdersItem.orderId = "ORD-123"
              total = 74.97
              items = [ item ] }

        let user =
            { User.id = 42
              name = "Bob"
              email = "bob@example.com"
              age = 28
              active = true
              roles = [ "developer"; "tester" ]
              address = address
              orders = [ order ] }

        printfn "Generated User type from JSON schema:"
        printfn $"  Name: %s{user.name}"
        printfn $"  Email: %s{user.email}"
        printfn $"  Age: %d{user.age}"
        printfn $"  Active: %b{user.active}"
        printfn $"  Roles: %A{user.roles}"
        printfn $"  Address: %s{user.address.street}, %s{user.address.city} %s{user.address.zipCode}"
        printfn $"  Orders: %d{user.orders.Length} order(s)"

        for order in user.orders do
            printfn $"    Order %s{order.orderId}: $%.2f{order.total} (%d{order.items.Length} items)"

        printfn ""
        printfn "Running original generator..."
        Generator.source $"{__SOURCE_DIRECTORY__}/output.fs" |> Async.RunSynchronously

        0
