namespace DotNetAuthors

open System.Collections.Generic

type CommitRange(head: IReadOnlyList<Commit>, exTail: IReadOnlyList<Commit>) =
    do if head.Count = 0 then failwithf "Head commit list should not be empty."

    member _.Head: IReadOnlyList<Commit> = head
    member _.ExTail: IReadOnlyList<Commit> = exTail
