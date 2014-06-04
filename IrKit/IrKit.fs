namespace IrKit

open FSharpPlus
open System.Threading
open System.Net.Http
open Zeroconf

type DeviceEndPoint = Wifi of string

type Message = {
  Frequency : int
  Data : int list
}

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

  let send (http:#HttpMessageInvoker) (endPoint:DeviceEndPoint) msg = async {
    let (Wifi ip) = endPoint
    let uri = sprintf "http://%s/messages" ip
    use req = new HttpRequestMessage(HttpMethod.Post, uri)
    let! _ = Async.AwaitTask <| http.SendAsync(req, CancellationToken.None)
    return ()
  }