# Contribution Guidelines

**Note:** If these contribution guidelines are not followed your issue or PR might be closed, so
please read these instructions carefully.

## Contribution types

### Bug Reports

- If you find a bug, please first report it using [GitHub issues].
  - First check if there is not already an issue for it; duplicated issues will be closed.

### Bug Fix

- If you'd like to submit a fix for a bug, please read the [How-To](#how-to-contribute) for how to
   send a Pull Request.
- Indicate on the open issue that you are working on fixing the bug and the issue will be assigned
   to you.
- Write `Fixes #xxxx` in your PR text, where xxxx is the issue number (if there is one).
- Include a test that isolates the bug and verifies that it was fixed.

### New Features

- If you'd like to add a feature to the library that does not already exist, feel free to describe
   the feature in a new [GitHub issue].
- If you'd like to implement the new feature, please wait for feedback from the project maintainers
   before spending too much time writing the code. In some cases, enhancements may not align well
   with the project's future development direction.
- Implement the code for the new feature and please read the [How-To](#how-to-contribute).

### Documentation & Miscellaneous

- If you have suggestions for improvements to the documentation, tutorial or examples (or something
   else), we would love to hear about it.
- As always first file a [GitHub issue].
- Implement the changes to the documentation, please read the [How-To](#how-to-contribute).

## How To Contribute

### Requirements

For a contribution to be accepted:

- Format the code using
```
dotnet tool restore
dotnet fantomas -r src
```
- Make sure that the Fantomas [Oak](https://fsprojects.github.io/fantomas-tools/#/oak?data=N4KABGBEDGD2AmBTSAuKAbRAXMAPMAvGAIwAMkANOFEgGYCWAdogM6pSXWT0sBiL9drQCG6FoioRuLAOIAnYQAcAFgDV6iAO5DR4kAF8gA) model supports the code you are trying to generate.
- Check that all tests pass: `dotnet test`;
- Documentation should always be updated or added (if applicable);
- Examples should always be updated or added (if applicable);
- Tests should always be updated or added (if applicable).

If the contribution doesn't meet these criteria, a maintainer will discuss it with you on the issue
or PR.
You can still continue to add more commits to the branch you have sent the Pull Request from, 
and it will be automatically reflected in the PR.


## Open an issue and fork the repository

- If it is a bigger change or a new feature, first of all
   [file a bug or feature report][GitHub issue], so that we can discuss what direction to follow.
- [Fork the project][fork guide] on GitHub.
- Clone the forked repository to your local development machine
   (e.g. `git clone git@github.com:<YOUR_GITHUB_USER>/Fabulous.AST.git`).


### Environment Setup

To build Fabulous.AST, you will need to install:
- the [.NET 8.0 SDK]
- an IDE of your preference
- Open the Fabulous.AST.sln ith your preferred IDE and build the solution or execute one of the following commands:
```shell
dotnet build Fabulous.AST.sln
```

or

```shell
dotnet fsi build.fsx
```

### Performing changes

- Create a new local branch from `main` (e.g. `git checkout -b my-new-feature`)
- Make your changes (try to split them up with one PR per feature/fix).
- When committing your changes, make sure that each commit message is clear.
- Push your new branch to your own fork into the same remote branch
 (e.g. `git push origin my-username.my-new-feature`, replace `origin` if you use another remote.)


### Open a pull request

Go to the [pull request page of Fabulous.AST][PRs] and at the top
of the page it will ask you if you want to open a pull request from your newly created branch.

The title of the pull request should be descriptive of the work you did.


[GitHub issue]: https://github.com/edgarfgp/Fabulous.AST/issues
[GitHub issues]: https://github.com/edgarfgp/Fabulous.AST/issues
[GitHub releases page]: https://github.com/edgarfgp/Fabulous.AST/releases
[PRs]: https://github.com/edgarfgp/Fabulous.AST/pulls
[fork guide]: https://docs.github.com/en/get-started/quickstart/contributing-to-projects
[.NET 8.0 SDK]: https://dotnet.microsoft.com/en-us/download
[build workflow]: .github/workflows/build.yml
[CHANGELOG.md]: CHANGELOG.md
[JetBrains Rider]: https://www.jetbrains.com/rider/
