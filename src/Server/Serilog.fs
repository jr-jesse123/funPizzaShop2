module FunPizza.Server.Serilog

open Serilog
open Giraffe.SerilogExtensions
open Microsoft.AspNetCore.Http
open Serilog.Context
open System.Threading.Tasks
open System

//todo: separate contracts vs serilog implementation
let boostsTrapLogger () =
    let logger = LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.Console()
                    .WriteTo.File("logs/log_boot_strapper_json_.txt")
                    .CreateLogger()
                    //TODO: ADD SEQ
    Serilog.Log.Logger <- logger


let configure errHandler = 
    {
        SerilogConfig.defaults with 
           ErrorHandler = errHandler
           //RequestMessageTemplate = "{Method} Request at {Path}, User: {UserName}"
           //ResponseMessageTemplate =
           //    "{Method} Response (StatusCode {StatusCode}) at {Path} took {Duration} ms, User: {UserName}"
           //TODO: CHECK IF THESE NAMES LIKE {Method} are some kind of property or context and if this owrks with other names and values.
           //TODO: CHECK IF TEMPLATED MESSAGES ARE LIKE PROPERTIES IN NORMAL NON SERILOG LOGS
    }


type LogUserNameMiddleware(next: RequestDelegate) =
    member this.Invoke(context: HttpContext) : Task = //TODO: brincar com o tipo de retorno para async e terntar usar task CE
        use x = LogContext.PushProperty("UserName", context.User.Identity.Name) //TODO TRY TO REPLACE WITH MICROSOFT INTERFACE
        next.Invoke(context)

        //let userName = context.User.Identity.Name
        //let log = Serilog.Log.ForContext("UserName", userName)
        //log.Information("User {UserName} is requesting {Path}", userName, context.Request.Path)
        //next.Invoke(context)
    


open Serilog.Events
open Serilog.Filters
open System
open Serilog.Expressions
open Serilog.Formatting.Compact



let configureMiddleware _ (services:IServiceProvider) (loggerConfiguration:LoggerConfiguration)=
    //excludes some bots from logging
    let isBot = fun x -> String.IsNullOrWhiteSpace x || x = "AlwaysOn"

    loggerConfiguration
#if DEBUG
        .MinimumLevel.Debug()
#else
        .MinimumLevel.Information()
#endif
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("Giraffe", LogEventLevel.Warning)
        .Enrich.FromLogContext() //TODO: CHECK IF THIS IS NEEDED IN ORDER FOR LOGCONTEXT TO WORK
        .Filter.ByExcluding(Matching.WithProperty<string>("UserAgent", isBot))
        .Filter.ByExcluding("@m like '%/dist/%'")
        .Filter.ByExcluding("@m = 'Passing through logger HttpHandler'")
        .Filter.ByExcluding(
            "@m like 'GET Response%' and (@p['UserName'] is null) and @p['Path'] = '/' and @p['StatusCode'] = 200") 
            //TODO: FIND TYPED EQUIVALENT FILTERS
        .WriteTo.File(new CompactJsonFormatter(),"logs/log_json.txt" ,rollingInterval = RollingInterval.Day)
        .Destructure.FSharpTypes()
        .WriteTo.Console()
#if DEBUG
        .WriteTo.Seq("http://localhost:5341") //TODO: ADD SEQ USING DOCKER COMPSOSE
#endif
        

        
        

