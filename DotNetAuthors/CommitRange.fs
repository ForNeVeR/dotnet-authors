namespace DotNetAuthors

open System.Collections.Generic
open DotNetAuthors.Git
open FSharp.Control
open Fenrir.Git
open Fenrir.Git.Metadata
open JetBrains.Lifetimes
open TruePath

[<Struct>]
type CommitRangeBorder =
    | Sha1 of Sha1Hash
    /// A virtual root for any orphaned commits.
    | VirtualRoot

type CommitRange(head: IReadOnlyList<Sha1Hash>, exTail: Set<CommitRangeBorder>) =
    do if head.Count = 0 then failwithf "Head commit list should not be empty."
    do if exTail.Count = 0 then failwithf "Exclusive tail commit set should not be empty." // TODO: test

    let byAuthorDate = Comparer.Create(fun commitA commitB -> compare(GetAuthorDate commitA) (GetAuthorDate commitB))

    member _.ApplyToRepository(repoRoot: AbsolutePath): AsyncSeq<Commit> =
        asyncSeq {
            use ld = new LifetimeDefinition()
            let lt = ld.Lifetime
            let gitDir = repoRoot / ".git"
            let index = PackIndex(lt, gitDir)

            let readCommit hash = Async.AwaitTask <| Commits.ReadCommit(index, LocalPath gitDir, hash)

            let headCommits = SortedSet byAuthorDate // TODO: Figure out if this maintains stable sort order.
                                                     // TODO: Most likely this will break on duplicate dates.
                                                     // TODO: Invent an order to stably sort duplicates somehow.
            do!
                head
                |> AsyncSeq.ofSeq
                |> AsyncSeq.mapAsync readCommit
                |> AsyncSeq.iter(fun x -> headCommits.Add x |> ignore)

            while headCommits.Count <> 0 do
                let topCommit = Seq.head headCommits
                headCommits.Remove topCommit |> ignore
                yield topCommit

                let! parents =
                    topCommit.Body.Parents
                    |> AsyncSeq.ofSeq
                    |> AsyncSeq.mapAsync readCommit
                    |> AsyncSeq.toArrayAsync

                if Array.isEmpty parents && not(exTail.Contains(VirtualRoot)) then
                    failwithf $"Commit {topCommit.Hash} has no parents and exclusive tail commit list doesn't allow root commits."
                     // TODO: test this

                parents |> Array.iter(fun x -> headCommits.Add x |> ignore)
        }
