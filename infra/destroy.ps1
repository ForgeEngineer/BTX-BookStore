$ErrorActionPreference = 'Stop'

Push-Location $PSScriptRoot
try {
    npm ci
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    npx cdk destroy --profile bookstore --force
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}
finally {
    Pop-Location
}
