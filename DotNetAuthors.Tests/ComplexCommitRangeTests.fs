module DotNetAuthors.Tests.ComplexCommitRangeTests

open Xunit

[<Fact>]
let ``Disjoined inclusion test``(): unit =
    let commitRange = {
        Included = [| {
            Head = [| Sha1Hash.OfHexString "f27e263cd1cc43bdd860ecc0b190179d7913eb2b" |]
            ExTail = [| VirtualRoot |]
        }; {
            Head = [| Sha1Hash.OfHexString "546d046cde6786c8cd0cb9dce5db62488c580afd" |]
            ExTail = [| Sha1Hash.OfHexString "06e7f86f27403603e2eeb63ebf4a00778c8a1791" |]
        } |]
        Excluded = Array.empty
    }

    let normalized = commitRange.Normalize()
    Assert.Equal(commitRange, normalized)
    Assert.Equal(normalized.ApplyToRepository().Count, failwith "TODO")

[<Fact>]
let ``Conjoined inclusion test``(): unit =
    let commitRange = {
        Included = [| {
            Head = [| Sha1Hash.OfHexString "f27e263cd1cc43bdd860ecc0b190179d7913eb2b" |]
            ExTail = [| VirtualRoot |]
        }; {
            Head = [| Sha1Hash.OfHexString "546d046cde6786c8cd0cb9dce5db62488c580afd" |]
            ExTail = [| Sha1Hash.OfHexString "f27e263cd1cc43bdd860ecc0b190179d7913eb2b" |]
        } |]
        Excluded = Array.empty
    }

    let normalized = commitRange.Normalize()
    Assert.Equal({
        Included = [| {
            Head = [| Sha1Hash.OfHexString "546d046cde6786c8cd0cb9dce5db62488c580afd" |]
            ExTail = [| VirtualRoot |]
        } |]
        Excluded = Array.empty
    }, normalized)
    Assert.Equal(normalized.ApplyToRepository().Count, failwith "TODO")

[<Fact>]
let ``Overlapping inclusion test``(): unit =
    let commitRange = {
        Included = [| {
            Head = [| Sha1Hash.OfHexString "d656f5518928aeed849c294a5d64ebc82abecdb2" |]
            ExTail = [| VirtualRoot |]
        }; {
            Head = [| Sha1Hash.OfHexString "97bf4b57c9ae0a684710e23dbedfecbad3e9d8c1" |]
            ExTail = [| Sha1Hash.OfHexString "21a50983293f9fb16e55012d90cba81825259925" |]
        } |]
        Excluded = Array.empty
    }

    let normalized = commitRange.Normalize()
    Assert.Equal({
        Included = [| {
            Head = [| Sha1Hash.OfHexString "97bf4b57c9ae0a684710e23dbedfecbad3e9d8c1" |]
            ExTail = [| VirtualRoot |]
        } |]
        Excluded = Array.empty
    }, normalized)
    Assert.Equal(normalized.ApplyToRepository().Count, failwith "TODO")

[<Fact>]
let ``Zero exclusion test``(): unit =
    let commitRange = {
        Included = [| {
            Head = [| Sha1Hash.OfHexString "21a50983293f9fb16e55012d90cba81825259925" |]
            ExTail = [| Sha1Hash.OfHexString "f27e263cd1cc43bdd860ecc0b190179d7913eb2b" |]
        } |]
        Excluded = [| {
            Head = [| Sha1Hash.OfHexString "367c1cd310239fc4c86786ec71d6281c152968cb" |]
            ExTail = [| VirtualRoot |]
        } |]
    }

    let normalized = commitRange.Normalize()
    Assert.Equal({
        Included = [| {
            Head = [| Sha1Hash.OfHexString "21a50983293f9fb16e55012d90cba81825259925" |]
            ExTail = [| Sha1Hash.OfHexString "f27e263cd1cc43bdd860ecc0b190179d7913eb2b" |]
        } |]
        Excluded = Array.empty
    }, normalized)
    Assert.Equal(normalized.ApplyToRepository().Count, failwith "TODO")

[<Fact>]
let ``Partial exclusion test``(): unit =
    let commitRange = {
        Included = [| {
            Head = [| Sha1Hash.OfHexString "2d921038f77c25caed664db9e29261fb11e9004f" |]
            ExTail = [| Sha1Hash.OfHexString "f27e263cd1cc43bdd860ecc0b190179d7913eb2b" |]
        } |]
        Excluded = [| {
            Head = [| Sha1Hash.OfHexString "48402e146de416fcf6650baf6fa4e94f6cfe58cf" |]
            ExTail = [| VirtualRoot |]
        } |]
    }

    let normalized = commitRange.Normalize()
    Assert.Equal({
        Included = [| {
            Head = [| Sha1Hash.OfHexString "2d921038f77c25caed664db9e29261fb11e9004f" |]
            ExTail = [| Sha1Hash.OfHexString "48402e146de416fcf6650baf6fa4e94f6cfe58cf" |]
        } |]
        Excluded = Array.empty
    }, normalized)
    Assert.Equal(normalized.ApplyToRepository().Count, failwith "TODO")

[<Fact>]
let ``Enclave exclusion test``(): unit =
    let commitRange = {
        Included = [| {
            Head = [| Sha1Hash.OfHexString "2d921038f77c25caed664db9e29261fb11e9004f" |]
            ExTail = [| VirtualRoot |]
        } |]
        Excluded = [| {
            Head = [| Sha1Hash.OfHexString "48402e146de416fcf6650baf6fa4e94f6cfe58cf" |]
            ExTail = [| Sha1Hash.OfHexString "21a50983293f9fb16e55012d90cba81825259925" |]
        } |]
    }

    let normalized = commitRange.Normalize()
    Assert.Equal({
        Included = [| {
            Head = [| Sha1Hash.OfHexString "2d921038f77c25caed664db9e29261fb11e9004f" |]
            ExTail = [| Sha1Hash.OfHexString "48402e146de416fcf6650baf6fa4e94f6cfe58cf" |]
        }; {
            Head = [| Sha1Hash.OfHexString "21a50983293f9fb16e55012d90cba81825259925" |]
            ExTail = [| VirtualRoot |]
        }|]
        Excluded = Array.empty
    }, normalized)
    Assert.Equal(normalized.ApplyToRepository().Count, failwith "TODO")

[<Fact>]
let ``Subtree exclusion test``(): unit =
    let commitRange = {
        Included = [| {
            Head = [| Sha1Hash.OfHexString "5a07ff83130e12be69cc59295c81eb1f58a90c27" |]
            ExTail = [| VirtualRoot |]
        } |]
        Excluded = [| {
            Head = [| Sha1Hash.OfHexString "7db34c084de9edca97c6880f1311ea70da9bb8b7" |]
            ExTail = [| VirtualRoot |]
        } |]
    }

    let normalized = commitRange.Normalize()
    Assert.Equal({
        Included = [| {
            Head = [| Sha1Hash.OfHexString "5a07ff83130e12be69cc59295c81eb1f58a90c27" |]
            ExTail = [| VirtualRoot; Sha1Hash.OfHexString "7db34c084de9edca97c6880f1311ea70da9bb8b7" |]
        } |]
        Excluded = Array.empty
    }, normalized)
    Assert.Equal(normalized.ApplyToRepository().Count, failwith "TODO")
