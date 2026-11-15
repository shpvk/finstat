# Working rules

Reply in the user's language (Russian by default). Several developers share this repository and work through the same instructions. The app UI is Ukrainian; code identifiers, branch names and commits are English.

## Sources
- `docs/business-decisions.md` (Russian): agreed scenarios and MVP scope. Read before product or UI work.
- `docs/technical-decisions.md` (Russian): stack, DDD layering, money/date rules, checks. Read before code work.
- `.claude/git-policy.md`: read only for Git operations.

Read only the sections relevant to the task. The user's explicit current instruction overrides these documents. Update them when a decision changes; never record your own assumption as the user's answer.

## Autonomy
- Carry a task through implementation, review and verification; don't stop at a plan or the first working version.
- Choose ordinary technical details yourself within the architecture, existing dependencies and project scripts.
- Ask about business ambiguity that changes behaviour (e.g. future-dated expenses, recategorising an expense in an archived category). Batch questions and keep doing independent work meanwhile.
- Get separate approval for major stack changes, public exposure, sending financial data outside, destructive migrations. Don't re-ask once granted.
- No unagreed features. Don't complicate the small MVP for the sake of patterns.
- Preserve others' changes: check the working tree first. No reset, clean, stash, data deletion or history rewriting for convenience.
- Don't bypass tool or permission limits; report the actual limit instead of presenting unverified results as done.

## Workflow
- `/do <task>`: the main session coordinates `developer` → `reviewer` → `tester` for every task and returns defects to the developer (see `.claude/commands/do.md`).
- `/git <operation>`: delegates to `git-manager`.
- Brief subagents with the task, acceptance criteria, the relevant decisions (quoted), file scope and previous-stage results, so they don't reread whole documents. They don't run orchestration commands.
- No parallel edits of the same files; reviewers and testers work on the finished version.
- `/do` never commits, pushes, opens PRs or merges.
- No assistant attribution anywhere: sources, product docs, commits, PRs. Don't change the user's Git identity.
- This file and `.claude/` (agents, commands, settings, Git policy) are part of the repository and are committed like code. Only `.claude/settings.local.json` and `.claude/work/` stay local.

## Verification and reporting
Run checks matching the change plus the project's required commands. Add useful regression tests for behaviour; no dummy tests for text edits. Verify UI in a browser when tools allow. Re-run passing checks only after changes or on a concrete doubt.

Finish briefly: what changed, what was actually verified, what remains blocked. Never call a task verified if a required check didn't run. For long tasks keep decisions and progress in `.claude/work/`, without secrets or user financial data.
