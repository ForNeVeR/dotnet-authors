// SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module DotNetAuthors.Tests.CommitTests

open DotNetAuthors.Commits
open Fenrir.Git
open Fenrir.Git.Metadata
open Xunit

let private doTest bodyLines expectedAuthor expectedEmail =
    let commit = {
        Hash = Sha1Hash.Zero
        Body = {
            Tree = Sha1Hash.Zero
            Parents = Array.empty
            Rest = bodyLines
        }
    }
    let author = GetContributors commit
    Assert.Equal({
        Name = expectedAuthor
        Email = expectedEmail
    }, author)

[<Fact>]
let ``Commit parser without author``(): unit =
    doTest [|
        ""
        "Some test changes"
    |] null null

[<Fact>]
let ``Commit parser without name``(): unit =
    doTest [|
        "author  <friedrich@fornever.me> 1457121723 -0500"
        ""
        "Some test changes"
    |] "" "friedrich@fornever.me"

[<Fact>]
let ``Commit parser without email``(): unit =
    doTest [|
        "author Friedrich von Never <> 1457121723 -0500"
        ""
        "Some test changes"
    |] "Friedrich von Never" ""

[<Fact>]
let ``Commit parser without name and email``(): unit =
    let ex = Assert.Throws(fun() ->
        doTest [|
            "author 1457121723 -0500"
            ""
            "Initial source commit"
        |] null null
    )
    Assert.Equal("Invalid commit author line: author 1457121723 -0500", ex.Message)
