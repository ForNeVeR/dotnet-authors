// SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module DotNetAuthors.Commands

open System.Collections.Generic
open System.Collections.Immutable
open System.Threading.Tasks
open DotNetAuthors.Commits
open FVNever.Reuse
open Fenrir.Git
open TruePath

let UpdateCopyrightStatements (existingStatements: string seq) (contributors: GitContribution seq): option<seq<string>> =
    failwithf "TODO"
    // TODO: Go through the contributors, find a place for each in the existingStatements list.
    // TODO: If any is not found, product a new item, while incorporating the existing items while required.
    // TODO: Essentially, three modes for now:
    // TODO: 1. No update
    // TODO: 2. Year update
    // TODO: 3. Update to group copyright owner, replacing the existing nodes

/// Accepts data from files and from the metadata source; returns the collection of entries that should be updated.
let PatchMetadata (metadata: ReuseFileEntry seq)
                  (authors: IReadOnlyDictionary<AbsolutePath, #seq<GitContribution>>): ReuseFileEntry seq =
    seq {
        for entry in metadata do
            let existingCopyrights = entry.CopyrightStatements
            let contributors = authors[entry.Path]
            let updatedStatements = UpdateCopyrightStatements existingCopyrights contributors
            match updatedStatements with
            | None -> ()
            | Some statements ->
                let updated = ReuseFileEntry(
                    entry.Path,
                    LicenseIdentifiers = entry.LicenseIdentifiers,
                    CopyrightStatements = ImmutableArray.ToImmutableArray statements
                )
                yield updated
    }

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
    let! actualContributors = Git.GetContributorsPerFile repository headCommit.CommitObjectId
    let mappedContributions = Dictionary()
    for kvp in actualContributors do
        let path = repository.Root / kvp.Key
        let contributors = kvp.Value
        mappedContributions.Add(path, contributors)

    let patch = PatchMetadata currentMetadata mappedContributions
    return! ApplyMetadata patch
}
