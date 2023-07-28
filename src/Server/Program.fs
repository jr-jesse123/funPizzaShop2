open Microsoft.Extensions.DependencyInjection
open Giraffe
open Giraffe.SerilogExtensions
//open Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer
//open Microsoft.AspNetCore.app
open Microsoft.AspNetCore.Cors
open Microsoft.AspNetCore.Http
open Serilog
open FunPizzaShop.Server.Handlers.Default
open System.Globalization
open Microsoft.Extensions.Hosting

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Giraffe.SerilogExtensions

open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open Serilog
open Hocon.Extensions.Configuration
open ThrottlingTroll
open FunPizzaShop.Server.Views
open FunPizzaShop.Server.Handlers.Default
open FunPizzaShop.Server

open Http

open System.Globalization


let webApp : HttpHandler = 
    choose [
        route "/ping"   >=> text "pong"
    ]


//TODO: CHECK THIS LINE
CultureInfo.DefaultThreadCurrentCulture <- CultureInfo.InvariantCulture

FunPizza.Server.Serilog.bootsTrapLogger()

type Self = Self

open Microsoft.AspNetCore.Cors.Infrastructure
open FunPizza.Server.Views
open FunPizza.Server
open System
open Microsoft.AspNetCore.Builder
open FunPizzaShop.Server

let errorHandler (ex: Exception) (ctx:HttpContext) = 
    Log.Error(ex, "Error processing request {Path}", ctx.Request.Path)
    match ex with
    | :? System.Text.Json.JsonException -> clearResponse >=> setStatusCode 400 >=> text ex.Message
    | _ -> 
        clearResponse 
            >=> setStatusCode 500 
            >=> 
#if DEBUG 
                    text ex.Message
#else
                    text "Internal Server Error"
#endif


let configureCors (builder: CorsPolicyBuilder) = 
#if DEBUG
    builder
        .WithOrigins("http://localhost:5010", "https://localhost:5011") //TODO: WHY IS THIS ALLOWED ONLY IN DEVELOPMENT?
        .AllowAnyMethod()
        .AllowAnyHeader()
    |> ignore
#else 
    ()
#endif

let configureApp (app: IApplicationBuilder, appenv) = 
    let env = app.ApplicationServices.GetService<IWebHostEnvironment>()
    let isDevelopment = env.IsDevelopment()
    let app = if isDevelopment then app else app.UseResponseCompression()

    app
        .UseDefaultFiles() //TODO: CHECK
        .UseAuthentication()
        .UseAuthorization()
        .UseMiddleware<Serilog.LogUserNameMiddleware>() //TODO: PENSAR EM MUDAR PARA UM HTTPHANDLER OU ENTENDER PQ � MELHOR SER MIDDLEWARE
        .Use(Http.headerMiddleware) //TODO: ENTENDER PQ ESSE AQUI � MELHOR EM FUN��O DO QUE EM CLASSE    
        |> ignore

    
    let layout ctx = Layout.view ctx appenv (env.IsDevelopment())

    let webApp = webAppWrapper appenv layout //TODO: ENTENDER O WRAPPER

    let sConfig = Serilog.configure errorHandler //TODO: brincar com as configura��es

    let handler = SerilogAdapter.Enable(webApp, sConfig)// TODO: entender esse adaptador.


    if isDevelopment then
        app.UseDeveloperExceptionPage() |> ignore
    else
        app
            .UseCors(configureCors)
            .UseStaticFiles(Http.staticFileOptions)
                //  .UseThrottlingTroll(Throttling.setOptions)
            .UseWebSockets() 
            .UseGiraffe(handler)

            
let configureServices (services:IServiceCollection) = 
    services
        .AddAuthorization()
        //.AddResponseCompression(fun options -> options.EnableForHttps <- true) //TODO: WHY WE'RE GOING THROUGH RESPONSE COMPRESSIONS OVER HTTPS
        .AddCors()
        .AddGiraffe()
        .AddAntiforgery() //TODO: CHECK THIS
        //.AddApplicationInsightsTelemetry()
        .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie( //TODO: CHANGE THIS TO JWT
            CookieAuthenticationDefaults.AuthenticationScheme,
            fun options ->
                options.SlidingExpiration <- true
                options.ExpireTimeSpan <- TimeSpan.FromDays(7)
        )
    |> ignore

        

let configureLoggin (builder:ILoggingBuilder) =
    builder.AddConsole().AddDebug() |> ignore //TODO: ISSO N�O TA REPETIDO?



let host appEnv args = 
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot = Path.Combine(contentRoot, "WebRoot") //TODO: fazer configur�vel com valida��o
    let host = 
        Host.CreateDefaultBuilder(args)
            .UseSerilog(Serilog.configureMiddleware)
            .ConfigureWebHostDefaults(fun webHostBuilder -> 
                webHostBuilder
#if !DEBUG           
                    .UseEnvironment(Environments.Production)
#else                    
                    .UseEnvironment(Environments.Production)
#endif
                    .UseContentRoot(contentRoot)                    
                    .UseWebRoot(webRoot)
                    .Configure(fun builder -> configureApp(builder, appEnv))
                    .ConfigureServices(configureServices)
                    .ConfigureLogging(configureLoggin)
                    |> ignore
                )
    ()


[<EntryPoint>]
let main args =
    let configBuilder = 
        ConfigurationBuilder()
            .AddUserSecrets<Self>() //TODO: what is this self type for?
            .AddHoconFile("config.hocon") //TODO: wtf is a hocon file?
            .AddEnvironmentVariables() 

    let config = configBuilder.Build()




    // let builder = WebApplication.CreateBuilder(args)
    
    // builder.Services.AddGiraffe() |> ignore
    
    // let app = builder.Build()
    
    // app.UseGiraffe () |> ignore

    // app.MapGet("/", Func<string>(fun () -> "Hello World!")) |> ignore

    // app.Run()

    0 // Exit code

