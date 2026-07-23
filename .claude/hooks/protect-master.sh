#!/usr/bin/env bash
# PreToolUse guard: block git commit/push/merge while the working tree is on
# master/main. Enforces the operator-only-merge and feature-branch-only rules
# (CLAUDE.md, "Workflow, permissions, and roles") deterministically.

input=$(cat)

if printf '%s' "$input" | grep -qE 'git[^"]*\b(commit|push|merge)\b'; then
  branch=$(git branch --show-current 2>/dev/null)
  if [ "$branch" = "master" ] || [ "$branch" = "main" ]; then
    echo "BLOCKED by .claude/hooks/protect-master.sh: git commit/push/merge attempted on '$branch'." >&2
    echo "Create or switch to a feature branch first (naming: <type>/s<sprint>-<slug> -- see the pr-flow skill)." >&2
    exit 2
  fi
fi

exit 0
