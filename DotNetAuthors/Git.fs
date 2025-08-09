// SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module DotNetAuthors.Git

open System.Collections.Generic
open System.Threading.Tasks
open FSharp.Control
open Fenrir.Git
open JetBrains.Lifetimes
open Microsoft.Extensions.Logging
open TruePath

type Repository(root: AbsolutePath) =
    member val DotGit = root / ".git" |> LocalPath

let ReadCommits (logger: ILogger) (repository: Repository): Task<string[]> = task {
    let! head = Refs.ReadHead repository.DotGit
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
            Commits.TraverseCommits(repository.DotGit, hash)
            |> AsyncSeq.ofAsyncEnum
        )
        |> AsyncSeq.map(_.Hash.ToString())
        |> AsyncSeq.toArrayAsync
}

let GetCommitsPerFile (repository: Repository)
                      (startCommit: Sha1Hash)
                      : Task<Dictionary<LocalPath, ResizeArray<Sha1Hash>>> = task {
    let commits = Commits.TraverseCommits(repository.DotGit, startCommit)

    let mergeState (map: IDictionary<LocalPath, ResizeArray<Sha1Hash>>) newFiles newCommit =
        for file in newFiles do
            let array =
                match map.TryGetValue file with
                | true, array -> array
                | false, _ ->
                    let array = ResizeArray()
                    map.Add(file, array)
                    array
            array.Add newCommit

    return! Lifetime.UsingAsync(fun lt -> task {
        let index = PackIndex(lt, repository.DotGit)
        let! _, fullFileMap =
            commits
            |> AsyncSeq.ofAsyncEnum
            |> AsyncSeq.mapAsync(fun commit -> Async.AwaitTask <| Trees.ReadFull index repository.DotGit commit)
            |> AsyncSeq.fold (fun (prevTree, resultMap) nextTree ->
                let diff =
                    prevTree
                    |> Option.map(fun prevTree -> Trees.Diff prevTree nextTree)
                    |> Option.defaultWith(fun() -> nextTree.Files.Keys)

                mergeState resultMap diff nextTree.CommitHash
                Some nextTree, resultMap
            ) (None, Dictionary())
        return fullFileMap
    })
}
