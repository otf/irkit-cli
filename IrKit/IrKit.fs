namespace IrKit

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
  let lookup (resolver:IDeviceEndPointResolver) =
    resolver.Resolve()

  let send (http:#HttpMessageInvoker) (endPoint:DeviceEndPoint) msg = async {
    let (Wifi ip) = endPoint
    let uri = sprintf "http://%s/messages" ip
    use req = new HttpRequestMessage(HttpMethod.Post, uri)
    let _ = http.SendAsync(req, CancellationToken.None)
    return ()
  }