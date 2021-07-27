# Not designed for Powershell ISE
# Double-check you are allowed to run custom scripts.

$VariablesSource = "Development"

if("#{Deploy.HSMScripting.OpenSslLoc}#" -like "*Deploy.HSMScripting.OpenSslLoc*")
{
	#dev
    $HSMAdminToolsDir = "C:\Program Files\Utimaco\CryptoServer\Administration"
    $openSslLoc = "`"C:\Program Files\OpenSSL-Win64\bin\openssl.exe`""

    $IsOnDevEnvironment = $True #When set to $False: skips signing and accepting of the RSA request
    $CnValue = "ontw.coronamelder-api.nl" #should be [test.signing|acceptatie.signing|signing].coronamelder-api.nl
	$CertLifeTimeDays = 3650
}
else
{
	#test, accp and prod
	$VariablesSource = "Deploy"
    $HSMAdminToolsDir = "#{Deploy.HSMScripting.HSMAdminToolsDir}#"
    $openSslLoc = "`"#{Deploy.HSMScripting.OpenSslLoc}#`""

    $IsOnDevEnvironment = $False #When set to $False: skips signing and accepting of the RSA request
    $CnValue = "#{Deploy.HSMScripting.CnValue}#" #should be [test.signing|acceptatie.signing|signing].coronamelder-api.nl
}

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

function SetErrorToStop
{
	$script:ErrorActionPreference = "Stop"
	write-host "Error-behaviour of script is set to $script:ErrorActionPreference."
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

function GenerateRequestInf([string] $filename, [string] $hashAlgorithm, [string] $keyAlgorithm, [string] $keyLength, [string] $friendlyName, [bool] $AddOIDs = $true)
{
	$fileContent = `
"[Version]`
Signature = `$Windows Nt`$`
[NewRequest]`
Subject = `"C=NL, ST=Zuid-Holland, L=Den Haag, O=CIBG, OU=CIBG, SerialNumber=00000002006756402002, CN=$script:CnValue`"`
Exportable = FALSE`
HashAlgorithm = $hashAlgorithm`
KeyAlgorithm = $keyAlgorithm`
KeyLength = $keyLength`
KeySpec = AT_SIGNATURE`
KeyUsage = 0xa0`
KeyUsageProperty = 2`
MachineKeySet = True`
RequestType = PKCS10`
ProviderName = `"Utimaco CryptoServer key storage Provider`"`
ProviderType = 1`
FriendlyName = $friendlyName"
# tabs aren't ignored...

if($AddOIDs -eq $true)
{
	$fileContent = $fileContent + "`n[EnhancedKeyUsageExtension]`nOID = 1.3.6.1.5.5.7.3.2 ; Client Auth`nOID = 1.3.6.1.5.5.7.3.1 ; Server Auth"
}
	
	New-Item -force -name ($filename + ".inf") -path "." -ItemType File -Value $fileContent -ErrorAction Stop
}


#
# Start
#


write-host "RSA-certificate renewal script for Utimaco HSM"
write-host "Location and date: $env:computername. $(Get-Date -Format `"dd MMM, HH:mm:ss`")."
CheckNotIse

write-warning "`nPlease check the following:`
- Using variables from $VariablesSource. Correct?`
- Checked the values in the GenerateRequestInf-function?
- `$IsOnDevEnvironment is $IsOnDevEnvironment. Correct?`
- `$CnValue is $CnValue. Correct?`
- (Is the simulator on?)`
If not: abort this script with Ctrl+C."
Pause

SetErrorToStop

write-host "`nPre-check for key presence"
Pause

RunWithErrorCheck "`"$HSMAdminToolsDir\cngtool`" listkeys"

write-host "`nGenerate requestfile"
Pause

$date = Get-Date -Format "MM_dd_HH-mm-ss"
	
$requestFileName = read-host "Enter a preferred name for the request files to be generated"
$Host.UI.RawUI.FlushInputBuffer() #clears any annoying newlines that were accidentally copied in

$requestRSAname = ".\Temp$script:date\$requestFileName-Req"
$signedrequestRSAname = ".\Temp$script:date\$requestFileName-Signed"

$FriendlyName = read-host "`nPlease enter a `'Friendly name`' for the certificates.`n Make sure the name is not already in use! (look inside the machine personal keystore)"
$Host.UI.RawUI.FlushInputBuffer() #clears any annoying newlines that were accidentally copied in
	
GenerateRequestInf -filename $requestRSAname -hashAlgorithm "SHA256" -keyAlgorithm "RSA" -keyLength "2048" -friendlyName "$FriendlyName-RSA"
	
write-host "`nSend request to HSM to generate new keypair"
Pause

RunWithErrorCheck "certreq -new $requestRSAname.inf $requestRSAname.csr"

if($IsOnDevEnvironment)
{	
	write-host "`nYou need to sign the request with the self-signed root-certificate from the keygen-folder."
	$RootCertLocation = read-host "`nPlease enter the location and name of the files, without extension"
	$Host.UI.RawUI.FlushInputBuffer()
	
	RunWithErrorCheck "$openSslLoc x509 -req -in $requestRSAname.csr -set_serial $(Get-Random) -days $CertLifeTimeDays -CA $RootCertLocation.pem -CAkey $RootCertLocation.key -out $signedrequestRSAname.pem"
	
	write-host "`nSending signed request to HSM"
	Pause
	
	RunWithErrorCheck "certreq -accept -machine $signedrequestRSAname.pem"
}

write-host "`nPost-check for key presence"
Pause

RunWithErrorCheck "`"$HSMAdminToolsDir\cngtool`" listkeys"
Pause

write-host "`nDone!"

if($IsOnDevEnvironment -eq $False)
{
	write-host "`nThe RSA request-file for PKIO is $requestRSAname."
}

write-host "`nOpening the local machine store.`nThe renewed cert should be present under personal certificates."
Pause

RunWithErrorCheck "certlm.msc"