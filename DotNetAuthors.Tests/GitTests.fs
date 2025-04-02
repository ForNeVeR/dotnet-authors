module DotNetAuthors.Tests.GitTests

open System
open System.Globalization
open DotNetAuthors.Git
open Fenrir.Git
open Fenrir.Git.Metadata
open Xunit

[<Theory>]
[<InlineData("author Microsoft GitHub User <msftgits@microsoft.com> 1455909603 -0800", "2016-02-19T11:20:03-0800")>]
let rec ``GetAuthorDate correctness``(line: string, expectedOffset: string): unit =
    let commit = {
        Hash = Sha1Hash.Zero
        Body = {
            Parents = Array.empty
            Tree = Sha1Hash.Zero
            Rest = [| line |]
        }
    }
    let expectedTime = DateTimeOffset.ParseExact(expectedOffset, "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture)
    let parsedTime = GetAuthorDate commit
    Assert.Equal(expectedTime, parsedTime)
    Assert.Equal(expectedTime.Offset, parsedTime.Offset)
