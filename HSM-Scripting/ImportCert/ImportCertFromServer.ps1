# Not designed for Powershell ISE
# Double-check you are allowed to run custom scripts.

$VariablesSource = "Development"

if("#{Deploy.HSMScripting.OpenSslLoc}#" -like "*Deploy.HSMScripting.OpenSslLoc*")
{
	#dev
    $OpenSslLoc = "`"C:\Program Files\OpenSSL-Win64\bin\openssl.exe`""
    $HSMAdminToolsDir = "C:\Program Files\Utimaco\CryptoServer\Administration"
}
else
{
	#test, accp and prod
	$VariablesSource = "Deploy"
    $OpenSslLoc = "`"#{Deploy.HSMScripting.OpenSslLoc}#`""
    $HSMAdminToolsDir = "#{Deploy.HSMScripting.HSMAdminToolsDir}#"
}

function SetErrorToStop
{
	$ErrorAtStart = $ErrorActionPreference
	$ErrorActionPreference = "Stop"
	write-host "Error-behaviour is set from $ErrorAtStart to $ErrorActionPreference."
}

function CheckNotIse
{
	if($host.name -match "ISE")
	{
		write-host "`nYou are running this script in Powershell ISE. Please switch to the regular Powershell."
		Pause
		
		exit
	}
}

function Pause ($Message = "Press any key to continue...`n")
{
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

function RunWithErrorCheck ([string]$command) 
{
	iex "& $command"

    if($lastexitcode -ne 0)
    {
        write-Warning "Script terminated due to an error. :("
		Read-Host 'Press Enter to continue.'
        exit
    }
}


#
# Start
#


write-host "Importer for certificates from other server"
CheckNotIse

write-warning "`nPlease check the following:`
- Using variables from $VariablesSource. Correct?`
- Are the required root certificates installed?
- Is the certificate placed in this folder?
- Is the certificate NOT a root certificate?
- Is the certificate in Base-64 (.pem) format?
If not: abort this script with Ctrl+C."
Pause

write-host "`nCheck if HSM is accessible"
Pause

RunWithErrorCheck "`"$HSMAdminToolsDir\cngtool`" providerinfo" 

write-host "`nImporting certificate"
Pause

$CertName = read-host "`nPlease enter the filename of the certificate with extension"
RunWithErrorCheck "certutil -addstore -f `"my`" $CertName"
Pause

$CertSerial = RunWithErrorCheck "$OpenSslLoc x509 -serial -in $CertName -noout"
$CertSerial = $CertSerial.Replace("serial=","")
write-host "Serial: $CertSerial"

write-host "`nConnecting installed cert to private key in HSM"
Pause

RunWithErrorCheck "certutil -f -csp `"Utimaco CryptoServer Key Storage Provider`" -repairstore `"my`" `"$CertSerial`""

write-host "`nDone! The installed certificate should show a key on its icon in certlm."
Pause