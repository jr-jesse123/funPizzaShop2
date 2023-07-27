module FunPizzaShop.Server.Views.Index

open FunPizzaShop.Server.Views

open Common
open Thoth.Json.Net
open Microsoft.AspNetCore.Http

let view (env:#_) (ctx:HttpContext) (datalevel: int) = task{//TODO: MAYBE CREATE DEDICATED TYPES FOR THE DEPENDENCIES?
    return html $""" Hello World! """
}