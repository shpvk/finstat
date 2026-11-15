---
description: Manage branches, short commits and pull requests targeting Development
argument-hint: "[status | branch <task> | commit | publish | commit and publish]"
disable-model-invocation: true
---

Request: $ARGUMENTS

Delegate to `git-manager`, stating exactly which operations are allowed (see "Commands" in `.claude/git-policy.md`) and the known task scope. Empty request = read-only status. Never widen commit to push, or publish to commit or merge.

If check evidence is missing or stale, run the relevant checks before commit/publish. For docs-only changes reviewing the diff and format is enough; don't invent build commands.

Return only what really exists: branch, commit hash, PR URL, checks run, remaining limits. If the agent is unavailable, say so and run the same process in the main session without claiming a subagent ran.
