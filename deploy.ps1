$scriptPath = Split-Path -parent $MyInvocation.MyCommand.Definition
# Change into the deployment root directory.
Set-Location $scriptPath

function Set-WebAppOffline
{
	param($targetFolder)
	Write-Host Taking website offline
    Copy-Item -Path "app_offline.htm" -Destination $targetFolder -Force
}
 
function Set-WebAppOnline
{
	param($targetFolder)
	Write-Host Back online
	$target = Join-Path $targetFolder "app_offline.htm"
    Remove-Item -Path $target -Force
}

function Clean-TemporaryFolder
{
	param($rootFolder)
	Write-Host Delete temp extract folder
	$Source = Join-Path $rootFolder 'temp'
	if (Test-Path $Source) 
	{
		Remove-Item -force -Recurse $Source
	}
	New-Item -ItemType Directory -Force -Path $Source  | Out-Null
	return $Source
}

function Clean-WebsiteFolder
{
	param($targetWebsiteFolder)
	Write-Host Cleaning website folder for deployment (keeping config)
	Get-ChildItem -Path $targetWebsiteFolder | 
		Where {$_.Name -notlike 'app_offline.htm'} | 
		Where {$_.Name -notlike 'LocalAppSettings.config'} | 
		Where {$_.Name -notlike 'LocalConnectionStrings.config'} | 
		Select -ExpandProperty FullName |
		sort length -Descending |
		Remove-Item -force 
}

function Install-Package
{
	param($Name,$Source,$Target) 
	.\nuget.exe install $Name  -Source $Source -ExcludeVersion -OutputDirectory $Target 
}

if ($args.count -lt 2)
{
    Write-Host "You need to enter website/folder to deploy to and at least one package."
	exit 34
} 
if([string]::IsNullOrWhiteSpace($args[0])) 
{
    Write-Host "You need to enter website/folder to deploy to."
	exit 35
} 
if([string]::IsNullOrWhiteSpace($args[1])) 
{
    Write-Host "You need to enter at least one package."
	exit 36
} 

Write-Host Deploying to:
Write-Host $args[0]
$targetWebsite = $args[0]
Write-Host Deploying packages:
for ( $i = 1; $i -lt $args.count; $i++ ) {
	Write-Host $args[$i]
}

try
{
	$invocation = (Get-Variable MyInvocation).Value
	$currenddirectorypath = Split-Path $invocation.MyCommand.Path
	$targetWebsiteFolder = Join-Path $currenddirectorypath $targetWebsite

	$Source = Clean-TemporaryFolder($currenddirectorypath)

	for ( $i = 1; $i -lt $args.count; $i++ ) {
		Install-Package -Name $args[$i] -Source $currenddirectorypath -Target $Source
	}
	New-Item -ItemType Directory -Force -Path $targetWebsiteFolder | Out-Null
}
catch
{
	exit 37
}



try
{
	Set-WebAppOffline($targetWebsiteFolder)
	for ( $i = 1; $i -lt $args.count; $i++ ) {
		$packagePath = Join-Path $Source $args[$i]
		$s = "$($packagePath)\*" 
		Write-Host $s
		Copy-Item -Path $s -Destination $targetWebsiteFolder -Force -Recurse -Exclude @('*.nupkg')
	}
}
catch
{
	Write-Host error while copying from temp to deployment folder
	exit 38
}
finally
{
	Set-WebAppOnline($targetWebsiteFolder)
}




#Get-ChildItem -Path C:\Temp\dep\extract\One.Net -Recurse | Select -ExpandProperty FullName | Write-Host
#Get-ChildItem -Path C:\Temp\dep\extract\One.Net  -Recurse | %{ Copy-Item -Path $_.FullName -Destination $targetWebsiteFolder }
#Get-ChildItem -Path $Source  -Recurse | % { Copy-Item -Path $_.FullName -Destination $targetWebsiteFolder -Force -Recurse -Container }





