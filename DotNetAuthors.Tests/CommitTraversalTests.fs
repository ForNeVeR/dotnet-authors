// SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module DotNetAuthors.Tests.CommitTraversalTests

open System.Threading.Tasks
open DotNetAuthors
open DotNetAuthors.Tests.TestFramework
open Xunit

[<Fact>]
let ``Commit count should be correct``(): Task = task {
    let! testRepoBase = EnsureTestRepositoryCheckedOut()
    let! commits = Git.ReadCommits testRepoBase
    Assert.Equal(1000, Seq.length commits)
}
