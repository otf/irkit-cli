namespace IrKit

open System.Threading
open System.Net.Http

type DeviceEndPoint = Lookup

type Message = {
  Frequency : int
  Data : int list
}

[<AutoOpen>]
module IrKitFuncs =
  let send (http:#HttpMessageInvoker) endPoint msg = async {
    use req = new HttpRequestMessage(HttpMethod.Post, "http://192.168.1.20/messages")
    let _ = http.SendAsync(req, CancellationToken.None)
    return ()
  }