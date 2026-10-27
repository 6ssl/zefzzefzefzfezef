$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Write-HookResult([string]$Message) {
    $result = @{ continue = $true }
    if ($Message) {
        $result.systemMessage = $Message
    }

    [Console]::Out.WriteLine(($result | ConvertTo-Json -Compress))
}

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
Set-Location $repositoryRoot

$changes = & git status --porcelain --untracked-files=all
if ($LASTEXITCODE -ne 0) {
    Write-HookResult 'Automatic commit skipped because Git status could not be read.'
    exit 0
}

if ([string]::IsNullOrWhiteSpace(($changes -join "`n"))) {
    Write-HookResult ''
    exit 0
}

& dotnet test '.\ConsoleApp1.Tests\ConsoleApp1.Tests.csproj' --nologo 2>&1 |
    ForEach-Object { [Console]::Error.WriteLine($_) }
if ($LASTEXITCODE -ne 0) {
    Write-HookResult 'Automatic commit skipped because the tests failed.'
    exit 0
}

& dotnet build '.\DataBanken\leerlingenConsole\ConsoleApp1.csproj' --nologo 2>&1 |
    ForEach-Object { [Console]::Error.WriteLine($_) }
if ($LASTEXITCODE -ne 0) {
    Write-HookResult 'Automatic commit skipped because the console build failed.'
    exit 0
}

& dotnet build '.\DataBanken\LeerlingenGui\LeerlingenGui.csproj' --nologo 2>&1 |
    ForEach-Object { [Console]::Error.WriteLine($_) }
if ($LASTEXITCODE -ne 0) {
    Write-HookResult 'Automatic commit skipped because the GUI build failed.'
    exit 0
}

& git add --all
if ($LASTEXITCODE -ne 0) {
    Write-HookResult 'Automatic commit skipped because Git could not stage the changes.'
    exit 0
}

& git commit -m 'Auto-commit after passing tests and builds' 2>&1 |
    ForEach-Object { [Console]::Error.WriteLine($_) }
if ($LASTEXITCODE -ne 0) {
    Write-HookResult 'Automatic commit failed. Review the Git output.'
    exit 0
}

Write-HookResult 'Changes were committed after the tests and both builds passed.'
