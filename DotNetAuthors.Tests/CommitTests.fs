// SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module DotNetAuthors.Tests.CommitTests

open System
open DotNetAuthors.Commits
open Fenrir.Git
open Fenrir.Git.Metadata
open Xunit

let private doTest bodyLines expectedData =
    let commit = {
        Hash = Sha1Hash.Zero
        Body = {
            Tree = Sha1Hash.Zero
            Parents = Array.empty
            Rest = bodyLines
        }
    }
    let author = GetContributors commit
    Assert.Equal<GitContribution>(
        expectedData |> Seq.map(fun(date, name, email) -> {
            Date = date
            Name = name
            Email = email
        }),
        author
    )

[<Fact>]
let ``Commit parser without author``(): unit =
    doTest [|
        ""
        "Some test changes"
    |] Array.empty

let private time = DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.FromHours -5)

[<Fact>]
let ``Commit parser without name``(): unit =
    doTest [|
        "author  <friedrich@fornever.me> 1457121723 -0500"
        ""
        "Some test changes"
    |] [| time, "", "friedrich@fornever.me" |]

[<Fact>]
let ``Commit parser without email``(): unit =
    doTest [|
        "author Friedrich von Never <> 1457121723 -0500"
        ""
        "Some test changes"
    |] [| time, "Friedrich von Never", "" |]

[<Fact>]
let ``Commit parser without name and email``(): unit =
    let ex = Assert.Throws(fun() ->
        doTest [|
            "author 1457121723 -0500"
            ""
            "Initial source commit"
        |] [| time, "Friedrich von Never", "" |]
    )
    Assert.Equal("Invalid commit author line: author 1457121723 -0500", ex.Message)
