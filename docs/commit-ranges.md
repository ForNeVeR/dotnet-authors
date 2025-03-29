<!--
SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>

SPDX-License-Identifier: MIT
-->

Commit Ranges
=============
A **commit range** is a structured way to define and represent a set of Git commits.
Commit ranges are used to operate on a subset of commits in a Git repository,
allowing for specific actions such as enumeration, merging, or subtraction.
Each commit range is fully defined by the **borders**
(**top** and **bottom**, that are called **head** and **exclusive tail** below) that form the boundaries of the range.

Commit Range Structure
----------------------
A **commit range** might be written as
```json
{
    "head": [
        "502349c56ac5d76d86a9cbcfa1e57c2fda0022d0"
    ],
    "exTail": [
        "a282860c3c7bbbfc86d76ea1ec528f4050b1413e"
    ]
}
```

The fields are:
- `head` represents the most nested child commit set (descendants) that must be children of `exTail`,
- `exTail` (**exclusive tail** — meaning they are _excluded_ from the range's commit set) are the _parent_ commits of the commit set. `exTail` might include a special marker `*` that means "virtual root commit" — that is considered as a virtual root for any orphan commits.

All the commits between the `head` (inclusive) and `exTail` (exclusive) are contained in the range.

In most practical cases, `head` is a single commit.

Let's consider several examples to see why this structure looks like that.
Here, to avoid writing down long hashes, we'll use short labels like `commit-a`.
These labels are not part of the official specification (it always required full hashes), but used here for brevity.

First, let's check this graph:
```
commit-a *   * commit-b
          \ /
           * commit-c
          / \
commit-d *   * commit-e
(no further parent commits exist)
```
Here, `commit-c` is a commit that's a result of `commit-d` and `commit-e` merge,
so it has those commits as its parents; `commit-a` and `commit-b` are child commits of `commit-c`.

`commid-d` and `commit-e` do not have any parents (which might happen in a real commit graph that's been a result of merging several repositories).

If we want to define a commit range that only includes a `commit-a`, it will look like
```json
{
    "head": ["commit-a"],
    "exTail": ["commit-c"]
}
```

As `exTail` is excluded, this will result in only `commit-a` being included.

If we want to define a commit range that includes both `commit-a` and `commit-b`, then it will look like
```json
{
    "head": ["commit-a", "commit-b"],
    "exTail": ["commit-c"]
}
```

a range that defines `commit-a`, `commit-b` and `commit-c` will look like
```json
{
    "head": ["commit-a", "commit-b"],
    "exTail": ["commit-d", "commit-e"]
}
```

What if we want to include all the commits in the graph?
```json
{
    "head": ["commit-a", "commit-b"],
    "exTail": ["*"]
}
```

And only `commit-a`, `commit-c` and `commit-d`?
```json
{
    "head": ["commit-a"],
    "exTail": ["*", "commit-e"]
}
```

It works like this:
```
  commit-a * (included)
    (head)  \
             * commit-c (included)
            / \
  commit-d *   * commit-e (part of exTail, not included)
(included) |
           * (virtual root commit, part of exTail not included)
```

Commit Range Enumeration Algorithm
----------------------------------
To list all the commits in a range, you should follow this algorithm.
1. Create an empty set of all the visited commits. `set = {}`
2. Perform the following loop for each commit from the `head` set:
   1. If the `set` includes the current commit already, finish the loop.
   2. If the `exTail` set includes the current commit, finish the loop.
   3. Include the current commit into the `set`, emit it as one of the results.
   4. If the current commit has any parents, repeat the loop for each of the parents.
   5. If the current commit doesn't have any parents and `*` is not a part of the `exTail`, emit error.

This algorithm doesn't grant any particular order in the output; if order is required, then some adjustments are needed.
