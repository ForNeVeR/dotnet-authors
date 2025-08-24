module DotNetAuthors.Tests.CommandTests

open System.Collections.Immutable
open System.Threading.Tasks
open DotNetAuthors
open FVNever.Reuse
open TruePath
open Xunit

[<Fact>]
let ``authors-to-metadata works correctly``(): Task = task {
    let configuration = {
        DefaultAuthorGroup = "LogList contributors <https://github.com/codingteam/loglist>"
    }

    let! repo = TestFramework.CloneLogList()

    let repository = Git.Repository AbsolutePath.CurrentWorkingDirectory
    do! Commands.WriteAuthorMetadata(configuration, repository)

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
