// SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module DotNetAuthors.Tests.TestFramework

open System.IO
open System.Threading
open System.Threading.Tasks
open Fenrir.Git
open Medallion.Shell
open TruePath

let private TeamExplorerRepoName = "team-explorer-everywhere"
let private TeamExplorerRepoCloneUrl = "https://github.com/ForNeVeR/team-explorer-everywhere.git"
let private TeamExplorerRepoCommit = Sha1Hash.OfHexString "5a07ff83130e12be69cc59295c81eb1f58a90c27"

let private LogListRepoCloneUrl = "https://github.com/codingteam/loglist.git"
let private LogListRepoName = "loglist"
let private LogListRepoCommit = Sha1Hash.OfHexString "77f63b4983da08612e78018e9cb53162d8c9f7d6"

let private AccessMutex = new SemaphoreSlim(1)

let RunGit(workDirectory: AbsolutePath, arguments: string[]) = task {
    let arguments' = seq {
        // To properly process long paths on Windows:
        yield "-c" :> obj; yield "core.longpaths=true"
        yield! arguments |> Seq.map(fun x -> x :> obj)
    }
    let command = Command.Run(
        "git",
        arguments',
        options = (fun (opts: Shell.Options) ->
            opts.WorkingDirectory(workDirectory.Value)
                // NOSYSTEM here to avoid starting the FS monitor daemon that might lock the files from deletion
                .EnvironmentVariable("GIT_CONFIG_NOSYSTEM", "1")
                .EnvironmentVariable("GIT_CONFIG_GLOBAL", "") |> ignore
        )
    )
    let! result = command.Task
    if not result.Success then
        let arguments = String.concat " " arguments
        failwithf $"Error {result.ExitCode} executing command git {arguments}.\nStandard output: {result.StandardOutput}\nStandard error: {result.StandardError}"
    return result
}

let private CloneTestRepository(path: AbsolutePath, cloneUrl: string, hash: Sha1Hash)= task {
    Directory.CreateDirectory path.Value |> ignore

    // https://stackoverflow.com/a/3489576/2684760
    let! _ = RunGit(path, [| "init" |])
    let! _ = RunGit(path, [| "remote"; "add"; "origin"; cloneUrl |])
    let! _ = RunGit(path, [| "fetch"; "origin"; hash.ToString() |])
    let! _ = RunGit(path, [| "reset"; "--hard"; hash.ToString() |])
    return ()
}

let private DeleteDirectory(path: AbsolutePath) =
    Directory.EnumerateFileSystemEntries(
        path.Value,
        "*",
        EnumerationOptions(
            RecurseSubdirectories = true,
            AttributesToSkip = EnumerationOptions().AttributesToSkip ^^^ FileAttributes.Hidden
        )
    )
    |> Seq.iter(fun filePath ->
        let attributes = File.GetAttributes filePath
        if attributes.HasFlag FileAttributes.ReadOnly then
            File.SetAttributes(filePath, attributes ^^^ FileAttributes.ReadOnly)
    )

    Directory.Delete(path.Value, recursive = true)

let private CloneTestRepo (name: string) (cloneUrl: string) (hash: Sha1Hash) = task {
    try
        do! AccessMutex.WaitAsync()
        let repositoryPath = Temporary.SystemTempDirectory() / "DotNetAuthors.Tests" / name
        if not <| Directory.Exists repositoryPath.Value then
            do! CloneTestRepository(repositoryPath, cloneUrl, hash)
        else
            let! commit = Refs.ReadHead(LocalPath repositoryPath / ".git")
            let currentHash = commit |> ValueOption.ofObj |> ValueOption.map _.CommitObjectId
            if currentHash <> ValueSome hash then
                DeleteDirectory repositoryPath
                do! CloneTestRepository(repositoryPath, cloneUrl, hash)
        return repositoryPath
    finally
        AccessMutex.Release() |> ignore
}

let CloneTeamExplorerEverywhere(): Task<AbsolutePath> =
    CloneTestRepo TeamExplorerRepoName TeamExplorerRepoCloneUrl TeamExplorerRepoCommit

let CloneLogList(): Task<AbsolutePath> =
    CloneTestRepo LogListRepoName LogListRepoCloneUrl LogListRepoCommit
