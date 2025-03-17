// SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module DotNetAuthors.Git

open System.Collections.Generic
open System.Threading.Tasks
open Fenrir.Git
open Microsoft.Extensions.Logging
open TruePath

#nowarn 3511 // non statically-compilable state machine
let ReadCommits (logger: ILogger) (repositoryBase: AbsolutePath): Task<string[]> = task {
    let gitDirectory = repositoryBase / ".git"

    let traverseCommitTree(hash: string) = seq {
        let visitedCommits = HashSet()
        let currentCommits = Stack [| hash |]
        while currentCommits.Count > 0 do
            let commitHash = currentCommits.Pop()
            if visitedCommits.Add commitHash then
                logger.LogTrace("Enumerating commit {Commit}.", commitHash)
                yield commitHash

                let commit = Commands.parseCommitBody gitDirectory.Value commitHash
                commit.Parents |> Array.iter currentCommits.Push
    }

    let! head = Refs.ReadHeadRef(LocalPath gitDirectory)
    let headCommit = ValueOption.ofObj head |> ValueOption.map _.CommitObjectId
    logger.LogTrace(
        "Head commit {Commit}.",
        headCommit |> ValueOption.map string |> ValueOption.defaultValue "not found"
    )
    let result = headCommit |> ValueOption.toArray |> Seq.collect traverseCommitTree |> Seq.toArray
    return result
}
