<#
.SYNOPSIS
	Evergine MRTK NuGet Packages helper script, (c) 2022 Evergine
.DESCRIPTION
	This script contains some helper functions needed for the package generation process.
.EXAMPLE
	<script> -version 3.4.22.288-local
.LINK
	https://evergine.com/
#>

# Utility functions
function LogDebug($line)
{
	Write-Host "##[debug] $line" -Foreground Blue -Background Black
}

function ShowVariables($version, $buildConfiguration, $buildVerbosity, $outputFolderBase)
{
	LogDebug "############## VARIABLES ##############"
	LogDebug "Version.............: $version"
	LogDebug "Build configuration.: $buildConfiguration"
	LogDebug "Build verbosity.....: $buildVerbosity"
	LogDebug "Output folder.......: $outputFolderBase"
	LogDebug "#######################################"
}

function CreateOutputFolder($outputFolderBase)
{
	$_ = New-Item -ItemType Directory -Force -Path $outputFolderBase
	Resolve-Path $outputFolderBase
}

function PrepareEnvironment
{
	# Create temp folder
	$tempFolder = "temp"
	New-Item -ItemType Directory -Force -Path $tempFolder
	
	# Add to path
	$toolsPath = Resolve-Path $tempFolder
	$env:Path = "$toolsPath;" + $env:Path

	# Download vswhere
	$vsWherePath = Join-Path -Path $tempFolder -ChildPath "vswhere.exe"
	Invoke-WebRequest "https://github.com/microsoft/vswhere/releases/download/3.1.1/vswhere.exe" -OutFile $vsWherePath

	# Invoke vswhere
	$VSPath = vswhere -prerelease -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
	if (-Not $?) { exit $lastexitcode }
	
	# Enter dev shell
	Import-Module "$VSPath\Common7\Tools\Microsoft.VisualStudio.DevShell.dll"
	Enter-VsDevShell -VsInstallPath "$VSPath" -SkipAutomaticLocation

	# Clean up
	Remove-Item $toolsPath -Recurse
}