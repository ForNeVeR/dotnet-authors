// SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

open System.Reflection
open DotNetAuthors
open Fenrir.Git
open TruePath

let private printVersion() =
    let version = Assembly.GetExecutingAssembly().GetName().Version |> nonNull
    printfn $"dotnet-authors v{version}"

let private printUsage() =
    printVersion()
    printfn "\nAlways run the program from the repository root."
    printfn "\nUsage:"
    printfn "  --version - print the program version."
    printfn "  --help - print this message."
    printfn "  authors [file] - print authors contributed to a file (or all files in repo by default)."
    printfn "  commits [file] - print commits contributed to a file (or all files in repo by default)."

let private printFilteredCommits filter =
    (task {
         let repository = Git.Repository AbsolutePath.CurrentWorkingDirectory
         let! headCommit = Refs.ReadHead repository.DotGit
         let! fileContributors = Git.GetCommitsPerFile repository (nonNull headCommit).CommitObjectId
         fileContributors
         |> Seq.sortBy _.Key.Value
         |> Seq.filter(fun kvp -> filter kvp.Key)
         |> Seq.iter(fun kvp ->
             let path = kvp.Key
             let commits = String.concat ", " (Seq.map string kvp.Value)
             printfn $"{path}: {commits}"
         )
    }).Wait()

let private repoRelativeFileExists(path: LocalPath) =
    (AbsolutePath.CurrentWorkingDirectory / path).ReadKind().HasValue

let private printCommitsForAllFiles() =
    printFilteredCommits repoRelativeFileExists

let private printCommits filePath =
    printFilteredCommits(fun path -> path = filePath)

let private printFilteredAuthors filter =
    (task {
         let repository = Git.Repository AbsolutePath.CurrentWorkingDirectory
         let! headCommit = Refs.ReadHead repository.DotGit
         let! fileAuthors = Git.GetAuthorsPerFile repository (nonNull headCommit).CommitObjectId
         fileAuthors
         |> Seq.sortBy _.Key.Value
         |> Seq.filter(fun kvp -> filter kvp.Key)
         |> Seq.iter(fun kvp ->
             let path = kvp.Key
             let authors = String.concat ", " (Seq.map string kvp.Value)
             printfn $"{path}: {authors}"
         )
    }).Wait()

let private printAuthorsForAllFiles() =
    printFilteredAuthors repoRelativeFileExists

let private printAuthors filePath =
    printFilteredAuthors(fun path -> path = filePath)

[<EntryPoint>]
let main(args: string[]): int =
    match args with
    | [|"commits"|] -> printCommitsForAllFiles(); 0
    | [|"commits"; file|] -> printAuthors(LocalPath file); 0
    | [|"authors"|] -> printAuthorsForAllFiles(); 0
    | [|"authors"; file|] -> printCommits(LocalPath file); 0
    | [|"--version"|] -> printVersion(); 0
    | [|"--help"|] -> printUsage(); 0
    | _ -> printUsage(); 1
