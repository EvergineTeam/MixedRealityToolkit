# tools/sync-standards.ps1
# Synchronize static standard files from a central public repository into the current repository.
# Source can be:
#   - Public GitHub repo via raw.githubusercontent.com (default)
#   - Local folder via -SourcePath
#
# Supports schema v2 (groups with defaultGroups) and per-repo overrides:
#   - .standards.override.json with:
#       { "groups": ["group1", "group2"],
#         "remap": { "srcOrDst": "new/destination", ... },
#         "ignore": [ "glob/**", ... ] }

param(
    # Remote source (used when -SourcePath is NOT provided)
    [string]$Org = "EvergineTeam",
    [string]$Repo = "evergine-standards",
    [string]$Ref = "main",           
    
    # Local source (used when provided; Ref/Org/Repo are ignored)
    [string]$SourcePath,                           # e.g., ../evergine-standards-snapshot# Tag/branch/SHA (e.g., v1)

    # Common settings
    [string]$Manifest = "sync-manifest.json",    # Manifest filename
    [string]$Root = (Resolve-Path ".").Path,     # Target base path (usually repo root)
    [string]$OverrideFile = ".standards.override.json",
    [switch]$DryRun,                             # Only show what would be done
    [switch]$TestMode                            # Only load functions for testing, don't execute main logic
)

# ---------------------------
# Helpers: HTTP (public raw)
# ---------------------------

function Get-RawUrl([string]$path, [string]$Org, [string]$Repo, [string]$Ref) {
    return "https://raw.githubusercontent.com/$Org/$Repo/$Ref/$path"
}

function Get-SourceText([string]$path) {
    if ($SourcePath) {
        $full = Join-Path (Resolve-Path $SourcePath) $path
        if (-not (Test-Path $full)) { 
            throw "Local source file not found: $full" 
        }

        return Get-Content -Path $full -Raw
    }
    else {
        $url = Get-RawUrl $path $Org $Repo $Ref
        try { 
            return (Invoke-WebRequest -Uri $url).Content 
        }
        catch { 
            throw "Failed to download '$path' from '$url'. $($_.Exception.Message)" 
        }
    }
}

function Get-SourceBytes([string]$path) {
    if ($SourcePath) {
        $full = Join-Path (Resolve-Path $SourcePath) $path
        if (-not (Test-Path $full)) { 
            throw "Local source file not found: $full" 
        }
        return [System.IO.File]::ReadAllBytes($full)
    }
    else {
        $url = Get-RawUrl $path $Org $Repo $Ref
        $tmp = [System.IO.Path]::GetTempFileName()
        try {
            Invoke-WebRequest -Uri $url -OutFile $tmp | Out-Null
            return [System.IO.File]::ReadAllBytes($tmp)
        }
        catch {
            throw "Failed to download binary '$path' from '$url'. $($_.Exception.Message)"
        }
        finally {
            try { 
                Remove-Item -Path $tmp -Force -ErrorAction SilentlyContinue 
            }
            catch {
                # ignore
            }
        }
    }
}

# ---------------------------
# Helper functions for processing overrides and files
# ---------------------------

function Resolve-Dst([string]$src, [string]$dstDefault, [string]$overwriteDefault) {
    if ($overwrites -and $overwrites.remap) {
        $remapValue = $null
        
        # Check if there's a remap for dst or src
        if ($overwrites.remap.PSObject.Properties.Name -contains $dstDefault) { 
            $remapValue = $overwrites.remap.$dstDefault 
        }
        elseif ($overwrites.remap.PSObject.Properties.Name -contains $src) { 
            $remapValue = $overwrites.remap.$src 
        }
        
        if ($remapValue) {
            # Handle both string format ("dst": "path") and object format ("dst": {"dst": "path", "overwrite": "..."})
            if ($remapValue -is [string]) {
                # Simple string format - just the new destination path
                return @{ dst = $remapValue; overwrite = $overwriteDefault }
            }
            else {
                # Object format with dst and optionally overwrite
                $newDst = if ($remapValue.PSObject.Properties.Name -contains 'dst') { $remapValue.dst } else { $dstDefault }
                $newOverwrite = if ($remapValue.PSObject.Properties.Name -contains 'overwrite') { $remapValue.overwrite } else { $overwriteDefault }
                return @{ dst = $newDst; overwrite = $newOverwrite }
            }
        }
    }

    return @{ dst = $dstDefault; overwrite = $overwriteDefault }
}

function Is-Ignored([string]$dstFinal) {
    if ($overwrites -and $overwrites.ignore) {
        foreach ($pat in $overwrites.ignore) { 
            if ($dstFinal -like $pat) { 
                return $true 
            } 
        }
    }

    return $false
}

# ---------------------------
# Load manifest (schema v2)
# ---------------------------

# Exit early if in test mode (functions are already loaded)
if ($TestMode) {
    return
}

$sourceLabel = $(if ($SourcePath) { "local: $(Resolve-Path $SourcePath).Path" } else { "$Org/$Repo@$Ref" })
Write-Host "Loading manifest '$Manifest' from $sourceLabel ..."
$manifestText = Get-SourceText $Manifest
$manifestObj = $manifestText | ConvertFrom-Json -Depth 10

# Require schema v2
$schema = if ($manifestObj.PSObject.Properties.Name -contains 'schema') { $manifestObj.schema } else { $null }
if ($schema -ne "2") { 
    throw "Manifest must use schema v2. Found: '$schema'. Please update your manifest to use schema v2 with groups." 
}

Write-Host "Using manifest schema: v$schema"

# -------------------------------------------------------
# Load per-repo overrides (groups + remap + ignore)
# -------------------------------------------------------

$overwrites = $null
$overridePath = Join-Path $Root $OverrideFile
Write-Host "Looking for override file at: $overridePath"
if (Test-Path $overridePath) {
    try {
        $overwrites = Get-Content $overridePath -Raw | ConvertFrom-Json -Depth 5
    
        # Validate override schema matches manifest schema
        $overrideSchema = if ($overwrites.PSObject.Properties.Name -contains 'schema') { $overwrites.schema } else { "2" }
        if ($overrideSchema -ne "2") {
            throw "Override file must use schema v2. Found: '$overrideSchema'. Please update your override file to use schema v2."
        }

        Write-Host "Loaded overrides from '$OverrideFile' (schema: v2)."
        if ($overwrites.PSObject.Properties.Name -contains 'groups') {
            Write-Host "Selected groups: $($overwrites.groups -join ', ')"
        }
        if ($overwrites.PSObject.Properties.Name -contains 'remap') {
            Write-Host "Remap rules: $($overwrites.remap | ConvertTo-Json -Compress)"
        }
        if ($overwrites.PSObject.Properties.Name -contains 'ignore') {
            Write-Host "Ignore files: $($overwrites.ignore -join ', ')"
        }
    }
    catch {
        throw "Failed to parse overrides file '$OverrideFile'. $($_.Exception.Message)"
    }
}
else {
    Write-Host "No override file found at: $overridePath"
}

# -------------------------------------------------------
# Process manifest entries (schema v2 only)
# -------------------------------------------------------

$entries = @()

# Determine which groups to use
$selectedGroups = @()
if ($overwrites -and $overwrites.PSObject.Properties.Name -contains 'groups') {
    # Use groups specified in override file
    $selectedGroups = $overwrites.groups
    Write-Host "Using groups from override file: $($selectedGroups -join ', ')"
}
elseif ($manifestObj.PSObject.Properties.Name -contains 'defaultGroups') {
    # Use defaultGroups from manifest
    $selectedGroups = $manifestObj.defaultGroups
    Write-Host "Using defaultGroups from manifest: $($selectedGroups -join ', ')"
}
else {
    throw "Schema v2 manifest must have 'defaultGroups' defined, or override file must specify 'groups'."
}

# Process selected groups
foreach ($groupName in $selectedGroups) {
    if ($manifestObj.groups.PSObject.Properties.Name -contains $groupName) {
        $groupFiles = $manifestObj.groups.$groupName
        Write-Host "Processing group '$groupName' with $($groupFiles.Count) files..."
        
        foreach ($file in $groupFiles) {
            $overwrite = if ($file.PSObject.Properties.Name -contains 'overwrite') { $file.overwrite } else { "always" }
            $entries += [pscustomobject]@{ src = $file.src; dst = $file.dst; overwrite = $overwrite }
        }
    }
    else {
        Write-Warning "Group '$groupName' not found in manifest. Available groups: $($manifestObj.groups.PSObject.Properties.Name -join ', ')"
    }
}

Write-Host "Loaded $($entries.Count) entries from manifest."

# -------------------------------------------------------
# Sync files (always overwrite unless -DryRun)
# -------------------------------------------------------

$updated = 0
$ignored = 0
$skipped = 0
$total = $entries.Count

foreach ($e in $entries) {
    $src = $e.src
    $resolved = Resolve-Dst $src $e.dst $e.overwrite
    $dst = $resolved.dst
    $overwrite = $resolved.overwrite

    if (Is-Ignored $dst) {
        Write-Host "Ignored by override: $dst"
        $ignored++
        continue
    }

    $dstFull = Join-Path $Root $dst

    # Check overwrite policy
    if ($overwrite -eq "ifMissing" -and (Test-Path $dstFull)) {
        Write-Host "Skipped (exists, overwrite=ifMissing): $dst"
        $skipped++
        continue
    }

    $dir = Split-Path $dstFull -Parent
    if ($dir -and -not (Test-Path $dir)) {
        if (-not $DryRun) { 
            New-Item -ItemType Directory -Force -Path $dir | Out-Null 
        }
    }

    if ($DryRun) {
        Write-Host "(dry-run) $src  →  $dst (overwrite: $overwrite)"
        continue
    }

    try {
        $bytes = Get-SourceBytes $src
        if ($null -eq $bytes -or $bytes.Length -eq 0) {
            Write-Warning "Source file '$src' is empty or could not be read. Skipping: $dst"
            $skipped++
            continue
        }
        [System.IO.File]::WriteAllBytes($dstFull, $bytes)
        Write-Host "Updated: $dst (overwrite: $overwrite)"
        $updated++
    }
    catch {
        Write-Warning "Failed to process file '$src' -> '$dst': $($_.Exception.Message)"
        $skipped++
        continue
    }
}Write-Host ""
Write-Host "Summary:"
Write-Host "   Updated: $updated"
Write-Host "   Ignored: $ignored"
Write-Host "   Skipped: $skipped"
Write-Host "   Total in manifest: $total"
Write-Host "Done."