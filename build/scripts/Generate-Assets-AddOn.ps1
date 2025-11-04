<#
.SYNOPSIS
	Evergine add-on assets generator script, (c) 2025 Evergine Team
.DESCRIPTION
	This script generates .wepkg asset packages for Evergine add-ons using dotnet
	It handles .wespec file updates, version management, and package generation using the CreateEvergineAddOn target.
	It's meant to have the same behavior when executed locally as when it's executed in a CI pipeline.
.PARAMETER Version
	Version string for the add-on packages (e.g., "2025.1.0.123-preview"). Mutually exclusive with Revision.
.PARAMETER Revision
	Revision number for date-based versioning (e.g., 123). Mutually exclusive with Version.
.PARAMETER VersionSuffix
	Optional suffix to append to the final version (e.g., "nightly" for "2025.11.3.123-nightly")
.PARAMETER AssetsCsprojPath
	Path to the assets .csproj file that contains the CreateEvergineAddOn target
.PARAMETER WespecPath
	Path to the .wespec file. If not provided, will auto-detect in the same directory as the .csproj
.PARAMETER OutputFolderBase
	Base folder name for .wepkg package output (default: "wepkgs")
.PARAMETER BuildVerbosity
	Build verbosity level for dotnet commands (default: "normal")
.PARAMETER BuildConfiguration
	Build configuration (Release/Debug, default: "Release")
.PARAMETER HelpersPath
	Path to Helpers.ps1 file (default: "$PSScriptRoot\Helpers.ps1")
.PARAMETER VersionToken
	Exact token to search and replace in .wespec Nugets section (default: "2025.0.0.0-preview")
.EXAMPLE
	.\Generate-Assets-AddOn.ps1 -Version "2025.1.0.123-preview" -AssetsCsprojPath "Source\MyAddon.Assets\MyAddon.Assets.csproj"
.EXAMPLE
	.\Generate-Assets-AddOn.ps1 -Revision 123 -AssetsCsprojPath "Assets\Assets.csproj" -WespecPath "Assets\MyAddon.wespec"
.EXAMPLE
	.\Generate-Assets-AddOn.ps1 -Version "2025.1.0.123-preview" -AssetsCsprojPath "Assets\Assets.csproj" -VersionToken "2025.0.0.0-preview"
.EXAMPLE
	# Using version suffix for nightly builds
	.\Generate-Assets-AddOn.ps1 -Revision 123 -AssetsCsprojPath "Source\MyAddon.Assets\MyAddon.Assets.csproj" -VersionSuffix "nightly"
.LINK
	https://evergine.com/
#>

param (
    [string]$Version = "",                      # Version string for the add-on packages
    [string]$Revision = "",                     # Revision number for date-based versioning
    [string]$VersionSuffix = "",                # Optional suffix to append to final version
    [string]$AssetsCsprojPath = "",            # Path to the assets .csproj file
    [string]$WespecPath = "",                  # Path to the .wespec file (auto-detect if empty)
    [string]$OutputFolderBase = "wepkgs",     # Base folder name for .wepkg package output
    [string]$BuildVerbosity = "normal",       # Build verbosity level for dotnet commands
    [string]$BuildConfiguration = "Release",  # Build configuration (Release/Debug)
    [string]$HelpersPath = "$PSScriptRoot\..\common\Helpers.ps1",  # Path to Helpers.ps1 file
    [string]$VersionToken = "2025.0.0.0-preview",  # Exact token to search and replace in .wespec Nugets section
    [switch]$TestMode                         # Load only functions for testing, do not execute main logic
)

# Exported utility functions for unit testing
function Test-AssetParameters {
    param(
        [Parameter(Mandatory)] [hashtable]$params
    )
    if ([string]::IsNullOrWhiteSpace($params.Version) -and [string]::IsNullOrWhiteSpace($params.Revision)) { return $false }
    if (-not [string]::IsNullOrWhiteSpace($params.Version) -and -not [string]::IsNullOrWhiteSpace($params.Revision)) { return $false }
    if ([string]::IsNullOrWhiteSpace($params.AssetsCsprojPath)) { return $false }
    return $true
}

function Find-WespecFile {
    param([string]$ProjectDirectory)
    
    $wespecFiles = Get-ChildItem -Path $ProjectDirectory -Filter "*.wespec" -File
    if ($wespecFiles.Count -eq 0) {
        return $null
    }
    elseif ($wespecFiles.Count -gt 1) {
        throw "Multiple .wespec files found in project directory: $ProjectDirectory. Please specify -WespecPath parameter."
    }
    else {
        return $wespecFiles[0].FullName
    }
}

function Update-WespecVersion {
    param(
        [string]$WespecPath,
        [string]$Version,
        [string]$VersionToken
    )
    
    LogDebug "Updating .wespec file: $WespecPath"
    
    # Read and parse .wespec file
    $wespecContents = Get-Content -Raw -Path $WespecPath | ConvertFrom-Yaml -Ordered
    
    # Update version in Nugets section using exact token replacement
    if ($wespecContents.Nugets) {
        $originalNugets = $wespecContents.Nugets
        $wespecContents.Nugets = $wespecContents.Nugets -replace $VersionToken, $Version
        
        if ($originalNugets -ne $wespecContents.Nugets) {
            LogDebug "Updated Nugets in .wespec: $originalNugets -> $($wespecContents.Nugets)"
        }
        else {
            LogDebug "No version token '$VersionToken' found in Nugets, .wespec file unchanged"
        }
    }
    
    # Write updated .wespec file
    ConvertTo-Yaml -Data $wespecContents | Out-File -Encoding ascii -FilePath $WespecPath
    LogDebug "Updated .wespec file saved"
}

# If in test mode, only load functions and exit
if ($TestMode) {
    return
}

# Validate required parameters
if (-not (Test-AssetParameters @{ Version = $Version; Revision = $Revision; AssetsCsprojPath = $AssetsCsprojPath })) {
    Write-Host "ERROR: Either Version or Revision must be provided (but not both), and AssetsCsprojPath is required" -ForegroundColor Red
    exit 1
}

# Load helpers
. $HelpersPath

# Resolve version from parameters (including suffix)
try {
    $Version = Resolve-Version -version $Version -revision $Revision -versionSuffix $VersionSuffix
}
catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Prepare environment
LogDebug "=== Generate-Assets-AddOn.ps1 Started ==="

# Validate inputs
if (-not (Test-Path $AssetsCsprojPath)) {
    LogDebug "ERROR: Assets project file not found: $AssetsCsprojPath"
    exit 1
}

$AssetsCsprojPath = Resolve-Path $AssetsCsprojPath
$projectDir = Split-Path -Parent $AssetsCsprojPath

# Auto-detect .wespec file if not provided
if ([string]::IsNullOrEmpty($WespecPath)) {
    $WespecPath = Find-WespecFile $projectDir
    if ([string]::IsNullOrEmpty($WespecPath)) {
        LogDebug "ERROR: No .wespec file found in project directory: $projectDir"
        exit 1
    }
    LogDebug "Auto-detected .wespec file: $WespecPath"
}
else {
    if (-not (Test-Path $WespecPath)) {
        LogDebug "ERROR: Wespec file not found: $WespecPath"
        exit 1
    }
    $WespecPath = Resolve-Path $WespecPath
}

# Show variables
ShowVariables @{
    "Version"             = $Version
    "Revision"            = $Revision
    "Assets project"      = $AssetsCsprojPath
    "Wespec file"         = $WespecPath
    "Output folder"       = $OutputFolderBase
    "Build configuration" = $BuildConfiguration
    "Build verbosity"     = $BuildVerbosity
    "Helpers path"        = $HelpersPath
    "Version token"       = $VersionToken
}

# Ensure powershell-yaml module is available
LogDebug "Checking powershell-yaml module..."
if (-not (Get-Module -ListAvailable -Name powershell-yaml)) {
    LogDebug "Installing powershell-yaml module..."
    Install-Module -Name powershell-yaml -Force -Scope CurrentUser
}

Import-Module powershell-yaml

# Create output directory
$outputDir = CreateOutputFolder $OutputFolderBase

try {
    # Update .wespec file with version
    Update-WespecVersion -WespecPath $WespecPath -Version $Version -VersionToken $VersionToken

    # Generate .wepkg using dotnet msbuild with CreateEvergineAddOn target
    LogDebug "START add-on assets generation process"
    
    # First, build the project
    LogDebug "Building assets project..."
    & dotnet build "$AssetsCsprojPath" `
        --configuration $BuildConfiguration `
        --verbosity $BuildVerbosity `
        -p:Version=$Version
        
    if ($LASTEXITCODE -ne 0) {
        LogDebug "ERROR: Assets project build failed"
        exit 1
    }
    
    # Then execute the CreateEvergineAddOn target
    LogDebug "Executing CreateEvergineAddOn target..."
    & dotnet msbuild "$AssetsCsprojPath" `
        /t:CreateEvergineAddOn `
        /p:Configuration=$BuildConfiguration `
        /p:Version=$Version `
        /p:OutputPath="$outputDir" `
        /v:$BuildVerbosity
        
    if ($LASTEXITCODE -eq 0) {
        LogDebug "END add-on assets generation process"
    }
    else {
        LogDebug "ERROR: Add-on assets generation failed"
        exit 1
    }
    
    # Verify output
    $wepkgFiles = Get-ChildItem -Path $outputDir -Filter "*.wepkg" -File
    if ($wepkgFiles.Count -eq 0) {
        LogDebug "ERROR: No .wepkg files generated in output directory: $outputDir"
        exit 1
    }
    
    LogDebug "Successfully generated .wepkg packages:"
    foreach ($file in $wepkgFiles) {
        LogDebug "  - $($file.Name) ($([math]::Round($file.Length / 1KB, 2)) KB)"
    }
    
    LogDebug "=== Generate-Assets-AddOn.ps1 Completed Successfully ==="
    
}
catch {
    LogDebug "=== Generate-Assets-AddOn.ps1 Failed ==="
    LogDebug "ERROR: $($_.Exception.Message)"
    exit 1
}