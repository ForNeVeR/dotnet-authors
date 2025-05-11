namespace DotNetAuthors

open System.Collections.Generic
open System.Threading.Tasks
open FSharp.Control
open Fenrir.Git.Metadata

type ComplexCommitRange =
    { Included: IReadOnlyList<CommitRange>
      Excluded: IReadOnlyList<CommitRange> }

    /// Normalizes the range.
    member _.Normalize(): Task<ComplexCommitRange> = failwithf "TODO"

    member _.ApplyToRepository(): AsyncSeq<Commit> = failwithf "TODO"
