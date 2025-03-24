// SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module DotNetAuthors.Git

open System.Threading.Tasks
open FSharp.Control
open Fenrir.Git
open Microsoft.Extensions.Logging
open TruePath

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
