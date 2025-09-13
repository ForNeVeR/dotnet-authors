// SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module DotNetAuthors.Commits

open System
open System.Text.RegularExpressions
open Fenrir.Git.Metadata

type GitContribution = {
    Name: string
    Email: string
    Date: DateTimeOffset
}

let private authorRegex = Regex("author (.*?) <(.*?)> (.*)", RegexOptions.Compiled)

let private parseDate (d: string) =
    let components = d.Split(' ', 2)
    if components.Length <> 2 then failwithf $"Invalid date expression: {d}"
    let unixTimestampSec = int64 components[0]
    let timeOffset = components[1]
    if timeOffset.Length <> 5 then failwithf $"Invalid time offset value: {timeOffset}"
    let timeOffset =
        let v = TimeSpan.FromHours(int(timeOffset.Substring(1, 2)), int(timeOffset.Substring(3, 2)))
        match timeOffset[0] with
        | '-' -> -v
        | '+' -> v
        | _ -> failwithf $"Invalid time offset value (no sign): {timeOffset}"

    DateTimeOffset
        .FromUnixTimeSeconds(unixTimestampSec)
        .ToOffset timeOffset

let GetContributors (commit: Commit): GitContribution seq =
    // TODO[#52]: Support different author + committer as well?
    // TODO[#52]: Support Co-authored-by as well

    match commit.Body.Rest |> Seq.filter(_.StartsWith("author ")) |> Seq.tryHead with
    | None -> Seq.empty
    | Some authorLine ->

    let m = authorRegex.Match authorLine
    if m.Success then
        { Date = parseDate m.Groups[3].Value; Name = m.Groups[1].Value; Email = m.Groups[2].Value }
        |> Seq.singleton
    else failwithf $"Invalid commit author line: {authorLine}"
