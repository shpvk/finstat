---
description: Implement a task through developer, independent review and testing
argument-hint: "<task description>"
disable-model-invocation: true
---

Task: $ARGUMENTS

You are the coordinator in the main session. Call each agent yourself; don't hand this whole command to one subagent.

If the task is a GitHub issue (`#N`), read it with `gh issue view N`; its acceptance criteria are the starting criteria, and its open questions must be settled with the user first.

## 1. Prepare
1. Read the relevant sections of `docs/*-decisions.md`, the existing code and project scripts. If a repo exists, check `git status`, record pre-existing user changes and keep them out of scope. Without Git, edit files but don't claim branches or history.
2. If a repo exists, ensure a work branch from `Development` per `.claude/git-policy.md` (`git-manager` may create it; no commit/push/PR). Never change code on `Development`.
3. Define observable acceptance criteria, affected scenarios and required checks. Decide technical details yourself; settle business ambiguity with the user before dependent work.
4. Write a short card `.claude/work/<task-name>.md`: request, criteria, decisions, initial state, file scope, check plan, current stage. Update it at each stage.

## 2. Develop
Call `developer` with the brief: task, criteria, quoted relevant decisions, file scope, constraints. Several developers only on disjoint files; you integrate. The developer's report alone doesn't close the task.

## 3. Review
Call a fresh `reviewer` with: criteria, decisions, the list of changed and new files, the developer's report. Return required fixes to `developer`, then re-review the changed parts and what they touch. Don't hand fixable technical issues to the user.

## 4. Test
Call a fresh `tester` with: criteria, final changed files, review findings and their fixes, project commands, the developer's report. Defects go back to `developer`. Any later production change is re-reviewed, then the affected scenarios are re-tested. Tests the tester adds also go through `reviewer`.

## 5. Done
Done when criteria are met, required findings are closed, the final changes passed independent review and the relevant checks actually passed; for UI, the running interface was viewed if tools allow.
- On tool or environment failure, fix ordinary local causes and continue independent checks. If a required check is impossible, the result is "implemented, verification blocked" with the exact cause and the minimal action to unblock.
- If subagents are unavailable, don't imitate independent review in your own role; report the cycle as incomplete.
- No fixed iteration cap: stop when done or on a real external blocker. On repeated failure change the approach.
- Record final status and evidence in the card; update the decision docs if new agreements were made.
- Reply: what changed, review outcome, checks run, limitations. No commit, push, PR or merge: the user runs `/git`.
