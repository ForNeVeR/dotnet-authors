// SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module DotNetAuthors.Tests.TestFramework

open System.IO
open System.Threading
open System.Threading.Tasks
open DotNetAuthors
open Medallion.Shell
open TruePath

let private TestRepositoryRemote = "https://github.com/ForNeVeR/team-explorer-everywhere.git"
let private TestRepositoryCommitSha = "5a07ff83130e12be69cc59295c81eb1f58a90c27"
// let private TestRepositoryCommitSha = "06e7f86f27403603e2eeb63ebf4a00778c8a1791"
let private TestRepositoryName = "team-explorer-everywhere"
let private AccessMutex = new SemaphoreSlim(1)

let private RunGit(workDirectory: AbsolutePath, arguments: string[]) = task {
    let arguments' = arguments |> Seq.map (fun x -> x :> obj)
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
}

let private CloneTestRepository(path: AbsolutePath)= task {
    Directory.CreateDirectory path.Value |> ignore

    // https://stackoverflow.com/a/3489576/2684760
    do! RunGit(path, [| "init" |])
    do! RunGit(path, [| "remote"; "add"; "origin"; TestRepositoryRemote |])
    do! RunGit(path, [| "fetch"; "origin"; TestRepositoryCommitSha |])
    do! RunGit(path, [| "reset"; "--hard"; TestRepositoryCommitSha |])
}

let EnsureTestRepositoryCheckedOut(): Task<AbsolutePath> = task {
    try
        do! AccessMutex.WaitAsync()
        let repositoryPath = Temporary.SystemTempDirectory() / "DotNetAuthors.Tests" / TestRepositoryName
        if not <| Directory.Exists repositoryPath.Value then
            do! CloneTestRepository(repositoryPath)
        else
            let! commit = Git.GetHeadCommit repositoryPath
            if commit <> Some TestRepositoryCommitSha then
                Directory.Delete(repositoryPath.Value, recursive = true)
                do! CloneTestRepository(repositoryPath)
        return repositoryPath
    finally
        AccessMutex.Release() |> ignore
}
