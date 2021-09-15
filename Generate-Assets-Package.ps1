<#
.SYNOPSIS
	Wave Engine MRTK Assets Packages generator script, (c) 2021 Wave Engine
.DESCRIPTION
	This script generates Assets packages for the Mixed Reality Toolkit for Wave Engine
	It's meant to have the same behavior when executed locally as when it's executed in a CI pipeline.
.EXAMPLE
	<script> -version 3.4.22.288-local
.LINK
	https://waveengine.net
#>

param (
    [Parameter(mandatory=$true)][string]$version,
	[string]$outputFolderBase = "wepkgs",
	[string]$buildVerbosity = "normal",
	[string]$buildConfiguration = "Release",
	[string]$assetsCsprojPath = "Source\WaveEngine.MRTK.Assets\WaveEngine.MRTK.Assets.csproj"
)

# Utility functions
function LogDebug($line) { Write-Host "##[debug] $line" -Foreground Blue -Background Black }

# Show variables
LogDebug "############## VARIABLES ##############"
LogDebug "Version.............: $version"
LogDebug "Build configuration.: $buildConfiguration"
LogDebug "Build verbosity.....: $buildVerbosity"
LogDebug "Output folder.......: $outputFolderBase"
LogDebug "#######################################"

# Create output folder
$outputFolder = Join-Path $outputFolderBase $versionWithSuffix
New-Item -ItemType Directory -Force -Path $outputFolder
$absoluteOutputFolder = Resolve-Path $outputFolder

# Generate packages
LogDebug "START assets packaging process"
& dotnet build "$assetsCsprojPath" -v:$buildVerbosity -p:Configuration=$buildConfiguration -p:OutputPath="$absoluteOutputFolder" -p:Version=$version

Get-ChildItem "$absoluteOutputFolder" -Exclude "*.wepkg" | Remove-Item -Recurse

LogDebug "END assets packaging process"
