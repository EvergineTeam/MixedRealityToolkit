<#
.SYNOPSIS
	Evergine MRTK Assets Packages generator script, (c) 2022 Evergine
.DESCRIPTION
	This script generates Assets packages for the Mixed Reality Toolkit for Evergine
	It's meant to have the same behavior when executed locally as when it's executed in a CI pipeline.
.EXAMPLE
	<script> -version 2022.2.11.1-local
.LINK
	https://evergine.com/
#>

param (
	[Parameter(mandatory=$true)][string]$version,
	[string]$outputFolderBase = "wepkgs",
	[string]$buildVerbosity = "normal",
	[string]$buildConfiguration = "Release",
	[string]$assetsCsprojPath = "Source\Evergine.MRTK.Assets\Evergine.MRTK.Assets.csproj",
	[string]$wespecPath = "Source\Evergine.MRTK.Assets\Evergine.MRTK.wespec"
)

# Source helper functions
. ./Helpers.ps1

# Show variables
ShowVariables $version $buildConfiguration $buildVerbosity $outputFolderBase

# Install YAML cmdlets
if (Get-Module -ListAvailable -Name powershell-yaml)
{
	Write-Host "Skip: powershell-yaml already installed"
}
else
{
	Write-Host "Installing powershell-yaml"
	Install-Module -Name powershell-yaml -Force -Repository PSGallery -Scope CurrentUser
	if (-Not $?)
	{ 
		throw "Could not install YAML module. Code: {$lastexitcode}"
	}
}

# Update wespec file
Write-Host "Updating version dependency for $wespecPath"
$wespecContents = Get-Content -Raw -Path $wespecPath | ConvertFrom-Yaml -Ordered
$wespecContents.Nugets = $wespecContents.Nugets -replace "2023.0.0.0-preview$", $version
ConvertTo-Yaml -Data $wespecContents | Out-File -Encoding ascii -FilePath $wespecPath

# Create output folder
$absoluteOutputFolder = (CreateOutputFolder $outputFolderBase)

# Locate build tools and enter build environment
PrepareEnvironment

# Generate packages
LogDebug "START assets packaging process"
& msbuild -t:restore "$assetsCsprojPath" -p:RestorePackagesConfig=true
& msbuild -t:build "$assetsCsprojPath" -v:$buildVerbosity -p:Configuration=$buildConfiguration -t:CreateEvergineAddOn -p:Version=$version -p:OutputPath="$absoluteOutputFolder"

LogDebug "END assets packaging process"
