namespace IrKit.Tests

open NUnit.Framework
open FsUnit
open Foq
open System.Net.Http
open IrKit

[<TestFixture>]
type SendingTest () =

  [<Test>]
  member test.``should request a msg when sending the msg.`` () =
    let http : HttpMessageInvoker = Mock.With(fun h -> 
      <@
        h.SendAsync(is(fun httpMsg -> httpMsg.Method = HttpMethod.Post), any()) --> null
      @>)

    { Frequency = 40; Data = [] }
    |> send http Lookup 
    |> Async.RunSynchronously

    expect <@ http.SendAsync(any(), any()) @> once
    verifyAll http