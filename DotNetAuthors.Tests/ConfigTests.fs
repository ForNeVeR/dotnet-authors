// SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module DotNetAuthors.Tests.ConfigTests

open System.Threading.Tasks
open DotNetAuthors
open TruePath
open TruePath.SystemIo
open Xunit

[<Fact>]
let ``Config.Read reads defaultAuthorGroup from YAML successfully``(): Task = task {
    let file = Temporary.CreateTempFile()
    let yaml = """
# Example configuration for dotnet-authors
# Expectation: the key name is camelCase in YAML
defaultAuthorGroup: "Example contributors <https://example.com/project>"
"""
    do! file.WriteAllTextAsync yaml
    let! config = Config.Read file
    Assert.Equal("Example contributors <https://example.com/project>", config.DefaultAuthorGroup)
}

[<Fact>]
let ``Config.Read fails when defaultAuthorGroup is missing``(): Task = task {
    let file = Temporary.CreateTempFile()
    let yaml = """
# Invalid configuration: required field is missing
someOtherField: "value"
"""
    do! file.WriteAllTextAsync yaml
    let! ex = Assert.ThrowsAsync(fun () -> Config.Read file)
    Assert.Contains("defaultAuthorGroup is missing", ex.Message)
}
