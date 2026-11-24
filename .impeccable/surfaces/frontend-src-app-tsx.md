---
version: 1
slug: "frontend-src-app-tsx"
primary_target: "frontend/src/App.tsx"
related_targets: []
---

# App shell and base components

## Scope

The application shell (navigation plus content area) and the base components every later screen reuses: button, text field, select, loading / empty / error states. Visitor mode: Operate. Desktop only, light theme only, Ukrainian interface.

## Audience and task

One owner at a laptop who has just paid for something and wants to record the expense within a minute. The shell must make the three sections (Огляд, Витрати, Категорії) reachable in one click and make every control trustworthy at first sight.

## Constraints

- No feature screens yet: section pages are placeholders until their own issues land.
- No comments in code or config; explanations live in README and docs.
- Money is only formatted in the interface, never calculated.

## Chosen direction

The category standard, chosen by the user over the rolled direction. Reference products for the craft bar, delegated to the implementer: Stripe Dashboard, Wise, Linear. Memorable moment: the single navigation pill gliding between sections.

## Unresolved

- Whether the owner (who asked to avoid a faceless template look) accepts the category standard.
- Charts and table composition belong to later issues.

## Direction contract

THESIS: The category standard played straight: a calm fintech workspace whose quality lives in precision. It refuses the hero-metric card mosaic, gradients and decorative icon tiles.

OWN-WORLD: White content surface, cool-grey sidebar, near-black slate text, one confident blue reserved for the primary action, the current section and focus. Hairline borders, 6px and 10px radii, Inter with tabular figures, one stroke icon set.

STORY: The owner sees where they are, reaches any section in one click and trusts every control without learning it.

FIRST VIEWPORT: 248px sidebar with the Finstat wordmark and three icon-plus-label links, the current one on a blue-tinted pill. Content: page title top-left, one-line description, then a 960px column. A section's primary action sits top-right of its header.

FORM: Canon (the standing exit), seed key de2db118. Signature interaction: one navigation pill glides between sections in 180ms, instant under reduced motion. Motion marks state changes only.

FINISH: unreviewed and undocumented is unfinished; this build ends with the finish review, the verdict, DESIGN.md, and every shipping raster carrying its provenance
