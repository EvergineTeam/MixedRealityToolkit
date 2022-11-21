<#
.SYNOPSIS
	Evergine MRTK NuGet Packages generator script, (c) 2022 Evergine
.DESCRIPTION
	This script generates NuGet packages for the Mixed Reality Toolkit for Evergine
	It's meant to have the same behavior when executed locally as when it's executed in a CI pipeline.
.EXAMPLE
	<script> -version 3.4.22.288-local
.LINK
	https://evergine.com/
#>

param (
    [Parameter(mandatory=$true)][string]$version,
	[string]$outputFolderBase = "nupkgs",
	[string]$buildVerbosity = "normal",
	[string]$buildConfiguration = "Release",
	[string]$bindingsCsprojPath = "Source\Evergine.MRTK\Evergine.MRTK.csproj",
	[string]$editorCsprojPath = "Source\Evergine.MRTK.Editor\Evergine.MRTK.Editor.csproj"
)

# Source helper functions
. ./Helpers.ps1

# Show variables
ShowVariables $version $buildConfiguration $buildVerbosity $outputFolderBase

# Create output folder
$absoluteOutputFolder = (CreateOutputFolder $outputFolderBase)

# Locate build tools and enter build environment
PrepareEnvironment

# Generate packages
LogDebug "START packaging process"
& msbuild -t:restore "$editorCsprojPath" -p:RestorePackagesConfig=true
& msbuild -t:build "$editorCsprojPath" -v:$buildVerbosity -p:Configuration=$buildConfiguration

& msbuild -t:restore "$bindingsCsprojPath" -p:RestorePackagesConfig=true
& msbuild -t:pack "$bindingsCsprojPath" -v:$buildVerbosity -p:Configuration=$buildConfiguration -p:PackageOutputPath="$absoluteOutputFolder" -p:IncludeSymbols=true -p:Version=$version

LogDebug "END packaging process"