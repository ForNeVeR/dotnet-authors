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

let private TestRepositoryRemote = "https://github.com/ForNeVeR/team-explorer-everywhere.git"
let private TestRepositoryCommitSha = "5a07ff83130e12be69cc59295c81eb1f58a90c27"
let private TestRepositoryName = "team-explorer-everywhere"
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
                // NOSYSTEM here to avoid starting FS monitor daemon that might lock the files from deletion
                .EnvironmentVariable("GIT_CONFIG_NOSYSTEM", "1") |> ignore
        )
    )
    let! result = command.Task
    if not result.Success then
        let arguments = String.concat " " arguments
        failwithf $"Error {result.ExitCode} executing command git {arguments}.\nStandard output: {result.StandardOutput}\nStandard error: {result.StandardError}"
    return result
}

let private CloneTestRepository(path: AbsolutePath)= task {
    Directory.CreateDirectory path.Value |> ignore

    // https://stackoverflow.com/a/3489576/2684760
    let! _ = RunGit(path, [| "init" |])
    let! _ = RunGit(path, [| "remote"; "add"; "origin"; TestRepositoryRemote |])
    let! _ = RunGit(path, [| "fetch"; "origin"; TestRepositoryCommitSha |])
    let! _ = RunGit(path, [| "reset"; "--hard"; TestRepositoryCommitSha |])
    return ()
}

let EnsureTestRepositoryCheckedOut(): Task<AbsolutePath> = task {
    try
        do! AccessMutex.WaitAsync()
        let repositoryPath = Temporary.SystemTempDirectory() / "DotNetAuthors.Tests" / TestRepositoryName
        if not <| Directory.Exists repositoryPath.Value then
            do! CloneTestRepository(repositoryPath)
        else
            let! commit = Refs.ReadHeadRef(LocalPath repositoryPath)
            if not(isNull commit) then
                Directory.Delete(repositoryPath.Value, recursive = true)
                do! CloneTestRepository(repositoryPath)
        return repositoryPath
    finally
        AccessMutex.Release() |> ignore
}
