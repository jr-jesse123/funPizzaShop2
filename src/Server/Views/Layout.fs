﻿module FunPizza.Server.Views.Layout
open System.IO
open Microsoft.AspNetCore.Http
open System.Threading.Tasks
open FunPizzaShop.Server.Views.Common

let scriptFiles = 
    let assetsDir = "WebRoot/dist/assets" //TODO: fazer configurável


    if Directory.Exists assetsDir then
        Directory.GetFiles(assetsDir, "*.js", SearchOption.AllDirectories)
    else
        [||]

let paths = 
    scriptFiles
    |> Array.map (fun x -> x.Substring(x.LastIndexOf(Path.DirectorySeparatorChar) + 1))


let view (ctx:HttpContext) (env:#_) (isDev) (body: int -> Task<string>) = task{
    let script = 
        if isDev || paths.Length = 0 then                           //TODO: fazer configurável, checar de onde vem cada valor., checar se todos os scripts foram carregados
            html """
            <script type="module" src="/dist/@vite/client"></script>
            <script type="module" src="/dist/build/App.js"></script>
            <script defer src="/_framework/aspnetcore-browser-refresh.js"></script>
            """
        else
            let createScriptTag path = 
                html $"""
                    
                    <script type="module" src="/dist/assets/{path}" ></script>
                    """


            paths |> Array.map createScriptTag |> String.concat "\n"

    let! body = body 0 //TODO: testar a ordem dos headers h1.. h2.. 

            //TODO: PESQUISAR CAD UMA DAS TAGAS ABAIXO, bem como desativalas para verificar o efeito
    return
        html $"""
        <!DOCTYPE html>
        <html theme=default-pizza lang="en">
        <head>
            <meta charset="utf-8" >
            <base href="/" />
            <title>Fun Pizza Shop </title>
            <meta name="description"
                content="Best Pizza in the Town" />
            <meta name="keywords" content="Order Pizza">
            <link rel="apple-touch-icon" href="/assets/icons/icon-512.png">
            <!-- This meta viewport ensures the webpage's dimensions change according to the device it's on. This is called Responsive Web Design.-->
            <meta name="viewport"
                content="viewport-fit=cover, width=device-width, initial-scale=1.0" />
            <meta name="theme-color"  content="#181818" />
            <!-- These meta tags are Apple-specific, and set the web application to run in full-screen mode with a black status bar. Learn more at https://developer.apple.com/library/archive/documentation/AppleApplications/Reference/SafariHTMLRef/Articles/MetaTags.html-->
            <meta name="apple-mobile-web-app-capable" content="yes" />
            <meta name="apple-mobile-web-app-title" content="Fun Pizza Shop" />
            <meta name="apple-mobile-web-app-status-bar-style" content="black" />
            <!-- Imports an icon to represent the document. -->
            <link rel="icon" href="/assets/icons/icon-512.svg" type="image/x-icon" />
            <!-- Imports the manifest to represent the web application. A web app must have a manifest to be a PWA. -->
            <link rel="manifest" href="/manifest.webmanifest" />
            <link rel="stylesheet" href="/css/index.css?v=202307101701"/>
            <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css"
         integrity="sha256-p4NxAoJBhIIN+hmNHrzRCf9tD/miZyoHS5obTRR9BMY="
         crossorigin=""/>
        
            <script defer crossorigin="anonymous" type="text/javascript" 
            src="https://cdnjs.cloudflare.com/ajax/libs/dompurify/3.0.1/purify.min.js"></script>
            <script defer src="/index.js"></script>
            <script defer src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"
         integrity="sha256-20nQCchB9co0qIjJZRGuk2/Z9VM+kNiyxNV1lvTlZBo="
         crossorigin=""></script>
            {script}
        </head>
       
        <body>
            <header>
           
            </header>
            <main>
                {body}
            </main>
        </body>
        </html>"""
}
