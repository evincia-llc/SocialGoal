#!/usr/bin/env bash
# PreToolUse guard: block git commit/push/merge while the working tree is on
# master/main. Enforces the operator-only-merge and feature-branch-only rules
# (CLAUDE.md, "Workflow, permissions, and roles") deterministically.

input=$(cat)

# POSIX ERE only (no \b, a GNU extension). The verb must be the first
# non-option token after `git` (allowing option tokens and args for
# -C/-c/--git-dir/--work-tree/--namespace), so read-only commands that merely
# mention a verb as an argument (`git log --grep commit`) are not blocked,
# and `git commit-graph` etc. pass. Keep in sync with protect-master.ps1.
if printf '%s' "$input" | grep -qE 'git([[:space:]]+(-[Cc]|--git-dir|--work-tree|--namespace)[[:space:]]+[^[:space:]"]+|[[:space:]]+--?[^[:space:]"]+)*[[:space:]]+(commit|push|merge)([^a-zA-Z-]|$)'; then
  branch=$(git branch --show-current 2>/dev/null)
  if [ "$branch" = "master" ] || [ "$branch" = "main" ]; then
    echo "BLOCKED by .claude/hooks/protect-master.sh: git commit/push/merge attempted on '$branch'." >&2
    echo "Create or switch to a feature branch first (naming: <type>/s<sprint>-<slug> -- see the pr-flow skill)." >&2
    exit 2
  fi
fi

exit 0
