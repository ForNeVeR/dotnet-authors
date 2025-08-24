namespace DotNetAuthors

open System.Threading.Tasks
open TruePath

type Config =
    {
        /// Group for all the authors in the repository. Any new contributions will be assigned to this group by
        /// default.
        DefaultAuthorGroup: string
    }

    static member Read(path: AbsolutePath): Task<Config> = task {
        return failwith "TODO"
    }
