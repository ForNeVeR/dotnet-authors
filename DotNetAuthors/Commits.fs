// SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module DotNetAuthors.Commits

open System
open System.Text.RegularExpressions
open Fenrir.Git.Metadata

type GitAuthor = {
    Name: string | null
    Email: string | null
}

type GitContributionInfo = {
    Author: GitAuthor
    Date: DateTimeOffset
}

let private authorRegex = Regex("author (.*?) <(.*?)> .*?", RegexOptions.Compiled)

let GetAuthor(commit: Commit): GitContributionInfo =
    // TODO[#52]: Support author + committer as well?
    // TODO[#52]: Support Co-authored-by as well

    match commit.Body.Rest |> Seq.filter(_.StartsWith("author ")) |> Seq.tryHead with
    | None -> { Name = null; Email = null }
    | Some authorLine ->

    let m = authorRegex.Match authorLine
    if m.Success then
        { Name = m.Groups[1].Value; Email = m.Groups[2].Value }
    else failwithf $"Invalid commit author line: {authorLine}"
