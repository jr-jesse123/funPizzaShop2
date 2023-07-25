open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Giraffe
open Microsoft.AspNetCore.Hosting

let webApp =
    choose [
        route "/ping"   >=> text "pong"
    ]


[<EntryPoint>]
let main args =

    let builder = WebApplication.CreateBuilder(args)
    
    builder.Services.AddGiraffe() |> ignore
    

    let app = builder.Build()
    
    app.UseGiraffe (webApp) |> ignore

    app.MapGet("/", Func<string>(fun () -> "Hello World!")) |> ignore

    app.Run()

    0 // Exit code

