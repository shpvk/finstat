# Git policy

Hosting: private GitHub repository `shpvk/finstat`, remote `origin`, operated via the `gh` CLI.
Several developers work in parallel: each task gets its own branch; never commit to, push to or rewrite another developer's branch.

## Branches
- Main branch: `Development`, exactly this case.
- Work branches start from an up-to-date `Development`, English names: `feature/expense-form`, `fix/month-filter`, `chore/project-setup`, `docs/project-decisions`.
- No work or commits directly on `Development`. Don't switch branches over unrelated uncommitted changes.
- Fetch first when a remote exists. Use a verified base; don't reset a local branch to match the remote.
- A task may continue on its existing work branch. If a branch's origin, base or ownership of changes is unclear, check Git first, then ask only what's necessary.
- Never push to `Development`: not directly, not via refspec, `--all` or `--mirror`. Push only the chosen work branch to a same-named remote branch.
- Changes reach `Development` via PR with base `Development`. Don't merge without a separate instruction.
- No force push or history rewriting in the normal flow.

## Commits
- English, one line, 1–3 words, at most 5: `Add expense form`, `Fix monthly totals`. No body, trailers or Conventional Commits prefixes.
- No `Co-authored-by`, `Generated with`, session links or any assistant mention. Use the user's existing `user.name`/`user.email`.
- Review the staged diff. Add task files by explicit path; never `git add .` or `git add -A`.
- Never include `.claude/work/`, `.claude/settings.local.json`, secrets, databases with financial data, logs or build output.
- Verify with `git log -1 --format=%B` and the commit's file list. If a hook added an unwanted body, don't publish; fix only your own unpublished commit within this task's commit permission. Never rewrite others' commits.

## PRs
Short English title. Body: problem, result, checks actually run; no agent reports or assistant attribution. Before publishing review the whole diff against `Development`, including earlier branch commits, for foreign data. After publishing verify base/head and the real PR text. Pass multi-line bodies via a temporary UTF-8 file and `--body-file`, without shell substitution.

## Commands
- `/git` with no arguments: show state and recommend the next step only.
- `/git branch <task>`: create a suitable branch; no commit/push.
- `/git commit`: local commit of the task's checked changes; no push.
- `/git publish`: push already prepared commits of the work branch and create a PR or return the existing one. Uncommitted changes are not included.
- `/git commit and publish`: both, in order, if the task already passed checks.
- Naming an operation authorizes it; don't ask again for each step.
- If Git, the base branch, a remote or PR auth is missing, say which condition is missing. Don't invent a remote, create an external repository or bypass the `Development` rule. A normal command doesn't authorize a bootstrap commit on the main branch.

These are workflow rules, not server-side branch protection. When a remote appears and the user asks for repository setup, configure branch protection; until then don't claim pushes to `Development` are technically blocked.
