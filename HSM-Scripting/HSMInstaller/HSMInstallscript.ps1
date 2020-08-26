# NOT designed for Powershell ISE
# Double-check you are allowed to run custom scripts.
# see: https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.security/set-executionpolicy?view=powershell-7
#win10 has Powershell 5.1, which includes an unzip function.

$currentDir
$HSMsimulatorDir = "C:\Program Files\Utimaco\CryptoServer\Simulator\sim5_windows\bin"
$HSMAdminToolsDir = "C:\Program Files\Utimaco\CryptoServer\Administration"
$cngtoolconfigloc = "C:\ProgramData\Utimaco\CNG"

function SetErrorToStop
{
	$ErrorAtStart = $ErrorActionPreference
	$ErrorActionPreference = "Stop"
	write-host "Error-behaviour is set from $ErrorAtStart to $ErrorActionPreference."
}

function RunWithErrorCheck ([string]$command) 
{
	write-host $command
	iex "& $command" #I'm not frustrated at all
	    
	if($lastexitcode -ne 0)
    {
        write-Warning "Script terminated due to an error. :("
		Read-Host 'Press Enter to continue.'
        exit
    }
}

function Pause ($Message = "Press any key to continue...`n") {
    If ($psISE) {
        # The "ReadKey" functionality is not supported in Windows PowerShell ISE.
 
        $Shell = New-Object -ComObject "WScript.Shell"
        $Button = $Shell.Popup("Click OK to continue.", 0, "Script Paused", 0)
 
        Return
    }
 
    Write-Host -NoNewline $Message
 
    $Ignore =
        16,  # Shift (left or right)
        17,  # Ctrl (left or right)
        18,  # Alt (left or right)
        20,  # Caps lock
        91,  # Windows key (left)
        92,  # Windows key (right)
        93,  # Menu key
        144, # Num lock
        145, # Scroll lock
        166, # Back
        167, # Forward
        168, # Refresh
        169, # Stop
        170, # Search
        171, # Favorites
        172, # Start/Home
        173, # Mute
        174, # Volume Down
        175, # Volume Up
        176, # Next Track
        177, # Previous Track
        178, # Stop Media
        179, # Play
        180, # Mail
        181, # Select Media
        182, # Application 1
        183  # Application 2
 
    While ($KeyInfo.VirtualKeyCode -Eq $Null -Or $Ignore -Contains $KeyInfo.VirtualKeyCode) {
        $KeyInfo = $Host.UI.RawUI.ReadKey("NoEcho, IncludeKeyDown")
    }
 
    Write-Host
}
# Got this from https://adamstech.wordpress.com/2011/05/12/how-to-properly-pause-a-powershell-script/
# A soviet-style pause function...

function createCngConfig
{
	$fileContent = `
"Logpath = $cngtoolconfigloc`
Logging = 3`
Logsize = 8mb`
KeysExternal = false`
KeyStore = $cngtoolconfigloc\keys`
ExportPolicy = 0`
KeepAlive = true`
ConnectionTimeout = 3000`
CommandTimeout = 60000`
Group = CNG`
Login=Henk,1234`
Device = 3001@127.0.0.1"

	New-Item -force -name "keys" -path $cngtoolconfigloc -ItemType Directory -ErrorAction Stop
	New-Item -force -name "cs_cng.cfg" -path $cngtoolconfigloc -ItemType File -Value $filecontent -ErrorAction Stop
}


#
# Start
#

write-host "Installscript for the Utimaco HSM simulator. The following will be installed:`
- OpenSSL`
- Utimaco HSM simulator`
- Utimaco HSM management software`
`
It assumes JRE 8 is installed."
Pause

SetErrorToStop

if($host.name -match "ISE")
{
	write-host "You are running this script in Powershell ISE. Please switch to the regular Powershell."
	
	Pause
	exit
}

$currentDir = Get-Location

write-host "`nInstalling OpenSSL`
This installer is grabbed from https://slproweb.com/products/Win32OpenSSL.html`
As listed under https://wiki.openssl.org/index.php/Binaries"
Pause

RunWithErrorCheck "msiexec /i `"$currentDir\Win64OpenSSL_Light-1_1_1g.msi`" /quiet /L* `"$currentDir\OpenSSlInstall.log`" | Out-Null"

write-host "`nInstalling Utimaco software"
Pause

Expand-Archive -Force -LiteralPath "$currentDir\SecurityServerEvaluation-V4.31.1.0.zip" -DestinationPath "$currentDir\HSMInstall\"

#installparams: https://jrsoftware.org/ishelp/index.php?topic=setupcmdline
RunWithErrorCheck "`"$currentdir\HSMInstall\CryptoServerSimSetup-4.31.1.0.exe`" /VERYSILENT /LOG=`"CryptoServerInstall.log`" /NORESTART /LOADINF=`"$currentdir\HSMInstallconfig.inf`" /SP- /SUPPRESSMSGBOXES | Out-Null"

write-host "`nCreating config file for CngTool"
Pause

createCngConfig

write-host "`nStarting the Simulator (background job); this script will wait 10 secs"
Pause

#This doesn't work on powershell-azure. Get Powershell 7 and use Start-ThreadJob for that
$simulator = start-job -filepath .\RunSimulator.ps1
Start-Sleep -Seconds 10

write-host "`nCreating user account in HSM"
Pause

RunWithErrorCheck "`"$HSMAdminToolsDir\csadm`" dev=3001@127.0.0.1 logonsign=ADMIN,`"$HSMAdminToolsDir\ADMIN.key`" adduser=Henk,00000002`"{CXI_GROUP=CNG}`",hmacpwd,1234"

write-host "`nCheck if the simulator is running and access is allowed"
Pause

RunWithErrorCheck -command "`"$HSMAdminToolsDir\cngtool`" providerinfo"

Pause