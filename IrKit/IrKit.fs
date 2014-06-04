namespace IrKit

open FSharpPlus
open System.Threading
open System.Net.Http
open System.Json
open Fleece
open Fleece.Operators
open Zeroconf

[<AutoOpen>]
module IrKitData = 
  type DeviceEndPoint = Wifi of string // TODO: add | Internet of ClientKey * DeviceId 

  type RawMessage = {
    Frequency : int
    Data : int list
  }
  let createRawMessage (freq:int) (data:int list) = { Frequency = freq; Data = data }

  type RawMessage with
    static member ToJSON (x: RawMessage) =
      jobj [ 
        "format" .= "raw"
        "freq" .= x.Frequency
        "data" .= x.Data
      ]

    static member FromJSON (_: RawMessage) =
      function
      | JObject o -> createRawMessage <!> (o .@ "freq") <*> (o .@ "data") 
      | x -> Failure (sprintf "Expected RawMessae, found %A" x)

  type IDeviceEndPointResolver =
    abstract Resolve : unit -> Async<DeviceEndPoint list>

[<AutoOpen>]
module IrKitService =
  let zeroconfResolver = { new IDeviceEndPointResolver with
    member this.Resolve () = async {
      let! hosts = Async.AwaitTask <| ZeroconfResolver.ResolveAsync("_irkit._tcp.local.")
      return hosts |> map (fun host -> Wifi host.IPAddress) |> Seq.toList
    }
  }

  let lookup (resolver:IDeviceEndPointResolver) =
    resolver.Resolve()

  let send (http:#HttpMessageInvoker) (Wifi ip:DeviceEndPoint) (msg:RawMessage) = async {
    let req = new HttpRequestMessage(HttpMethod.Post, sprintf "http://%s/messages" ip)
    req.Content <- new StringContent((msg |> toJSON).ToString())
    let! _ = Async.AwaitTask <| http.SendAsync(req, CancellationToken.None)
    return ()
  }

  let receive (http:#HttpMessageInvoker) (Wifi ip:DeviceEndPoint) = async {
    let req = new HttpRequestMessage(HttpMethod.Get, sprintf "http://%s/messages" ip)
    let! resp = Async.AwaitTask <| http.SendAsync(req, CancellationToken.None)
    let! str = Async.AwaitTask <| resp.Content.ReadAsStringAsync()
    let respMsg : RawMessage ParseResult = (JsonValue.Parse >> fromJSON) str
    match respMsg with
    | Success s -> return s
    | Failure fmsg -> return failwith fmsg 
  }