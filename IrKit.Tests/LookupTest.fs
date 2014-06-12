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
  [<TestCase>]
  member test.``should found the endpoint when lookup-ing`` () =
    lookup zeroconfResolver
    |> Async.RunSynchronously
    |> should haveLength 1