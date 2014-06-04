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
  [<TestCase("192.168.1.200")>]
  member test.``should found the endpoint when lookup-ing`` ip =
    lookup zeroConfResolver
    |> Async.RunSynchronously
    |> should equal [Wifi ip]
