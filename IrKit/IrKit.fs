namespace IrKit

open FSharpPlus
open System.Threading
open System.Net.Http
open Fleece
open Fleece.Operators
open Zeroconf

type DeviceEndPoint = Wifi of string // TODO: add | Internet of ClientKey * DeviceId 

type RawMessage = {
  Frequency : int
  Data : int list
}

type RawMessage with
  static member ToJSON (x: RawMessage) =
    jobj [ 
      "format" .= "raw"
      "freq" .= x.Frequency
      "data" .= x.Data
    ]

type IDeviceEndPointResolver =
  abstract Resolve : unit -> Async<DeviceEndPoint list>

[<AutoOpen>]
module IrKitFuncs =
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