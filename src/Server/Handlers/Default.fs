module FunPizzaShop.Server.Handlers.Default
open System.Threading.Tasks
open Giraffe
open Microsoft.AspNetCore.Http
open FunPizzaShop.Server.Views

let webApp (env:#_) (layout: HttpContext -> (int -> string Task) -> string Task) = //TODO: criar tipos semanticos para esta assinatura
    let viewRout view = 
        fun (next:HttpFunc) (ctx:HttpContext) -> 
            task {
                let! lay = view ctx |> layout ctx
                return! htmlString lay next ctx
            }

    let defaultRout = viewRout (Index.view env) 

    choose [ routeCi "/" >=> defaultRout ]


//TODO: UNDERSTAND THE PROPOUSE OF THIS FUNCTION
let webAppWrapper (env: #_) (layout: HttpContext -> (int -> Task<string>) -> Task<string>) =
    fun (next:HttpFunc) (ctx:HttpContext) -> 
        task {
            return! webApp env layout next ctx
        }