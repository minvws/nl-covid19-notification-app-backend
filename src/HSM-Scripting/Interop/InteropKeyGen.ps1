# Not designed for Powershell ISE
# Not designed for Windows 7
# Double-check you are allowed to run custom scripts.

$VariablesSource = "Development"
$TempPassword = "temppassword" #To get around the annoying p12-encryption in certutil -importpfx

if("#{Deploy.HSMScripting.OpenSslLoc}#" -like "*Deploy.HSMScripting.OpenSslLoc*")
{
	#dev
	$IsOnDevEnvironment = $true #When set to false: don't sign certificate requests
    $HSMAdminToolsDir = "C:\Program Files\Utimaco\CryptoServer\Administration"
    $openSslLoc = "`"C:\Program Files\OpenSSL-Win64\bin\openssl.exe`""
    $CnValueClient = "ontw.interop-client.coronamelder-api.nl"
	$CnValueSigner = "ontw.interop-signing.coronamelder-api.nl"
    $SelfSignSubject = "/C=NL/ST=Zuid-Holland/L=Den Haag/O=TestOrganisation/OU=TestOrganisation/CN="
	$CertDays = 3650 #cert lifetime of 10 years.
}
else
{
	#test, accp, prod
	$IsOnDevEnvironment = $false
	$VariablesSource = "Deploy"
    $HSMAdminToolsDir = "#{Deploy.HSMScripting.HSMAdminToolsDir}#"
    $openSslLoc = "`"#{Deploy.HSMScripting.OpenSslLoc}#`""
    $CnValueClient = "#{Deploy.HSMScripting.InteropCnValueClient}#" #([test|acceptatie|productie].interop-client.coronamelder-api.nl)
    $CnValueSigner = "#{Deploy.HSMScripting.InteropCnValueSigner}#" #([test|acceptatie|productie].interop-signing.coronamelder-api.nl)
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

function GenerateRequestInf([string] $filename, [string] $CnValue, [string] $KeyUsage, [string] $friendlyName, [bool] $AddOIDs = $true)
{
	$fileContent = `
"[Version]`
Signature = `$Windows Nt`$`
[NewRequest]`
Subject = `"C=NL, ST=Zuid-Holland, L=Den Haag, O=CIBG, OU=CIBG, CN=$CnValue`"`
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
	$script:signerName = "$script:baseName-Signing"
	$script:signerSigned = "$script:signerName-signed"
}


#
# Start
#


write-host "Certificate creation-script for interop certificates with Utimaco HSM"
CheckNotIse
CheckNotWin7

write-warning "`nPlease check the following:`
- Using variables from $VariablesSource. Correct?`
- Checked the values in the GenerateRequestInf-function?
- `$IsOnDevEnvironment is $IsOnDevEnvironment. Correct?`
- `$CnValueClient is $CnValueClient. Correct?`
- `$CnValueSigner is $CnValueSigner. Correct?`
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

$FriendlyNameClient = "$FriendlyName-Client"
$FriendlyNameSigning = "$FriendlyName-Signing"

#Generate Client cert
if($IsOnDevEnvironment -eq $true)
{
	write-host "`nGenerate self-signed Client certificate. HSM not involved"
	Pause
	
	RunWithErrorCheck "$openSslLoc req -new -x509 -nodes -days $CertDays -subj `"$SelfSignSubject$CnValueClient`" -keyout $clientName.key -out $clientname.pem"
	RunWithErrorCheck "$openSslLoc pkcs12 -export -in $clientName.pem -inkey $clientName.key -nodes -passout pass:$TempPassword -out $clientName.p12"
	
	write-host "`nAdding Client certificate to local machine store"
	RunWithErrorCheck "certutil -importpfx -p $TempPassword -f $clientName.p12"
}
else
{
	write-host "`nGenerate Client certificate-request"
	Pause
	
	GenerateRequestInf -filename $clientName -CnValue $CnValueClient -KeyUsage "0xA0" -friendlyName "$FriendlyNameClient"
	
	write-host "`nSend request to HSM to generate private key"
	Pause

	RunWithErrorCheck "certreq -new $clientName.inf $clientName.csr"
}

#Generate Signing cert
if($IsOnDevEnvironment -eq $true)
{
	write-host "`nGenerate self-signed Signing certificate. HSM not involved"
	Pause
	
	RunWithErrorCheck "$openSslLoc req -new -x509 -nodes -days $CertDays -subj `"$SelfSignSubject$CnValueSigner`" -keyout $signerName.key -out $signerName.pem"
	RunWithErrorCheck "$openSslLoc pkcs12 -export -in $signerName.pem -inkey $signerName.key -nodes -passout pass:$TempPassword -out $signerName.p12"
	
	write-host "`nAdding Client certificate to local machine store"
	RunWithErrorCheck "certutil -importpfx -p $TempPassword -f $signerName.p12"
}
else #Test could work with a self-signed signing cert, but doing that with the HSM is hard
{
	write-host "`nGenerate Signing certificate-request."
	Pause
	
	GenerateRequestInf -filename $signerName -CnValue $CnValueSigner -KeyUsage "0x80" -friendlyName "$FriendlyNameSigning" -AddOIDs $False
	
	write-host "`nSend request to HSM to generate private key"
	Pause

	RunWithErrorCheck "certreq -new $signerName.inf $signerName.csr"
}

if($IsOnDevEnvironment -eq $true)
{	
	write-host "`nDone! The self-signed certificates can be found in the local machine personal store.`nThe password for the certificates is `'$TempPassword`'."
}
else
{
	write-host "`nPost-check for key presence"
	Pause

	RunWithErrorCheck "`"$HSMAdminToolsDir\cngtool`" listkeys"
	Pause

	write-host "`nDone! The Client certificate request is $clientName.csr.`nThe Signing certificate request is $signerName.csr.`n"
	write-warning "Make sure to add the following when submitting the requests:`nClient: SubjectAltName`nSigning: 2-year validity period."
}