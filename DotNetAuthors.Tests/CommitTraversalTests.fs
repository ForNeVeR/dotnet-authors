// SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

namespace DotNetAuthors.Tests

open System
open System.Security.Cryptography
open System.Text
open System.Threading.Tasks
open DotNetAuthors
open DotNetAuthors.Commits
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
        let! testRepoBase = CloneTeamExplorerEverywhere()
        let! commits = Git.ReadCommits logger (Git.Repository testRepoBase)
        let! nativeCommits = CountCommitsUsingSystemClient testRepoBase
        Assert.True(nativeCommits > 0, $"Native commit count should be greater than zero but was {nativeCommits}.")
        Assert.Equal(nativeCommits, Seq.length commits)
    }

    [<Fact>]
    member _.``File contributing commits for single commit``(): Task = task {
        let! testRepoBase = CloneTeamExplorerEverywhere()
        let rootCommit = Sha1Hash.OfHexString "3751d9cd1ea573af7d37efcec5292cf36d8def98"
        let! contributingCommits = Git.GetCommitsPerFile (Git.Repository testRepoBase) rootCommit
        Assert.Equal([| rootCommit |], contributingCommits[LocalPath "README.md"])
    }

    [<Fact>]
    member _.``File contributing commits for multiple commits``(): Task = task {
        let! testRepoBase = CloneTeamExplorerEverywhere()
        let rootCommit = Sha1Hash.OfHexString "3751d9cd1ea573af7d37efcec5292cf36d8def98"
        let secondCommit = Sha1Hash.OfHexString "367c1cd310239fc4c86786ec71d6281c152968cb"
        let! contributingCommits = Git.GetCommitsPerFile (Git.Repository testRepoBase) secondCommit
        Assert.Equivalent([| rootCommit; secondCommit |], contributingCommits[LocalPath "README.md"])
        Assert.Equal([| secondCommit |], contributingCommits[LocalPath ".gitignore"])
        Assert.Equal([| secondCommit |], contributingCommits[LocalPath "build/.gitignore"])
    }

    [<Fact>]
    member _.``Authors contributing commits``(): Task = task {
        let sha256(s: string | null) =
            match s with
            | null -> ""
            | s ->

            use sha256 = SHA256.Create()
            let bytes = Encoding.UTF8.GetBytes(s)
            let hash = sha256.ComputeHash(bytes)
            Convert.ToHexString hash

        let! testRepoBase = CloneTeamExplorerEverywhere()
        let secondCommit = Sha1Hash.OfHexString "367c1cd310239fc4c86786ec71d6281c152968cb"
        let! contributingCommits = Git.GetContributorsPerFile (Git.Repository testRepoBase) secondCommit
        let assertContribution1(a: GitContribution) = // compare hashes to not expose other people's emails in our code
            Assert.Equal(DateTimeOffset(0L, TimeSpan.Zero), a.Date)
            Assert.Equal("David Staheli", a.Name)
            Assert.Equal("91B948ADED5F83812F6F2308F44E15CE41A12786F16FFE37C66524AF3C7D1D53", sha256 a.Email)
        let assertContribution2(a: GitContribution) =
            Assert.Equal(DateTimeOffset(0L, TimeSpan.Zero), a.Date)
            Assert.Equal("Microsoft GitHub User", a.Name)
            Assert.Equal("87E5C9B285C4C2EF84DAE798C1D05709106E01715D080F90D9FC19AA313D6E92", sha256 a.Email)

        Assert.Collection<GitContribution>(
            contributingCommits[LocalPath "README.md"] |> Seq.sortBy _.Email,
            assertContribution1,
            assertContribution2
        )
        assertContribution1(contributingCommits[LocalPath ".gitignore"] |> Seq.exactlyOne)
        assertContribution1(contributingCommits[LocalPath "build/.gitignore"] |> Seq.exactlyOne)
    }
