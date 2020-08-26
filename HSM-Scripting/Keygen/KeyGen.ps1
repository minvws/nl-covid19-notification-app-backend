# NOT designed for Powershell ISE
# Double-check you are allowed to run custom scripts.

$cngtoolloc = "`"C:\Program Files\Utimaco\CryptoServer\Administration\cngtool.exe`""
$openSslLoc = "`"C:\Program Files\OpenSSL-Win64\bin\openssl.exe`""

$keynameCert
$keynameRSA
$keynameECDSA
$selfsigncertname
$requestRSAname
$signedrequestRSAname
$requestECDSAname
$signedrequestECDSAname

function GenerateRequestInf([string] $filename, [string] $keyname, [string] $hashAlgorithm, [string] $keyAlgorithm, [string] $keyLength, [string] $friendlyName, [bool] $AddOIDs = $true)
{
#'Issued To'-column is defined by $keyname
#'Issued By'-column is defnied by $keynameCert

	$fileContent = `
"[Version]`
Signature = `$Windows Nt`$`
[NewRequest]`
Subject = `"C=NL, ST=Zuid-Holland, L=Den Haag, O=CIBG, OU=CIBG/serialNumber=00000002006756402002, CN=signing.coronamelder-api.nl`"`
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

function RunWithErrorCheck ([string]$command) 
{
	iex "& $command" #I'm not frustrated at all

    if($lastexitcode -ne 0)
    {
        write-Warning "Script terminated due to an error. :("
		Read-Host 'Press Enter to continue.'
        exit
    }
}

function SetKeyFilenames ()
{
	$script:keynameCert = read-host "Enter the preferred name of your keys"
	$script:selfsigncertname = $keynameCert + "CA"
	
	$script:keynameRSA = $keynameCert + "RSA"
	$script:requestRSAname = $keynameRSA + "Req"
	$script:signedrequestRSAname = $keynameRSA + "Signed"
	
	$script:keynameECDSA = $keynameCert + "ECDSA"
	$script:requestECDSAname = $keynameECDSA + "Req"
	$script:signedrequestECDSAname = $keynameECDSA + "Signed"
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

function SetErrorToStop
{
	$ErrorAtStart = $ErrorActionPreference
	$ErrorActionPreference = "Stop"
	write-host "Error-behaviour is set from $ErrorAtStart to $ErrorActionPreference."
}


#
# Start
#

write-host "Keygenerator script for Utimaco HSM"
Pause

write-warning "Did you do the following?`
- Checked the values in the GenerateRequestInf-function?
If not: abort this script with ctrl+c."
Pause

SetErrorToStop

if($host.name -match "ISE")
{
	write-host "`nYou are running this script in Powershell ISE. Please switch to the regular Powershell."
	Pause
	
	exit
}

write-host "`nPre-check for key presence"
Pause

RunWithErrorCheck "$cngtoolloc listkeys"

write-host "`nGenerate self-signed certificate"
Pause

SetKeyFileNames

RunWithErrorCheck "$openSslLoc req -new -x509 -nodes -subj /CN=$keynameCert -keyout $selfsigncertname.key -out $selfsigncertname.pem"

write-host "`nStoring certificate in machine root store"
Pause

RunWithErrorCheck "certutil -addstore -f `"root`" $selfsigncertname.pem"

write-host "`nGenerate requestfiles for both keys"
Pause

GenerateRequestInf -filename $requestRSAname -keyname $keynameRSA -hashAlgorithm "SHA256" -keyAlgorithm "RSA" -keyLength "2048" -friendlyName "RSATestKeyRequest"
GenerateRequestInf -filename $requestECDSAname -keyname $keynameECDSA -hashAlgorithm "SHA256" -keyAlgorithm "ECDSA_P256" -keyLength "256" -friendlyName "ECDSATestKeyRequest" -AddOIDs $false

write-host "`nSend requests to HSM to generate key"
Pause

RunWithErrorCheck "certreq -new $requestRSAname.inf $requestRSAname.csr"
RunWithErrorCheck "certreq -new $requestECDSAname.inf $requestECDSAname.csr"

write-host "`nSign request files with certificate"
Pause

RunWithErrorCheck "$openSslLoc x509 -req -in $requestRSAname.csr -set_serial 1234 -CA $selfsigncertname.pem -CAkey $selfsigncertname.key -out $signedrequestRSAname.pem"
RunWithErrorCheck "$openSslLoc x509 -req -in $requestECDSAname.csr -set_serial 1234 -CA $selfsigncertname.pem -CAkey $selfsigncertname.key -out $signedrequestECDSAname.pem"

write-host "`nSending signed requests to HSM"
Pause

RunWithErrorCheck "certreq -accept -machine $signedrequestRSAname.pem"
RunWithErrorCheck "certreq -accept -machine $signedrequestECDSAname.pem"

write-host "`nPost-check for key presence"
Pause

RunWithErrorCheck "$cngtoolloc listkeys"
Pause

if([int](Get-WmiObject Win32_OperatingSystem).BuildNumber -lt 9000)
{
	#Windows 7 doesn't have certlm.msc
	write-host "`nWindows 7 cannot automatically open the local machine keystore`nWe'll drop you off at the mmc. :)"
	Pause
	
	RunWithErrorCheck "mmc"
}
else
{
	write-host "`nOpening local machine store.`nCerts should be present under personal certificates and root certificates."
	Pause
	
	RunWithErrorCheck "certlm.msc"
}