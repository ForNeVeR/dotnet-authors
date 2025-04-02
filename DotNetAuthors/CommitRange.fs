namespace DotNetAuthors

open System.Collections.Generic
open Fenrir.Git

[<Struct>]
type CommitRangeBorder =
    | Sha1 of Sha1Hash
    /// A virtual root for any orphaned commits.
    | VirtualRoot

type CommitRange(head: IReadOnlyList<Sha1Hash>, exTail: IReadOnlyList<CommitRangeBorder>) =
    do if head.Count = 0 then failwithf "Head commit list should not be empty."

    member _.Head: IReadOnlyList<Sha1Hash> = head
    member _.ExTail: IReadOnlyList<CommitRangeBorder> = exTail
