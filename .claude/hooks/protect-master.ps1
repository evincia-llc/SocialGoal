# PreToolUse guard (PowerShell tool variant): block git commit/push/merge while
# the working tree is on master/main. Twin of protect-master.sh -- keep the
# pattern logic in sync. Enforces the operator-only-merge and
# feature-branch-only rules (CLAUDE.md, "Workflow, permissions, and roles").

$stdin = [Console]::In.ReadToEnd()

# Verb must be the first non-option token after `git` (option tokens and the
# args of -C/-c/--git-dir/--work-tree/--namespace are allowed in between).
$pattern = 'git(\s+(-[Cc]|--git-dir|--work-tree|--namespace)\s+[^\s"]+|\s+--?[^\s"]+)*\s+(commit|push|merge)([^a-zA-Z-]|$)'

if ($stdin -match $pattern) {
    $branch = (& git branch --show-current 2>$null)
    if ($branch -eq 'master' -or $branch -eq 'main') {
        [Console]::Error.WriteLine("BLOCKED by .claude/hooks/protect-master.ps1: git commit/push/merge attempted on '$branch'.")
        [Console]::Error.WriteLine("Create or switch to a feature branch first (naming: <type>/s<sprint>-<slug> -- see the pr-flow skill).")
        exit 2
    }
}

exit 0
