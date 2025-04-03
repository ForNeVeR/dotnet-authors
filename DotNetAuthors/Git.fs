// SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module DotNetAuthors.Git

open System
open System.Globalization
open System.Threading.Tasks
open FSharp.Control
open Fenrir.Git
open Fenrir.Git.Metadata
open Microsoft.Extensions.Logging
open TruePath

let GetAuthorDate(commit: Commit): DateTimeOffset =
    // TODO: Verify that the format is correct
    match commit.Body.Rest |> Seq.filter(fun x -> x.StartsWith "author ") |> Seq.tryHead with
    | None -> failwithf $"Cannot find author line in the commit body of commit {commit.Hash}."
    | Some authorLine ->

    let timeOffsetSeparator = authorLine.LastIndexOf ' '
    if timeOffsetSeparator = -1 then failwithf $"Incorrect author line format: \"{authorLine}\"."
    let timeSeparator = authorLine.LastIndexOf(' ', timeOffsetSeparator - 1)
    if timeSeparator = -1 then failwithf $"Incorrect author line format: \"{authorLine}\"."
    let time = int64(authorLine.Substring(timeSeparator + 1, timeOffsetSeparator - timeSeparator - 1))
    let offset = authorLine.Substring(timeOffsetSeparator + 1)
    if offset[0] <> '+' && offset[0] <> '-' then failwithf $"Invalid time zone offset value: \"{offset}\"."
    let offsetSign = int64 offset |> Math.Sign |> float
    let offsetValue = TimeSpan.ParseExact(offset.Substring(1), "hhmm", CultureInfo.InvariantCulture)
    DateTimeOffset.FromUnixTimeSeconds(time).ToOffset(offsetValue * offsetSign)

let ReadCommits (logger: ILogger) (repositoryBase: AbsolutePath): Task<string[]> = task {
    let gitDirectory = repositoryBase / ".git" |> LocalPath

    let! head = Refs.ReadHead(gitDirectory)
    let headCommit = ValueOption.ofObj head |> ValueOption.map _.CommitObjectId
    logger.LogTrace(
        "Head commit {Commit}.",
        headCommit |> ValueOption.map string |> ValueOption.defaultValue "not found"
    )
    return!
        headCommit
        |> ValueOption.toArray
        |> AsyncSeq.ofSeq
        |> AsyncSeq.collect(fun hash ->
            Commits.TraverseCommits(gitDirectory, hash)
            |> AsyncSeq.ofAsyncEnum
        )
        |> AsyncSeq.map(_.Hash.ToString())
        |> AsyncSeq.toArrayAsync
}
