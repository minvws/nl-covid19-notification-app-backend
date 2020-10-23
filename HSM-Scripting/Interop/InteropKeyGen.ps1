# Not designed for Powershell ISE
# Not designed for Windows 7
# Double-check you are allowed to run custom scripts.

$date
$VariablesSource = "Development"
$Environment = "#{Deploy.HSMScripting.Environment}#"
$FriendlyName-Client
$FriendlyName-Signing
$TempPassword = "temppassword" #To get around the annoying p12-encryption in certutil -importpfx

if("#{Deploy.HSMScripting.OpenSslLoc}#" -like "*Deploy.HSMScripting.OpenSslLoc*")
{
	#dev
    $HSMAdminToolsDir = "C:\Program Files\Utimaco\CryptoServer\Administration"
    $openSslLoc = "`"C:\Program Files\OpenSSL-Win64\bin\openssl.exe`""
    $CnValue = "ontw.interop-signing.coronamelder-api.nl" #([ontw|test|acceptatie|productie].interop-signing.coronamelder-api.nl)
    $CertDays = 3650 #cert lifetime of 10 years.
    $SelfSignSubject = "/C=NL/ST=Zuid-Holland/L=Den Haag/O=TestOrganisation/OU=TestOrganisation/CN=$CnValue"
}
else
{
	#test, accp, prod
	$VariablesSource = "Deploy"
    $HSMAdminToolsDir = "#{Deploy.HSMScripting.HSMAdminToolsDir}#"
    $openSslLoc = "`"#{Deploy.HSMScripting.OpenSslLoc}#`""
    $CnValue = "#{Deploy.HSMScripting.InteropCnValue}#" #should be [test.signing|acceptatie.signing|signing].coronamelder-api.nl
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

function GenerateRequestInf([string] $filename, [string] $KeyUsage, [string] $friendlyName, [bool] $AddOIDs = $true)
{
	$fileContent = `
"[Version]`
Signature = `$Windows Nt`$`
[NewRequest]`
Subject = `"C=NL, ST=Zuid-Holland, L=Den Haag, O=CIBG, OU=CIBG, CN=$script:CnValue`"`
Exportable = FALSE`
HashAlgorithm = SHA256`
KeyAlgorithm = RSA`
KeySpec = AT_SIGNATURE`
KeyLength = 4096`
KeyUsage = $KeyUsage`
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
	
	$script:baseName = "$script:folderName\" + $(read-host "Enter the preferred name for the certificate files")
	
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
- `$Environment is $Environment. Correct?`
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

New-Item -force -name $folderName -path "." -ItemType Directory -ErrorAction Stop

$FriendlyName = read-host "`nPlease enter a `'Friendly name`' for the certificates.`n Make sure the name is not already in use! (look inside the machine personal keystore)"

$FriendlyName-Client = "$FriendlyName-Client"
$FriendlyName-Signing = "$FriendlyName-Signing"


SetCertFileNames

#Generate Client cert
if($Environment -eq "ontw")
{
	write-host "`nGenerate self-signed Client certificate. HSM not involved"
	Pause
	
	RunWithErrorCheck "$openSslLoc req -new -x509 -nodes -days $CertDays -subj `"$SelfSignSubject`" -keyout $clientName.key -out $clientname.pem"
	RunWithErrorCheck "$openSslLoc pkcs12 -export -in $clientName.pem -inkey $clientName.key -nodes -passout pass:$TempPassword -out $clientName.p12"
	
	write-host "`nAdding Client certificate to local machine store"
	RunWithErrorCheck "certutil -importpfx -p $TempPassword -f $clientName.p12"
}
else
{
	write-host "`nGenerate Client certificate-request"
	Pause
	
	GenerateRequestInf -filename $clientName -KeyUsage "0xA0" -friendlyName "$FriendlyName-Client"
	
	write-host "`nSend request to HSM to generate private key"
	Pause

	RunWithErrorCheck "certreq -new $clientName.inf $clientName.csr"
}

#Generate Signing cert
if($Enviromnent -eq "ontw")
{
	write-host "`nGenerate self-signed Signing certificate. HSM not involved"
	Pause
	
	RunWithErrorCheck "$openSslLoc req -new -x509 -nodes -days $CertDays -subj `"$SelfSignSubject`" -keyout $signerName.key -out $signerName.pem"
	RunWithErrorCheck "$openSslLoc pkcs12 -export -in $signerName.pem -inkey $signerName.key -nodes -passout pass:$TempPassword -out $signerName.p12"
	
	write-host "`nAdding Client certificate to local machine store"
	RunWithErrorCheck "certutil -importpfx -p $TempPassword -f $signerName.p12"
}
else
{
	write-host "`nGenerate Signing certificate-request."
	Pause
	
	GenerateRequestInf -filename $signerName -KeyUsage "0x80" -friendlyName "$FriendlyName-Signing" -AddOIDs $False
	
	write-host "`nSend request to HSM to generate private key"
	Pause

	RunWithErrorCheck "certreq -new $signerName.inf $signerName.csr"
}

write-host "`nPost-check for key presence"
Pause

RunWithErrorCheck "`"$HSMAdminToolsDir\cngtool`" listkeys"
Pause

if($Environment -eq "ontw")
{	
	write-host "`nDone! The self-signed certificates can be found in the local machine personal store"
}
else
{
	write-host "`nDone! The Client certificate request is $clientName.csr.`nThe Signing certificate request is $SigningName.csr."
}