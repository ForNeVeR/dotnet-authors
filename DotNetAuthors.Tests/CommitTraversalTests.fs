// SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

namespace DotNetAuthors.Tests

open System.Threading.Tasks
open DotNetAuthors
open DotNetAuthors.Tests.TestFramework
open Fenrir.Git
open Meziantou.Extensions.Logging.Xunit
open Microsoft.Extensions.Logging
open TruePath
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
        let! commits = Git.ReadCommits logger (Git.Repository testRepoBase)
        let! nativeCommits = CountCommitsUsingSystemClient testRepoBase
        Assert.True(nativeCommits > 0, $"Native commit count should be greater than zero but was {nativeCommits}.")
        Assert.Equal(nativeCommits, Seq.length commits)
    }

    [<Fact>]
    member _.``File contributing commits for single commit``(): Task = task {
        let! testRepoBase = EnsureTestRepositoryCheckedOut()
        let rootCommit = Sha1Hash.OfHexString "3751d9cd1ea573af7d37efcec5292cf36d8def98"
        let! contributingCommits = Git.GetCommitsPerFile (Git.Repository testRepoBase) rootCommit
        Assert.Equal([| rootCommit |], contributingCommits[LocalPath "README.md"])
    }

    [<Fact>]
    member _.``File contributing commits for multiple commits``(): Task = task {
        let! testRepoBase = EnsureTestRepositoryCheckedOut()
        let rootCommit = Sha1Hash.OfHexString "3751d9cd1ea573af7d37efcec5292cf36d8def98"
        let secondCommit = Sha1Hash.OfHexString "367c1cd310239fc4c86786ec71d6281c152968cb"
        let! contributingCommits = Git.GetCommitsPerFile (Git.Repository testRepoBase) secondCommit
        Assert.Equivalent([| rootCommit; secondCommit |], contributingCommits[LocalPath "README.md"])
        Assert.Equal([| secondCommit |], contributingCommits[LocalPath ".gitignore"])
        Assert.Equal([| secondCommit |], contributingCommits[LocalPath "build/.gitignore"])
    }
