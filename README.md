<!--
SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>

SPDX-License-Identifier: MIT
-->

dotnet-authors [![Status Zero][status-zero]][andivionian-status-classifier]
==============
This is a tool for OSS projects to keep the project author list and update copyright statements in the project files whenever they are changed.

Versioning Notes
----------------
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
[reuse.spec]: https://reuse.software/spec-3.3/
[semver]: https://semver.org/spec/v2.0.0.html
[status-zero]: https://img.shields.io/badge/status-zero-lightgrey.svg
