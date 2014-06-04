namespace IrKit

open System.Net.Http

type DeviceEndPoint = Lookup

type Message = {
  Frequency : int
  Data : int list
}

[<AutoOpen>]
module IrKitFuncs =
  let send msg endPoint (http:#HttpMessageInvoker) = async {
    return ()
  }