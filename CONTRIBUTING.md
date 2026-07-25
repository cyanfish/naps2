# Contributing Guidelines

Follow these guidelines to increase the chance of your PR being accepted.

## Atomic PRs
Each PR should be for one change only. Create separate PRs for different issues.

Don't include extraneous changes in the PR (local environment config, language .resx updates, etc.).

## Design Decisions

For UI changes, SDK API changes, or new features where there could be different ways of approaching the same problem, consider opening a [discussion](https://github.com/cyanfish/naps2/discussions/categories/development) first to confirm the approach.

## Cross-Platform Testing

NAPS2 is multi-platform (Windows, Mac, Linux). You aren't required to implement or test features on all platforms, but do consider possible platform-specific differences and say in your PR which platforms you have tested on (if relevant).