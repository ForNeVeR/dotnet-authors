namespace DotNetAuthors

open System.Threading.Tasks
open TruePath
open TruePath.SystemIo
open YamlDotNet.Serialization
open YamlDotNet.Serialization.NamingConventions

// DTO for YAML deserialization
[<AllowNullLiteral>]
type ConfigDto() =
    member val DefaultAuthorGroup = null : string | null with get, set

type Config =
    {
        /// Group for all the authors in the repository. Any new contributions will be assigned to this group by
        /// default.
        DefaultAuthorGroup: string
    }
    static member Read(path: AbsolutePath): Task<Config> = task {
        let! text = path.ReadAllTextAsync()
        let deserializer =
            DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build()

        let parsed = deserializer.Deserialize<ConfigDto>(text)
        return {
            DefaultAuthorGroup =
                parsed.DefaultAuthorGroup
                |> ValueOption.ofObj
                |> ValueOption.defaultWith(fun() -> failwith "defaultAuthorGroup is missing")
        }
    }
