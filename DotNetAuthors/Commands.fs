// SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module DotNetAuthors.Commands

open System.Collections.Generic
open System.Threading.Tasks
open FVNever.Reuse
open Fenrir.Git
open TruePath

/// Accepts data from files and from the metadata source; returns the collection of entries to patch.
let PatchMetadata (metadata: ReuseFileEntry seq)
                  (authors: IReadOnlyDictionary<AbsolutePath, #IReadOnlySet<ContributionInfo>>): ReuseFileEntry seq =
    failwithf "TODO"

let private ApplyMetadata (metadata: ReuseFileEntry seq) = task {
    for meta in metadata  do
        do! meta.UpdateFileContents()
}

let WriteAuthorMetadata(config: Config, repository: Git.Repository): Task = task {
    let! headCommit = Refs.ReadHead repository.DotGit
    match headCommit with
    | null -> failwithf $"Cannot determine HEAD commit in repository \"{repository.DotGit.Value}\"."
    | headCommit ->

    let! currentMetadata = ReuseDirectory.ReadEntries repository.Root
    let! actualAuthors = Git.GetAuthorsPerFile repository headCommit.CommitObjectId
    let patch = PatchMetadata currentMetadata actualAuthors
    return! ApplyMetadata patch
}
