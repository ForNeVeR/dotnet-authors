<!--
SPDX-FileCopyrightText: 2024-2025 Friedrich von Never <friedrich@fornever.me>

SPDX-License-Identifier: MIT
-->

Maintainer Guide
================

Publish a New Version
---------------------
1. Choose the new version according the project's versioning scheme.
2. Update the project's status in the `README.md` file, if required.
3. Update the copyright statement in the `LICENSE.txt` file, if required.
4. Update the `<Copyright>` statement and `<PackageLicenseExpression>` field in the `Directory.Build.props`, if required.
5. Update the `<Version>` in `Directory.Build.props`.
6. Prepare a corresponding entry in the `CHANGELOG.md` file (usually by renaming the "Unreleased" section).
7. Merge the aforementioned changes via a pull request.
8. Push a tag in form of `v<VERSION>`, e.g. `v0.0.0`.
