namespace IrKit.Tests

open System
open NUnit.Framework
open FsUnit
open Foq
open System.Net.Http
open IrKit

[<TestFixture>]
type SendingTest () =

  [<TestCase("192.168.1.200")>]
  [<TestCase("192.168.1.201")>]
  member test.``should request a msg when sending the msg to the device by wifi.`` ip =
    let expectedReq = fun (req:HttpRequestMessage) -> 
      req.Method = HttpMethod.Post
      && req.RequestUri = Uri(sprintf "http://%s/messages" ip)
      
    let sendAsync = fun h -> <@ (h:HttpMessageInvoker).SendAsync(is(expectedReq), any()) @>

    let http = Mock.With(fun h -> 
      <@
        %(sendAsync h) --> null
      @>)

    { Frequency = 40; Data = [] }
    |> send http (Wifi ip)
    |> Async.RunSynchronously

    verify <@ %(sendAsync http) @> once