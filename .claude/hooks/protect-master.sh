#!/usr/bin/env bash
# PreToolUse guard: block git commit/push/merge while the working tree is on
# master/main. Enforces the operator-only-merge and feature-branch-only rules
# (CLAUDE.md, "Workflow, permissions, and roles") deterministically.

input=$(cat)

# POSIX ERE only (no \b, a GNU extension): require whitespace before the verb
# and a non-verb character (or end) after it, so `git commit-graph` etc. pass.
if printf '%s' "$input" | grep -qE 'git[^"]*[[:space:]](commit|push|merge)([^a-zA-Z-]|$)'; then
  branch=$(git branch --show-current 2>/dev/null)
  if [ "$branch" = "master" ] || [ "$branch" = "main" ]; then
    echo "BLOCKED by .claude/hooks/protect-master.sh: git commit/push/merge attempted on '$branch'." >&2
    echo "Create or switch to a feature branch first (naming: <type>/s<sprint>-<slug> -- see the pr-flow skill)." >&2
    exit 2
  fi
fi

exit 0
