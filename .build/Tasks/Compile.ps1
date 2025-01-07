param
(
    [Parameter()]
    [System.String]
    $ProjectName = (property ProjectName ''),

    [Parameter()]
    [System.String]
    $SourcePath = (property SourcePath ''),

    [Parameter()]
    [System.String]
    $OutputDirectory = (property OutputDirectory (Join-Path $BuildRoot 'output')),

    [Parameter()]
    [System.String]
    $BuiltModuleSubdirectory = (property BuiltModuleSubdirectory ''),

    [Parameter()]
    [System.Management.Automation.SwitchParameter]
    $VersionedOutputDirectory = (property VersionedOutputDirectory $true),

    [Parameter()]
    [System.String]
    $BuildModuleOutput = (property BuildModuleOutput (Join-Path $OutputDirectory $BuiltModuleSubdirectory)),

    [Parameter()]
    [System.String]
    $ReleaseNotesPath = (property ReleaseNotesPath (Join-Path $OutputDirectory 'ReleaseNotes.md')),

    [Parameter()]
    [System.String]
    $ModuleVersion = (property ModuleVersion ''),

    [Parameter()]
    [System.Collections.Hashtable]
    $BuildInfo = (property BuildInfo @{ })
)

task restore {

    $path = Get-SamplerAbsolutePath
    ##. Set-SamplerTaskVariable -AsNewBuild
    Write-Host "Cleaning solution..." -ForegroundColor Green
    dotnet clean "$path\WakeOnLan.sln"

    Write-Host "Restoring NuGet packages..." -ForegroundColor Green
    dotnet restore "$path\WakeOnLan.sln"
}

task compile {

    . Set-SamplerTaskVariable

    $path = "$OutputDirectory\$BuiltModuleSubdirectory\LWM.WakeOnLan"

    #Write-Host $path -ForegroundColor green

    #Write-Host ($BuildInfo |out-string) -ForegroundColor Cyan

    Write-Host "Building the solution..." -ForegroundColor Green

    $null = dotnet-gitversion /updateassemblyinfo

    dotnet build "$ProjectPath\WakeOnLan.sln" `
        --configuration Release --output "$BuiltModuleBase"
}


Task Bloop {
    . Set-SamplerTaskVariable
    $out = $BuiltModuleManifest |out-string

    Write-Host "testing:" $out -ForegroundColor Green
}

task pester_tests {
    Write-Host "Running Pester tests..." -ForegroundColor Green
    Invoke-Pester -Script "$PSScriptRoot\tests"
}

task publish_artifacts {
    Write-Host "Publishing artifacts..." -ForegroundColor Green
    # Logic to publish to Azure Artifacts or other locations
}

task install_xmldoc2cmdletdoc {
    Write-Host "Ensuring XmlDoc2CmdletDoc is installed..." -ForegroundColor Green
    dotnet tool install --global XmlDoc2CmdletDoc
}
