$ErrorActionPreference = 'Stop'

function Invoke-Native([scriptblock]$Command) {
    & $Command
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

Push-Location (Join-Path $PSScriptRoot '..\web')
try {
    Invoke-Native { npm ci }
    Invoke-Native { npx ng build --configuration production }
}
finally {
    Pop-Location
}

Push-Location $PSScriptRoot
try {
    Invoke-Native { npm ci }
    Invoke-Native { npx cdk deploy --profile bookstore }
}
finally {
    Pop-Location
}
