# NOT designed for Powershell ISE
# Double-check you are allowed to run custom scripts.

if("#{Deploy.HSMScripting.OpenSslLoc}#" -like "*Deploy.HSMScripting.OpenSslLoc*") {
    $cngtoolloc = "`"C:\Program Files\Utimaco\CryptoServer\Administration\cngtool.exe`""
    $openSslLoc = "`"C:\Program Files\OpenSSL-Win64\bin\openssl.exe`""

    $IsOnDevEnvironment = $True #When set to $False: skips sending, signing and accepting of the RSA request
    $CnValue = "ontw.coronamelder-api.nl" #should be [test.signing|acceptatie.signing|signing].coronamelder-api.nl
    $RootDays = 3650 #the dummy root is valid for 10 years.
    $RootSubject = "/C=NL/ST=Zuid-Holland/L=Den Haag/O=CIBG/OU=CIBG/serialNumber=00000002006756402002/CN=$CnValue"

    $date
    $keynameCert
    $keynameRSA
    $keynameECDSA
    $selfsigncertname
    $requestRSAname
    $signedrequestRSAname
    $requestECDSAname
    $signedrequestECDSAname
} else {
    $cngtoolloc = "`"#{Deploy.HSMScripting.HSMAdminToolsDir}#\cngtool.exe`""
    $openSslLoc = "`"#{Deploy.HSMScripting.OpenSslLoc}#`""

    $IsOnDevEnvironment = $False #When set to $False: skips sending, signing and accepting of the RSA request
    $CnValue = "#{Deploy.HSMScripting.CnValue}#" #should be [test.signing|acceptatie.signing|signing].coronamelder-api.nl
    $RootDays = 3650 #the dummy root is valid for 10 years.
    $RootSubject = "/C=NL/ST=Zuid-Holland/L=Den Haag/O=CIBG/OU=CIBG/serialNumber=00000002006756402002/CN=$CnValue"

    $date
    $keynameCert
    $keynameRSA
    $keynameECDSA
    $selfsigncertname
    $requestRSAname
    $signedrequestRSAname
    $requestECDSAname
    $signedrequestECDSAname
}

$IsOnDevEnvironment = $True #When set to $False: skips sending, signing and accepting of the RSA request
$CnValue = "ontw.coronamelder-api.nl" #should be [test.signing|acceptatie.signing|signing].coronamelder-api.nl
$RootDays = 3650 #the dummy root is valid for 10 years.
$RootSubject = "/C=NL/ST=Zuid-Holland/L=Den Haag/O=CIBG/OU=CIBG/serialNumber=00000002006756402002/CN=$CnValue"

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

function SetKeyFilenames ()
{
	$script:date = Get-Date -Format "MM_dd_HH-mm-ss"
	
	$script:keynameCert = read-host "Enter the preferred name of the keyfiles"
	$script:selfsigncertname = ".\Temp$script:date\$script:keynameCert-Root"
	
	$script:keynameRSA = "$script:keynameCert-RSA"
	$script:requestRSAname = ".\Temp$script:date\$script:keynameRSA-Req"
	$script:signedrequestRSAname = ".\Temp$script:date\$script:keynameRSA-Signed"
	
	$script:keynameECDSA = "$script:keynameCert-ECDSA"
	$script:requestECDSAname = ".\Temp$script:date\$script:keynameECDSA-Req"
	$script:signedrequestECDSAname = ".\Temp$script:date\$script:keynameECDSA-Signed"
}

function GenRequests
{
	$FriendlyName = read-host "`nPlease enter a `'Friendly name`' for the certificates.`n Make sure the name is not already in use! (look inside the machine personal keystore)"
	
	GenerateRequestInf -filename $requestRSAname -hashAlgorithm "SHA256" -keyAlgorithm "RSA" -keyLength "2048" -friendlyName "$FriendlyName-RSA"
	GenerateRequestInf -filename $requestECDSAname -hashAlgorithm "SHA256" -keyAlgorithm "ECDSA_P256" -keyLength "256" -friendlyName "$FriendlyName-ECDSA" -AddOIDs $false
}

#
# Start
#

write-host "Keygenerator script for Utimaco HSM"
Pause

write-warning "Did you do the following?`
- Turn the simulator on?`
- Checked the variables `$IsOnDevEnvironment and `$CnValue? 
- Checked the values in the GenerateRequestInf-function?
If not: abort this script with ctrl+c."
Pause

SetErrorToStop
CheckNotIse

write-host "`nPre-check for key presence"
Pause

RunWithErrorCheck "$cngtoolloc listkeys"

write-host "`nGenerate self-signed certificate"
Pause

SetKeyFileNames

New-Item -force -name "Temp$date" -path "." -ItemType Directory -ErrorAction Stop
RunWithErrorCheck "$openSslLoc req -new -x509 -nodes -days $RootDays -subj `"$RootSubject`" -keyout $selfsigncertname.key -out $selfsigncertname.pem"

write-host "`nStoring certificate in machine root store"
Pause

RunWithErrorCheck "certutil -addstore -f `"root`" $selfsigncertname.pem"

write-host "`nGenerate requestfiles for both keys"
Pause

GenRequests

write-host "`nSend requests to HSM to generate key"
Pause

RunWithErrorCheck "certreq -new $requestRSAname.inf $requestRSAname.csr"
RunWithErrorCheck "certreq -new $requestECDSAname.inf $requestECDSAname.csr"

write-host "`nSign request files with certificate"
Pause

#on test/accp/prod the RSA request is signed by PKIO
if($IsOnDevEnvironment -eq $True)
{
	RunWithErrorCheck "$openSslLoc x509 -req -in $requestRSAname.csr -set_serial $(Get-Random) -days $RootDays -CA $selfsigncertname.pem -CAkey $selfsigncertname.key -out $signedrequestRSAname.pem"
}
RunWithErrorCheck "$openSslLoc x509 -req -in $requestECDSAname.csr -set_serial $(Get-Random) -days $RootDays -CA $selfsigncertname.pem -CAkey $selfsigncertname.key -out $signedrequestECDSAname.pem"

write-host "`nSending signed requests to HSM"
Pause

#on test/accp/prod the RSA request is accepted when PKIO retuns the signed version
if($IsOnDevEnvironment -eq $True)
{
	RunWithErrorCheck "certreq -accept -machine $signedrequestRSAname.pem"
}
RunWithErrorCheck "certreq -accept -machine $signedrequestECDSAname.pem"

write-host "`nPost-check for key presence"
Pause

RunWithErrorCheck "$cngtoolloc listkeys"
Pause

if($IsOnDevEnvironment -eq $False)
{	
	write-host "`nDone! The RSA-request file for PKIO is $requestRSAname."
}

if([int](Get-WmiObject Win32_OperatingSystem).BuildNumber -lt 9000)
{
	#Windows 7 doesn't have certlm.msc
	write-host "`nWindows 7 cannot automatically open the local machine keystore`nWe'll drop you off at the mmc. :)"
	Pause
	
	RunWithErrorCheck "mmc"
}
else
{
	write-host "`nOpening the local machine store.`nCerts should be present under personal certificates and root certificates."
	Pause
	
	RunWithErrorCheck "certlm.msc"
}
