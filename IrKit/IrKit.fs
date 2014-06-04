﻿namespace IrKit

open System.Threading
open System.Net.Http

type DeviceEndPoint = Wifi of string

type Message = {
  Frequency : int
  Data : int list
}

type IDeviceEndPointResolver =
  abstract Resolve : unit -> Async<DeviceEndPoint list>

[<AutoOpen>]
module IrKitFuncs =
  let zeroConfResolver = { new IDeviceEndPointResolver with
    member this.Resolve () = async.Return [Wifi "192.168.1.200"]
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