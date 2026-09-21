# Release

MultiShell uses semantic version tags such as `v0.1.0`.

A release tag runs the complete Windows acceptance suite, publishes the self-contained `MultiShell-win-x64` folder, packages `MultiShell-win-x64.zip`, and attaches that ZIP to the GitHub release for the in-app portable updater.

Do not publish a release if the native clipboard/scroll, 288-line bracketed paste, Unicode/Vietnamese input, session isolation, or CLI preset smoke checks fail.
