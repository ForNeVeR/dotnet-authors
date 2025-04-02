module DotNetAuthors.Tests.CommitRangeTests

open DotNetAuthors
open Xunit

[<Fact>]
let ``Constructor should not pass empty list``(): unit =
    let e = Assert.Throws(fun() -> CommitRange(Array.empty, Array.empty) |> ignore)
    Assert.Equal("Head commit list should not be empty.", e.Message)
