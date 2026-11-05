# Shared agent memory

Important, non-obvious knowledge about this package. Use this file as your memory for this package instead of your local/auto memory.

- Add an entry when you learn something important that the code, git history or CLAUDE.md doesn't already say. Update or remove an existing entry instead of duplicating it.
- Every entry has **Rule**, **Reason**, **Example** and **Time scope** (the condition under which to rewrite or remove it). Remove or rewrite entries whose time scope has passed.
- No personal or machine-specific data (user names, home-dir paths, local tool locations).
- Entries are grouped under `## Category` headings, each entry a `### Title`. Put a new entry under the category that fits, or add a new category if none does.
- After every edit, regenerate `MEMORY.short.md` (CLAUDE.md imports it, so every character is paid on every request):
  - The same category headings, one bullet per entry: its title and the core of its Rule, at most ~120 characters. No Reason/Example/Time scope.
  - Leave out entries whose rule `CLAUDE.md` already states.
  - Situational entries (needed only in one kind of task) go under the final `## Situational (see MEMORY.md)` heading as a title-only label of at most 30 characters.

## Releasing

### Samples don't decide the version; a breaking SDK requirement does
- **Rule:** changes that only touch `Samples~` never need a major version on their own, even when their commits are `feat!:` with a `BREAKING CHANGE` footer. Raising the required Elympics SDK to a breaking SDK release (a new minor below SDK 1.0.0) is a breaking PlayPad change and needs a major. In the changelog, describe sample-only breaking notes as sample changes and make the SDK requirement the breaking note.
- **Reason:** Package Manager copies an imported sample into the game's `Assets/`, so a package update can't break a game's existing sample copy. Every PlayPad game also uses the SDK directly, so requiring a breaking SDK release forces its migrations on the game.
- **Example:** PlayPad 5.0.0 required SDK 0.30.0, while its two `feat!:` commits changed only the async game sample. 4.0.0 had listed "Require Unity 6.3 and Elympics 0.29.0" as BREAKING.
- **Time scope:** If the SDK reaches 1.0.0, rewrite the SDK part in terms of SDK major versions.
