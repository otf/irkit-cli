namespace IrKit.Tests

open System
open NUnit.Framework
open FsUnit
open Foq
open System.Net.Http
open IrKit

[<TestFixture>]
type SendingTest () =

  [<Test>]
  member test.``should request a msg when sending the msg to the device by lookup.`` () =
    let expectedReq = fun (req:HttpRequestMessage) -> 
      req.Method = HttpMethod.Post
      && req.RequestUri = Uri("http://192.168.1.200/messages")

    let http : HttpMessageInvoker = Mock.With(fun h -> 
      <@
        h.SendAsync(is(expectedReq), any()) --> null
      @>)

    { Frequency = 40; Data = [] }
    |> send http Lookup 
    |> Async.RunSynchronously

    expect <@ http.SendAsync(any(), any()) @> once
    verifyAll http