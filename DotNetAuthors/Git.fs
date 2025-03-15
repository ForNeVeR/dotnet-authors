// SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module DotNetAuthors.Git

open System
open System.IO
open System.Threading.Tasks
open Fenrir.Git
open TruePath

// TODO: Move this to Fenrir, https://github.com/ForNeVeR/Fenrir/issues/105
let private ReadHead(repositoryBase: AbsolutePath): Task<Option<string>> = task {
    let gitDirectory = repositoryBase / ".git"
    if gitDirectory.ReadKind() <> Nullable FileEntryKind.Directory then return None
    else

    let headFile = gitDirectory / "HEAD"
    let! content = File.ReadAllTextAsync headFile.Value
    return
        if content.StartsWith "ref:" then
            Some <| content.Substring("ref:".Length).Trim()
        else
            failwithf $"Unknown HEAD file state: \"{content}\"."
}

let GetHeadCommit(repositoryBase: AbsolutePath): Task<Option<string>> = task {
    do! Task.Yield()
    let! headRefName = ReadHead repositoryBase
    return headRefName |> Option.bind(fun headRefName ->
        let gitDirectory = repositoryBase / ".git"

        let refs = Refs.readRefs gitDirectory.Value
        let head = refs |> Seq.tryFind(fun r -> r.Name = headRefName)
        head |> Option.map _.CommitObjectId
    )
}

#nowarn 3511 // non statically-compilable state machine
let ReadCommits(repositoryBase: AbsolutePath): Task<string[]> = task {
    do! Task.Yield()
    let gitDirectory = repositoryBase / ".git"

    let rec traverseCommitTree hash = seq {
        yield hash
        let commitObject = Commands.parseCommitBody gitDirectory.Value hash
        yield! commitObject.Parents |> Seq.collect traverseCommitTree
    }

    let! headCommit = GetHeadCommit repositoryBase
    let result = headCommit |> Option.toArray |> Seq.collect traverseCommitTree |> Seq.toArray
    return result
}
