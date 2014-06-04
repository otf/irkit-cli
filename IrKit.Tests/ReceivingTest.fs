namespace IrKit.Tests

open System
open NUnit.Framework
open FsUnit
open Foq
open System.Net.Http
open System.Threading.Tasks
open System.Json
open FSharpPlus
open IrKit

[<TestFixture>]
type ReceivingTest () =
  let sendAsyncWhenGet ip = 
    let isGetReq = fun (req:HttpRequestMessage) -> 
      req.Method = HttpMethod.Get
      && req.RequestUri = Uri(sprintf "http://%s/messages" ip)

    fun h -> <@ (h:HttpMessageInvoker).SendAsync(is(isGetReq), any()) @>

  let createHttpMock ip content =
    let response = Task.Factory.StartNew(fun () -> 
        let resp = new HttpResponseMessage()
        resp.Content <- new StringContent(content)
        resp
      )
    Mock.With(fun h -> 
      <@
        %(sendAsyncWhenGet ip h) --> response
      @>)
  
  [<TestCase("192.168.1.200", 40, "[]")>]
  [<TestCase("192.168.1.201", 36, "[0,1,2]")>]
  member test.``should request getting msg when sending request to the device by looked.`` ip freq data =
    let content =  sprintf @"{""format"":""raw"",""freq"":%d,""data"":%s}" freq data
    let httpMock = createHttpMock ip content
    let resolve = fun r -> <@ (r:IDeviceEndPointResolver).ResolveAsync() @>

    let resolver = Mock.With(fun r ->
      <@
        %(resolve r) --> async.Return [Wifi ip]
      @>)
  
    let data = List.ofSeq ((JsonValue.Parse data :?> JsonArray) |> map (fun v -> v.Value.ReadAs<int>()) )
    monad {
      let! dev = List.head <!> lookup resolver
      return! receive httpMock dev
    }
    |> Async.RunSynchronously
    |> should equal ({ Frequency = freq; Data = data })

    verify <@ %(httpMock |> sendAsyncWhenGet ip) @> once

