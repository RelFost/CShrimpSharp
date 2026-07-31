$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
Push-Location $repositoryRoot

try {
    if (-not (Test-Path '.git')) {
        git init
        if ($LASTEXITCODE -ne 0) { throw 'git init failed.' }
    }

    git branch -M main
    if ($LASTEXITCODE -ne 0) { throw 'Unable to set the main branch.' }

    $null = git remote get-url origin 2>$null
    if ($LASTEXITCODE -eq 0) {
        git remote set-url origin https://github.com/RelFost/CShrimpSharp.git
    }
    else {
        git remote add origin https://github.com/RelFost/CShrimpSharp.git
    }

    if ($LASTEXITCODE -ne 0) { throw 'Unable to configure the origin remote.' }

    git add .
    if ($LASTEXITCODE -ne 0) { throw 'git add failed.' }

    git diff --cached --quiet
    if ($LASTEXITCODE -ne 0) {
        git commit -m 'feat: initialize CShrimpSharp'
        if ($LASTEXITCODE -ne 0) { throw 'git commit failed.' }
    }

    git push -u origin main
    if ($LASTEXITCODE -ne 0) { throw 'git push failed.' }
}
finally {
    Pop-Location
}
