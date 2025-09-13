module DotNetAuthors.Tests.CommandTests

open System
open System.Collections.Generic
open System.Collections.Immutable
open System.Threading.Tasks
open DotNetAuthors
open DotNetAuthors.Commands
open DotNetAuthors.Commits
open FVNever.Reuse
open TruePath
open Xunit

[<Fact>]
let ``UpdateCopyrightStatements should generate a new entry``(): unit =
    failwithf "TODO"

[<Fact>]
let ``UpdateCopyrightStatements should update the entry year``(): unit =
    failwithf "TODO"

[<Fact>]
let ``UpdateCopyrightStatements should replace an entry with a group entry``(): unit =
    failwithf "TODO"

[<Fact>]
let ``UpdateCopyrightStatements should not replace an identical entry``(): unit =
    failwithf "TODO"

[<Fact>]
let ``UpdateCopyrightStatements should not replace a contained entry``(): unit =
    failwithf "TODO"

[<Fact>]
let ``authors-to-metadata works correctly``(): Task = task {
    let configuration = {
        DefaultAuthorGroup = "LogList contributors <https://github.com/codingteam/loglist>"
    }

    let! repo = TestFramework.CloneLogList()

    let repository = Git.Repository repo
    do! WriteAuthorMetadata(configuration, repository)

    let! data = ReuseDirectory.ReadEntries repo
    let readMePath = repo / "README.md"
    let readMeFileData = data |> Seq.filter(fun x -> x.Path = readMePath) |> Seq.exactlyOne
    Assert.Equal(
        ReuseFileEntry(
            readMePath,
            ImmutableArray.Create "2014-2025 LogList contributors <https://github.com/codingteam/loglist>",
            ImmutableArray.Create "MIT"
        ), readMeFileData
    )
}

[<Fact>]
let ``PatchMetadata provides a correct patch``(): unit =
    let path x = AbsolutePath.CurrentWorkingDirectory / x
    let originalData = [|
        ReuseFileEntry(
            path "1",
            ImmutableArray.Create("MIT", "Apache-2.0"),
            ImmutableArray.Create("2021-2025 Test <test@example.com>")
        )
        ReuseFileEntry(
            path "2",
            ImmutableArray.Create("MIT"),
            ImmutableArray.Create(
                "2021 Test <test@example.com>",
                "2025 Test <test@example.com>"
            )
        )
        ReuseFileEntry(
            path "3",
            ImmutableArray.Create("MIT"),
            ImmutableArray.Create(
                "2021 Test <test@example.com>",
                "2025 Test <test@example.com>"
            )
        )
    |]
    let authorData = Dictionary [|
        let year x = DateTimeOffset(x, 1, 1, 0, 0, 0, TimeSpan.Zero)
        let entry p y =
            KeyValuePair(
                path p,
                HashSet([|{ Date = year y; Name = "Test"; Email = "test@example.com" }|])
            )
        entry "1" 2025
        entry "2" 2023
        entry "4" 2023
    |]
    let expectedResult = [|
        ReuseFileEntry(
            path "2",
            ImmutableArray.Create("MIT"),
            ImmutableArray.Create("2021-2025 Test <test@example.com>")
        )
        ReuseFileEntry(
            path "4",
            ImmutableArray.Create(),
            ImmutableArray.Create("2023 Test <test@example.com>")
        )
    |]

    Assert.Equal(expectedResult, PatchMetadata originalData authorData)
