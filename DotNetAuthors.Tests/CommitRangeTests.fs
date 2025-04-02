module DotNetAuthors.Tests.CommitRangeTests

open System.Threading.Tasks
open DotNetAuthors
open DotNetAuthors.Tests.TestFramework
open FSharp.Control
open Fenrir.Git
open Xunit

[<Fact>]
let ``Constructor should not pass empty list``(): unit =
    let e = Assert.Throws(fun() -> CommitRange(Array.empty, Set.empty) |> ignore)
    Assert.Equal("Head commit list should not be empty.", e.Message)

[<Fact>]
let ``ApplyToRepository should work correctly with only one commit``(): Task = task {
    let! repo = EnsureTestRepositoryCheckedOut()
    let firstCommitHash = Sha1Hash.OfHexString "3751d9cd1ea573af7d37efcec5292cf36d8def98"
    let commitRange = CommitRange(
        [| firstCommitHash |],
        Set.ofArray [| VirtualRoot |]
    )
    let! commits = commitRange.ApplyToRepository repo |> AsyncSeq.toArrayAsync
    let hashes = commits |> Seq.map _.Hash
    Assert.Equal([| firstCommitHash |], hashes)
}
