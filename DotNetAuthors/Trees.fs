module internal DotNetAuthors.Trees

open System.Collections.Generic
open System.Threading.Tasks
open Fenrir.Git
open Fenrir.Git.Metadata
open TruePath

type CommitTree = {
    CommitHash: Sha1Hash
    Files: IReadOnlyDictionary<LocalPath, Sha1Hash>
}

let rec private AppendFilesFromSubTree(index: PackIndex,
                                       dotGit: LocalPath,
                                       tree: Sha1Hash,
                                       prefix: LocalPath option,
                                       result: Dictionary<_, _>): Task = task {
    let appendPathElement(x: string) = prefix |> Option.map(fun p -> p / x) |> Option.defaultWith(fun() -> LocalPath x)

    let! atoms = Trees.ReadTreeBody(index, dotGit, tree)
    for atom in atoms do
        let! header = Objects.ReadHeader(index, dotGit, atom.Hash)
        match header.Type with
        | GitObjectType.GitBlob -> result.Add(appendPathElement atom.Name, atom.Hash)
        | GitObjectType.GitTree -> do! AppendFilesFromSubTree(index, dotGit, atom.Hash, Some(appendPathElement atom.Name), result)
        | other -> failwithf $"Incorrect tree {tree}: atom {atom.Name} has kind {other}."
}

let ReadFull (index: PackIndex) (dotGit: LocalPath) (commit: Commit): Task<CommitTree> = task {
    let files = Dictionary()
    do! AppendFilesFromSubTree(index, dotGit, commit.Body.Tree, None, files)
    return {
        CommitHash = commit.Hash
        Files = files
    }
}

let Diff (prevTree: CommitTree) (nextTree: CommitTree): LocalPath seq =
    nextTree.Files
    |> Seq.filter(fun kvp ->
        let path = kvp.Key
        let nextHash = kvp.Value
        match prevTree.Files.TryGetValue path with
        | false, _ -> true
        | true, prevHash -> nextHash <> prevHash
    )
    |> Seq.map(_.Key)
