# Issue tracker: GitHub

Issues and specs for this repository live as GitHub Issues in
`kaiflacco/HomeServeIT`. Use the `gh` CLI for operations.

## Conventions

- Create an issue with `gh issue create --title "..." --body "..."`.
- Read an issue with `gh issue view <number> --comments`.
- List issues with `gh issue list`.
- Comment with `gh issue comment <number> --body "..."`.
- Apply labels with `gh issue edit <number> --add-label "..."`.
- Close issues with `gh issue close <number>`.
- Infer the repository from the configured Git remote.

## Pull requests as a triage surface

PRs as a request surface: no.

## When a skill says "publish to the issue tracker"

Create a GitHub issue.

## When a skill says "fetch the relevant ticket"

Run `gh issue view <number> --comments`.
