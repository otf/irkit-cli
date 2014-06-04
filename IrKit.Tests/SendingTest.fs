namespace IrKit.Tests

open System
open NUnit.Framework
open FsUnit
open Foq
open System.Net.Http
open FSharpPlus
open IrKit

[<TestFixture>]
type SendingTest () =
  let sendAsyncWhenEmptyPost ip = 
    let isPostReq = fun (req:HttpRequestMessage) -> 
      req.Method = HttpMethod.Post
      && req.RequestUri = Uri(sprintf "http://%s/messages" ip)
    fun h -> <@ (h:HttpMessageInvoker).SendAsync(is(isPostReq), any()) @>

  let createHttpMock ip =
    Mock.With(fun h -> 
      <@
        %(sendAsyncWhenEmptyPost ip h) --> null
      @>)

  [<TestCase("192.168.1.200")>]
  [<TestCase("192.168.1.201")>]
  member test.``should request a msg when sending the msg to the device by wifi.`` ip =
    let httpMock = createHttpMock ip

    { Frequency = 40; Data = [] }
    |> send httpMock (Wifi ip)
    |> Async.RunSynchronously

    verify <@ %(httpMock |> sendAsyncWhenEmptyPost ip) @> once
  
  [<TestCase("192.168.1.200")>]
  member test.``should request a msg when sending the msg to the device by looked.`` ip =
    let httpMock = createHttpMock ip
    let resolve = fun r -> <@ (r:IDeviceEndPointResolver).Resolve() @>

    let resolver = Mock.With(fun r ->
      <@
        %(resolve r) --> async.Return [Wifi ip]
      @>)
  
    monad {
      let! dev = List.head <!> lookup resolver
      return! send httpMock dev { Frequency = 40; Data = [] }
    }
    |> Async.RunSynchronously

    verify <@ %(httpMock |> sendAsyncWhenEmptyPost ip) @> once