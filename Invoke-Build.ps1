task clean {
    Write-Host "Cleaning output directories..." -ForegroundColor Green
    Remove-Item -Recurse -Force "$PSScriptRoot\output" -ErrorAction SilentlyContinue
}

task restore {
    Write-Host "Restoring NuGet packages..." -ForegroundColor Green
    dotnet restore "$PSScriptRoot\src\WakeOnLan.sln"
}

task compile {
    Write-Host "Building the solution..." -ForegroundColor Green
    dotnet build "$PSScriptRoot\src\WakeOnLan.sln" `
        --configuration Release --output "$OutputDirectory\bin"
}

task package {
    Write-Host "Packaging the module..." -ForegroundColor Green
    Compress-Archive -Path "$OutputDirectory\bin\*" `
        -DestinationPath "$OutputDirectory\LWM.WakeOnLan.zip"
}

task pester_tests {
    Write-Host "Running Pester tests..." -ForegroundColor Green
    Invoke-Pester -Script "$PSScriptRoot\tests"
}

task publish_artifacts {
    Write-Host "Publishing artifacts..." -ForegroundColor Green
    # Logic to publish to Azure Artifacts or other locations
}
