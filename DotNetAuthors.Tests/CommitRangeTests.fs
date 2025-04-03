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

let private assertFilteredCommitSet head (filter: string seq | null) (expectedCommits: string[]) = task {
    let! repo = EnsureTestRepositoryCheckedOut()
    let headCommitHash = Sha1Hash.OfHexString head
    let commitRange = CommitRange(
        [| headCommitHash |],
        Set.ofArray [| VirtualRoot |]
    )

    let hashes =
        commitRange.ApplyToRepository repo
        |> AsyncSeq.map _.Hash

    let hashes =
        match filter with
        | null -> hashes
        | filter ->
            let filteredCommitSet = filter |> Seq.map Sha1Hash.OfHexString |> Set.ofSeq
            hashes
            |> AsyncSeq.filter(fun x -> Set.contains x filteredCommitSet)

    let! hashes = hashes |> AsyncSeq.toArrayAsync
    Assert.Equal<Sha1Hash>(expectedCommits |> Array.map Sha1Hash.OfHexString, hashes)
}

let private assertFullCommitSet head expectedCommits =
    assertFilteredCommitSet head null expectedCommits

[<Fact>]
let ``ApplyToRepository should work correctly with only one commit``(): Task =
    let firstCommitHash = "3751d9cd1ea573af7d37efcec5292cf36d8def98"
    assertFullCommitSet firstCommitHash [| firstCommitHash |]

[<Fact>]
let ``ApplyToRepository should work correctly with a small commit group``(): Task =
    let firstCommitHash = "db2ad6c0d589dc5ab7207b28f89bd67f9cf71031"
    assertFullCommitSet firstCommitHash [|
        firstCommitHash
        "367c1cd310239fc4c86786ec71d6281c152968cb"
        "3751d9cd1ea573af7d37efcec5292cf36d8def98"
    |]

[<Fact>]
let ``ApplyToRepository should work correctly with a merge commit``(): Task =
    let firstCommitHash = "f27e263cd1cc43bdd860ecc0b190179d7913eb2b"
    assertFullCommitSet firstCommitHash [|
        firstCommitHash
        "db2ad6c0d589dc5ab7207b28f89bd67f9cf71031"
        "367c1cd310239fc4c86786ec71d6281c152968cb"
        "3751d9cd1ea573af7d37efcec5292cf36d8def98"
    |]

[<Fact>]
let ``ApplyToRepository should work correctly with many roots``(): Task =
    let differentRepoMergeCommit = "04744f4f75de7915dba00d81a58ee2d9569d32af"
    let commitBeforeMerge1 = "cc54e88214df4c3f98c4230386a1a4fb97c190d9" // 2023-07-22
    let commitBeforeMerge2 = "f38cdeacd30b2f71cba3f9de1191499b948cab4f" // 2020-07-10
    let rootCommit1 = "3751d9cd1ea573af7d37efcec5292cf36d8def98" // 2016-02-19
    let rootCommit2 = "2df50c9f26f7647bd6c7884bbc3ee15109ceda69" // 2020-04-22

    let filter = [| differentRepoMergeCommit; commitBeforeMerge1; commitBeforeMerge2; rootCommit2; rootCommit1 |]
    assertFilteredCommitSet differentRepoMergeCommit filter filter
