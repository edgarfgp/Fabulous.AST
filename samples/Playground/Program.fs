namespace Playground

open Playground.Generated.User
open Playground.Generated.Api

module Main =

    let demoUserSchema() =
        // Demonstrate using the generated types from user.json schema
        let address =
            { Address.street = "456 Oak Ave"
              street2 = "Suite 12"
              city = "Portland"
              zipCode = "97201" }

        let item: ItemsItem =
            { ItemsItem.sku = "WIDGET-001"
              quantity = 3
              price = 24.99 }

        let order: OrdersItem =
            { OrdersItem.orderId = "ORD-123"
              total = 74.97
              items = [ item ] }

        let user: User =
            { User.id = 42
              name = "Bob"
              email = "bob@example.com"
              age = 28
              active = true
              roles = [ "developer"; "tester" ]
              address = address
              orders = [ order ] }

        printfn "=== User Schema (user.json) ==="
        printfn $"  Name: %s{user.name}"
        printfn $"  Email: %s{user.email}"
        printfn $"  Age: %d{user.age}"
        printfn $"  Active: %b{user.active}"
        printfn $"  Roles: %A{user.roles}"
        printfn $"  Address: %s{user.address.street}, %s{user.address.city} %s{user.address.zipCode}"
        printfn $"  Orders: %d{user.orders.Length} order(s)"

        for order in user.orders do
            printfn $"    Order %s{order.orderId}: $%.2f{order.total} (%d{order.items.Length} items)"

    let demoComplexApiSchema() =
        let coordinates =
            { Coordinates.latitude = 37.7749
              longitude = -122.4194 }

        let apiAddress =
            { Address.line1 = "123 Main Street"
              line2 = "Apt 4B"
              city = "San Francisco"
              state = "CA"
              postalCode = "94102"
              country = "US"
              coordinates = coordinates }

        let profile =
            { Profile.firstName = "Alice"
              lastName = "Wonder"
              displayName = "Alice W."
              bio = "Software engineer"
              avatarUrl = "https://example.com/avatar.png"
              coverImageUrl = null
              dateOfBirth = "1990-05-15"
              timezone = "America/Los_Angeles" }

        let notifications =
            { Notifications.email = true
              push = true
              sms = false
              frequency = "daily" }

        let privacy =
            { Privacy.profileVisibility = "public"
              showEmail = false
              showActivity = true
              allowTagging = true }

        let settings =
            { Settings.theme = "dark"
              language = "en-US"
              notifications = notifications
              privacy = privacy
              _2faEnabled = true } // Leading digit field!

        let stats =
            { Stats.followers = 15420
              following = 342
              posts = 891
              likes = 45230
              reputation = 98.75 // Float field
              level = 42 }

        let permissions =
            { Permissions.canPost = true
              canComment = true
              canModerate = true
              canAccessAdmin = false
              maxUploadSizeMb = 100 }

        let subscription =
            { Subscription.plan = "premium"
              status = "active"
              startDate = "2024-01-01"
              renewalDate = "2025-01-01"
              price = 9.99
              currency = "USD"
              features = [ "ad-free"; "priority-support" ] }

        let recentActivity =
            { RecentActivityItem.``type`` = "post" // Reserved keyword field!
              action = "created"
              targetId = "post-12345"
              timestamp = "2025-01-14T18:30:00Z"
              metadata =
                { Metadata.serverRegion = "us-west-2"
                  responseTimeMs = 45.2
                  cacheHit = false
                  deprecationWarnings = null
                  experimentalFeatures = [] } }

        let connectedAccount =
            { ConnectedAccountsItem.provider = "github"
              providerId = "gh-123456"
              connectedAt = "2024-06-15T10:00:00Z"
              lastSync = "2025-01-15T08:00:00Z" }

        let apiUser =
            { UsersItem.id = 9223372036854775807L // int64 field!
              username = "alice_wonder"
              email = "alice@example.com"
              profile = profile
              settings = settings
              stats = stats
              roles = [ "user"; "moderator" ]
              permissions = permissions
              subscription = subscription
              recentActivity = [ recentActivity ]
              connectedAccounts = [ connectedAccount ]
              address = apiAddress
              createdAt = "2023-06-15T08:00:00Z"
              updatedAt = "2025-01-15T10:29:00Z"
              lastLoginAt = "2025-01-15T10:00:00Z"
              isVerified = true
              isActive = true
              deletedAt = None } // Optional field!

        let pagination =
            { Pagination.page = 1
              pageSize = 25
              totalItems = 1542
              totalPages = 62
              hasNextPage = true
              hasPreviousPage = false }

        let summary =
            { Summary.totalUsers = 1542
              activeUsers = 1398
              newUsersToday = 23
              premiumUsers = 456
              averageSessionMinutes = 34.5 }

        let warning =
            { WarningsItem.code = "RATE_LIMIT_WARNING"
              message = "You are approaching the rate limit"
              details =
                { Details.currentUsage = 850
                  limit = 1000
                  resetAt = "2025-01-15T11:00:00Z" } }

        let links =
            { Links.self = "https://api.example.com/v2/users?page=1"
              next = "https://api.example.com/v2/users?page=2"
              prev = null
              first = "https://api.example.com/v2/users?page=1"
              last = "https://api.example.com/v2/users?page=62" }

        let apiResponse =
            { ApiResponse.apiVersion = "v2.1.0"
              requestId = "550e8400-e29b-41d4-a716-446655440000"
              timestamp = "2025-01-15T10:30:00Z"
              success = true
              pagination = pagination
              metadata =
                { Metadata.serverRegion = "us-west-2"
                  responseTimeMs = 145.67
                  cacheHit = false
                  deprecationWarnings = null
                  experimentalFeatures = [ "beta-search"; "ai-recommendations" ] }
              data =
                { Data.users = [ apiUser ]
                  summary = summary }
              errors = []
              warnings = [ warning ]
              links = links }

        printfn ""
        printfn "=== Complex API Schema (complex-api.json) ==="
        printfn $"  API Version: %s{apiResponse.apiVersion}"
        printfn $"  Request ID: %s{apiResponse.requestId}"
        printfn $"  Success: %b{apiResponse.success}"
        printfn $"  Response Time: %.2f{apiResponse.metadata.responseTimeMs}ms"
        printfn ""
        printfn "  Pagination:"
        printfn $"    Page %d{apiResponse.pagination.page} of %d{apiResponse.pagination.totalPages}"
        printfn $"    Total Items: %d{apiResponse.pagination.totalItems}"
        printfn ""
        printfn "  User (demonstrating special field handling):"
        printfn $"    ID (int64): %d{apiUser.id}"
        printfn $"    Username: %s{apiUser.username}"
        printfn $"    2FA Enabled (_2faEnabled): %b{apiUser.settings._2faEnabled}"
        printfn $"    Activity Type (``type``): %s{recentActivity.``type``}"
        printfn $"    Reputation (float): %.2f{apiUser.stats.reputation}"
        printfn $"    Deleted At (option): %A{apiUser.deletedAt}"
        printfn ""
        printfn $"  Warnings: %d{apiResponse.warnings.Length}"

        for w in apiResponse.warnings do
            printfn $"    [%s{w.code}] %s{w.message}"

    [<EntryPoint>]
    let main _ =
        demoUserSchema()
        demoComplexApiSchema()

        printfn ""
        printfn "Running original generator..."
        Generator.source $"{__SOURCE_DIRECTORY__}/output.fs" |> Async.RunSynchronously

        0
