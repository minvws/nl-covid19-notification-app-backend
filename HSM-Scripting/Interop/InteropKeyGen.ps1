# Not designed for Powershell ISE
# Not designed for Windows 7
# Double-check you are allowed to run custom scripts.

$date
$VariablesSource = "Development"

if("#{Deploy.HSMScripting.OpenSslLoc}#" -like "*Deploy.HSMScripting.OpenSslLoc*")
{
	#dev
    $HSMAdminToolsDir = "C:\Program Files\Utimaco\CryptoServer\Administration"
    $openSslLoc = "`"C:\Program Files\OpenSSL-Win64\bin\openssl.exe`""

    $IsOnDevEnvironment = $True #When set to $False: skips sending, signing and accepting of the RSA request
    $CnValue = "ontw.coronamelder-api.nl" #should be [test.signing|acceptatie.signing|signing].coronamelder-api.nl
    $RootDays = 3650 #the dummy root is valid for 10 years.
    $RootSubject = "/C=NL/ST=Zuid-Holland/L=Den Haag/O=TestOrganisation/OU=TestOrganisation/CN=$CnValue"    
}
else
{
	#test, accp and prod
	$VariablesSource = "Deploy"
    $HSMAdminToolsDir = "#{Deploy.HSMScripting.HSMAdminToolsDir}#"
    $openSslLoc = "`"#{Deploy.HSMScripting.OpenSslLoc}#`""

    $IsOnDevEnvironment = $False #When set to $False: skips sending, signing and accepting of the RSA request
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

function CheckNotWin7
{
	if([int](Get-WmiObject Win32_OperatingSystem).BuildNumber -lt 9000)
	{
		write-warning "`nThe KeySpec-line in the generated .infs will crash certreq on win7!`nThis script will not function."
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
Subject = `"C=NL, ST=Zuid-Holland, L=Den Haag, O=CIBG, OU=CIBG, CN=$script:CnValue`"`
Exportable = FALSE`
HashAlgorithm = $hashAlgorithm`
KeyAlgorithm = $keyAlgorithm`
KeySpec = AT_SIGNATURE`
KeyLength = $keyLength`
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

function SetCertFileNames ()
{
	$script:date = Get-Date -Format "MM_dd_HH-mm-ss"
	$script:folderName = "Results-$script:date"
	
	$script:baseName = "$script:folderName\" + $(read-host "Enter the preferred name for the certificates")
	
	$script:clientName = "$script:baseName-Client"
	$script:clientSigned = "$script:clientName-signed"
	$script:signerName = "$script:baseName-Signer"
	$script:signerSigned = "$script:signerName-signed"
}


#
# Start
#


write-host "Certificate creation-script for interop certificates with Utimaco HSM"
CheckNotIse
#CheckNotWin7

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

write-host "`nGenerate requestfiles for both certificates"
Pause

SetCertFileNames

New-Item -force -name $folderName -path "." -ItemType Directory -ErrorAction Stop

$FriendlyName = read-host "`nPlease enter a `'Friendly name`' for the certificates.`n Make sure the name is not already in use! (look inside the machine personal keystore)"
GenerateRequestInf -filename $clientName -hashAlgorithm "SHA256" -keyAlgorithm "RSA" -keyLength "2048" -friendlyName "$FriendlyName-Client"
GenerateRequestInf -filename $signerName -hashAlgorithm "SHA256" -keyAlgorithm "RSA" -keyLength "2048" -friendlyName "$FriendlyName-Signing"

write-host "`nSend requests to HSM to generate private key"
Pause

RunWithErrorCheck "certreq -new $clientName.inf $clientName.csr"
RunWithErrorCheck "certreq -new $signerName.inf $signerName.csr"

#on test/accp/prod the RSA request is signed by PKIO
if($IsOnDevEnvironment -eq $True)
{
	write-host "`nGenerate self-signed Rootcertificate"
	Pause
	$rootCertName = "$script:baseName-Root"
	
	RunWithErrorCheck "$openSslLoc req -new -x509 -nodes -days $RootDays -subj `"$RootSubject`" -keyout $rootCertName.key -out $rootCertName.pem"

	write-host "`nStoring Rootcertificate in local machine store"
	Pause

	RunWithErrorCheck "certutil -addstore -f `"root`" $rootCertName.pem"

	write-host "`nSign request files with certificate"
	Pause

	RunWithErrorCheck "$openSslLoc x509 -req -in $clientName.csr -set_serial $(Get-Random) -days $RootDays -CA $rootCertName.pem -CAkey $rootCertName.key -out $clientName.pem"
	RunWithErrorCheck "$openSslLoc x509 -req -in $signerName.csr -set_serial $(Get-Random) -days $RootDays -CA $rootCertName.pem -CAkey $rootCertName.key -out $signerName.pem"
	
	write-host "`nSending signed requests to HSM"
	Pause

	RunWithErrorCheck "certreq -accept -machine $clientName.pem"
	RunWithErrorCheck "certreq -accept -machine $signerName.pem"
}

write-host "`nPost-check for key presence"
Pause

RunWithErrorCheck "`"$HSMAdminToolsDir\cngtool`" listkeys"
Pause

if($IsOnDevEnvironment -eq $False)
{	
	write-host "`nDone! The RSA request-files for PKIO are:`n$clientName.csr`n$signerName.csr."
}
else
{
	write-host "`nDone! The new certificates can be found in the local machine personal store."
}