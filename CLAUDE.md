# ShinyPDF

Read [AGENTS.md](AGENTS.md) first. All project rules, commands, and workflow live there; this file holds only what is Claude-specific.

## Claude-specific rules

- No em-dashes in any generated text, code comments, or commit messages.
- This repo is on GitHub: use `gh`, not `az repos` / `az boards`.
- Before claiming done or fixed: verify the actual result (tests run, package packed, release visible), or say "UNVERIFIED". A green build is not verification.
- On Windows, PowerShell 5.1 writes UTF-8 with BOM by default; preserve the existing encoding of files you edit.
