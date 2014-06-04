namespace IrKit.Tests

open System
open NUnit.Framework
open FsUnit
open Foq
open System.Net.Http
open System.Threading.Tasks
open FSharpPlus
open IrKit

[<TestFixture>]
type SendingTest () =
  let sendAsyncWhenPost ip content = 
    let equalsContent expected (actualContent:HttpContent) =
      let str = actualContent.ReadAsStringAsync() |> Async.AwaitTask |> Async.RunSynchronously
      expected = str

    let isPostReq = fun (req:HttpRequestMessage) -> 
      req.Method = HttpMethod.Post
      && req.RequestUri = Uri(sprintf "http://%s/messages" ip)
      && equalsContent content req.Content

    fun h -> <@ (h:HttpMessageInvoker).SendAsync(is(isPostReq), any()) @>

  let createHttpMock ip content =
    let response = Task.Factory.StartNew(fun () -> new HttpResponseMessage())
    Mock.With(fun h -> 
      <@
        %(sendAsyncWhenPost ip content h) --> response
      @>)

  [<TestCase("192.168.1.200")>]
  [<TestCase("192.168.1.201")>]
  member test.``should request a msg when sending the msg to the device by wifi.`` ip =
    let content =  @"{""format"": ""raw"", ""freq"": 40, ""data"": [0,1,2]}" 
    let httpMock = createHttpMock ip content

    { Frequency = 40; Data = [0; 1; 2] }
    |> send httpMock (Wifi ip)
    |> Async.RunSynchronously

    verify <@ %(httpMock |> sendAsyncWhenPost ip content) @> once
  
  [<TestCase("192.168.1.200")>]
  member test.``should request a msg when sending the msg to the device by looked.`` ip =
    let content =  @"{""format"": ""raw"", ""freq"": 40, ""data"": [0,1,2]}" 
    let httpMock = createHttpMock ip content
    let resolve = fun r -> <@ (r:IDeviceEndPointResolver).Resolve() @>

    let resolver = Mock.With(fun r ->
      <@
        %(resolve r) --> async.Return [Wifi ip]
      @>)
  
    monad {
      let! dev = List.head <!> lookup resolver
      return! send httpMock dev { Frequency = 40; Data = [0; 1; 2] }
    }
    |> Async.RunSynchronously

    verify <@ %(httpMock |> sendAsyncWhenPost ip content) @> once