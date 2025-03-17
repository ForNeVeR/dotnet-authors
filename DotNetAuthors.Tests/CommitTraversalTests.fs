// SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

namespace DotNetAuthors.Tests

open System.Threading.Tasks
open DotNetAuthors
open DotNetAuthors.Tests.TestFramework
open Meziantou.Extensions.Logging.Xunit
open Microsoft.Extensions.Logging
open Xunit
open Xunit.Abstractions

type CommitTraversalTests(output: ITestOutputHelper) =

    let logger = XUnitLogger(output, LoggerExternalScopeProvider(), null)

    let CountCommitsUsingSystemClient repo = task {
        let! result = RunGit(repo, [|"log"; "--oneline"|])
        let lines = result.StandardOutput.Trim().Split '\n'
        return lines.Length
    }

    [<Fact>]
    member _.``Commit count should be correct``(): Task = task {
        let! testRepoBase = EnsureTestRepositoryCheckedOut()
        let! commits = Git.ReadCommits logger testRepoBase
        let! nativeCommits = CountCommitsUsingSystemClient testRepoBase
        Assert.True(nativeCommits > 0, $"Native commit count should be greater than zero but was {nativeCommits}.")
        Assert.Equal(nativeCommits, Seq.length commits)
    }
