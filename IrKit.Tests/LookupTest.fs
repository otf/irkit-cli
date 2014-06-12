namespace IrKit.Tests

open System
open NUnit.Framework
open FsUnit
open System.Net.Http
open System.Threading.Tasks
open IrKit

[<TestFixture>]
[<Explicit>]
type LookupTest () =
  [<TestCase("192.168.11.28")>]
  member test.``should found the endpoint when lookup-ing`` ip =
    lookup zeroconfResolver
    |> Async.RunSynchronously
    |> should contain (Wifi ip)