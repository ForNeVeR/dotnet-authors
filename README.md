<!--
SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>

SPDX-License-Identifier: MIT
-->

dotnet-authors [![Status Zero][status-zero]][andivionian-status-classifier] [![NuGet package][nuget.badge]][nuget.page]
==============
Supporting an open-source project means fulfilling some tasks that are not always pleasant.

One of the less pleasant tasks is maintaining accurate copyright statements and contributor lists across project files.
As projects grow, manually updating copyright years and tracking all contributors becomes time-consuming and
error-prone. This tool aims to automate these maintenance tasks by helping project maintainers keep copyright statements
up-to-date and maintain accurate lists of project contributors.

It will allow to perform hassle-free, asynchronous updates of all the related metadata, and send pull-requests when something requires changes.

Installation
------------
Install as a dotnet tool: either
```console
$ dotnet tool install --global FVNever.DotNetAuthors
```
for global installation or
```console
$ dotnet new tool-manifest
$ dotnet tool install FVNever.DotNetAuthors
```
for local solution-wide installation.

Usage
-----
After installation, the tool will be available in shell as `dotnet authors`.

For now, its only function is to exit with zero code. More functions will be available later.

Versioning Policy
-----------------
This project's versioning follows the [Semantic Versioning 2.0.0][semver] specification.

When considering compatible changes, we currently consider the project's public API is the command-line interface:
- the way of running the project (e.g., the executable file name),
- the input arguments,
- the input data formats,
- the output data format,
- the program exit code.

Documentation
-------------
- [Changelog][docs.changelog]
- [Contributor Guide][docs.contributing]
- [Maintainer Guide][docs.maintaining]

License
-------
The project is distributed under the terms of [the MIT license][docs.license].

The license indication in the project's sources is compliant with the [REUSE specification v3.3][reuse.spec].

[andivionian-status-classifier]: https://andivionian.fornever.me/v1/#status-zero-
[docs.changelog]: CHANGELOG.md
[docs.contributing]: CONTRIBUTING.md
[docs.license]: LICENSE.txt
[docs.maintaining]: MAINTAINING.md
[nuget.badge]: https://img.shields.io/nuget/v/FVNever.DotNetAuthors
[nuget.page]: https://www.nuget.org/packages/FVNever.DotNetAuthors
[reuse.spec]: https://reuse.software/spec-3.3/
[semver]: https://semver.org/spec/v2.0.0.html
[status-zero]: https://img.shields.io/badge/status-zero-lightgrey.svg
