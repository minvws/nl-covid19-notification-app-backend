# Not designed for Powershell ISE
# Not designed for Windows 7
# Double-check you are allowed to run custom scripts.

$testfileNameNoExt
$testfileName
$tempFolderLoc
$RsaRootCertLoc
$EcdsaCertLoc
$VariablesSource = "Development"

if("#{Deploy.HSMScripting.OpenSslLoc}#" -like "*Deploy.HSMScripting.OpenSslLoc*")
{
	#dev
    $OpenSslLoc = "`"C:\Applications\OpenSSL\bin\openssl.exe`""
    $HSMAdminToolsDir = "C:\Program Files\Utimaco\CryptoServer\Administration"
    $SignerLoc = ".\..\..\SigTestFileCreator\bin\Debug\netcoreapp3.1\SigTestFileCreator.exe"
    $ScrubberLoc = ".\..\..\ProtobufScrubber\bin\Debug\netcoreapp3.1\ProtobufScrubber.exe"
    
    $RsaRootCertThumbPrint = "235930c0869a8d84b3cb0a9379522a4b0b4dbe0b"
    $EcdsaCertThumbPrint = "d5b4ed5ddd8f6492a3c859792709570e9cc0a2ce"
	$Hsm1Address = "3001@127.0.0.1"
	$Hsm2Address = ""
}
else
{
	#test, accp and prod
	$VariablesSource = "Deploy"
    $OpenSslLoc = "`"#{Deploy.HSMScripting.OpenSslLoc}#`""
    $HSMAdminToolsDir = "#{Deploy.HSMScripting.HSMAdminToolsDir}#"
    $SignerLoc = "`"#{Deploy.HSMScripting.VerifierLoc}#`""
    $ScrubberLoc = "`"#{Deploy.HSMScripting.EksParserLoc}#`""
    
    $RsaRootCertThumbPrint = "#{Deploy.HSMScripting.RsaRootCertificateThumbprint}#"
    $EcdsaCertThumbPrint = "#{Deploy.HSMScripting.EcdsaCertThumbPrint}#"
	$Hsm1Address = "#{Deploy.HSMScripting.HSM1Address}#"
	$Hsm2Address = "#{Deploy.HSMScripting.HSM2Address}#"
}

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

function CheckNotWin7
{
	if([int](Get-WmiObject Win32_OperatingSystem).BuildNumber -lt 9000)
	{
		write-warning "`nAutomated export of certificates is not possible under Windows 7!`nThis script will not function."
		Pause
		
		exit
	}
}

function GenTestFileAndFolders
{
	$date = Get-Date -Format "MM_dd_HH-mm-ss"
	$fileContent = "Key verification file: $date"
	$script:tempFolderLoc = "Temp-$date"
	$script:testfileNameNoExt = "keytest$date"
	$script:testfileName = ("keytest$date" + ".txt") 
	
	New-Item -force -name ($script:testfileName) -path ".\$script:tempFolderLoc\" -ItemType File -Value $fileContent -ErrorAction Stop
	New-Item -force -name "$script:testfileNameNoExt-RSA" -path ".\$script:tempFolderLoc\" -ItemType Directory
	New-Item -force -name "$script:testfileNameNoExt-ECDSA" -path ".\$script:tempFolderLoc\" -ItemType Directory
}

function ExtractCert ([String] $ThumbPrint, [String] $Store, [String] $ExportPath)
{
	$cert = Get-ChildItem -Path "cert:\LocalMachine\$Store\$ThumbPrint"
	$collector = Export-Certificate -Cert $cert -FilePath "$ExportPath.cer" -Type CERT
	
	# the exported cert is in der-format and needs to be converted to pem-format to use for signature verification
	RunWithErrorCheck "$openSslLoc x509 -in $ExportPath.cer -inform der -out $ExportPath-pem.cer -outform pem"
	
	#collector prevents black magic from occurring.
	return "$ExportPath-pem.cer"
}

function TestHsmConnection
{
	& "$HSMAdminToolsDir\csadm" Dev=$Hsm1Address GetState
	
	if($Hsm2Address -ne "")
	{
		& "$HSMAdminToolsDir\csadm" Dev=$Hsm2Address GetState
	}
}


#
# Start
#


write-host "Certificate-Verifier"
write-host "Location and date: $env:computername. $(Get-Date -Format `"dd MMM, HH:mm:ss`")."
CheckNotWin7
CheckNotIse

write-warning "`nPlease check the following:`
- Using variables from $VariablesSource. Correct?`
- Rsa ROOT thumbprint is $RsaRootCertThumbPrint. Correct?`
- Ecdsa thumbprint is $EcdsaCertThumbPrint. Correct?`
- Signer is located at $SignerLoc. Correct?`
- Scrubber is located at $ScrubberLoc. Correct?`
- HSM-addresses are $Hsm1Address and $Hsm2Address. Correct?`
- Did you add the Rsa (non-root)- and Ecdsa thumbprint to the signer appsettings.json?`
If not: abort this script with Ctrl+C."
Pause

SetErrorToStop

write-host "`nCheck if HSM is accessible"
Pause

#TestHsmConnection

write-host "`nGenerating testfile"
Pause

GenTestFileAndFolders

write-host "`nExtracting certificates"
Pause

if($VariablesSource -eq "Development")
{
	#use local RSA cert for verification instead of root cert 
	$RsaRootCertLoc = ExtractCert -ThumbPrint $RsaRootCertThumbPrint -Store "my" -ExportPath ".\$tempFolderLoc\$testfileNameNoExt-RSA\RsaRootCert"
}
else
{
	$RsaRootCertLoc = ExtractCert -ThumbPrint $RsaRootCertThumbPrint -Store "root" -ExportPath ".\$tempFolderLoc\$testfileNameNoExt-RSA\RsaRootCert"
}

$EcdsaCertLoc = ExtractCert -ThumbPrint $EcdsaCertThumbPrint -Store "my" -ExportPath ".\$tempFolderLoc\$testfileNameNoExt-ECDSA\EcdsaCert"

#extract public key from ECDSA cert
RunWithErrorCheck "$openSslLoc x509 -in $EcdsaCertLoc -inform pem -noout -pubkey -out $EcdsaCertLoc.pub"

write-host "`nSigning testfile with SigTestFileCreator and scrubbing protobuf-header"
Pause

RunWithErrorCheck "$SignerLoc .\$tempFolderLoc\$testfileName"
RunWithErrorCheck "$ScrubberLoc .\$tempFolderLoc\$testfileNameNoExt-signed.zip"

write-host "`nChecking signature of signed testfiles"
Pause

Expand-Archive -Force -LiteralPath ".\$tempFolderLoc\$testfileNameNoExt-signed.zip" -DestinationPath ".\$tempFolderLoc\$testfileNameNoExt-RSA\" -ErrorAction Stop
Expand-Archive -Force -LiteralPath ".\$tempFolderLoc\$testfileNameNoExt-signed-scrubbed.zip" -DestinationPath ".\$tempFolderLoc\$testfileNameNoExt-ECDSA\" -ErrorAction Stop

write-host "`nRSA: "
RunWithErrorCheck "$openSslLoc cms -verify -CAfile $RsaRootCertLoc -in .\$tempFolderLoc\$testfileNameNoExt-RSA\content.sig -inform DER -binary -content .\$tempFolderLoc\$testfileNameNoExt-RSA\export.bin -purpose any"

write-host "`nECDSA: "
RunWithErrorCheck "$openSslLoc dgst -sha256 -verify $EcdsaCertLoc.pub -signature .\$tempFolderLoc\$testfileNameNoExt-ECDSA\export.sig .\$tempFolderLoc\$testfileNameNoExt-ECDSA\export.bin"

write-host "`nIf both checks return a succesful verification, then we're done!"
Pause
