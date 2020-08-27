# NOT designed for Powershell ISE
# Double-check you are allowed to run custom scripts.

$OpenSslLoc = "`"C:\Program Files\OpenSSL-Win64\bin\openssl.exe`""
$HSMAdminToolsDir = "C:\Program Files\Utimaco\CryptoServer\Administration"
$testfileNameNoExt = ""
$testfileName = ""
$verifierLoc = ".\Verifier\SigTestFileCreator.exe"
$RsaRootCertLoc = ""
$EcdsaCertLoc = ""
$EksParserLoc = ".\EksParser\EksParser.exe"

$RsaRootCertThumbPrint = "61d9cfc82f55cfbb4fb9f67ff5f35634832c7097"
$EcdsaCertThumbPrint = "fe3c560eeb63cb8f945e746194de4a168dba20d1"

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

function GenTestFile
{
	$date = Get-Date -Format "MM_dd_yyyy_HH-mm"
	$fileContent = "Key verification file: $date"
	$script:testfileNameNoExt = "keytest$date"
	$script:testfileName = ("keytest$date" + ".txt") 
	
	New-Item -force -name ($script:testfileName) -path "." -ItemType File -Value $fileContent -ErrorAction Stop
}

function ExtractCert ([String] $ThumbPrint, [String] $Store, [String] $ExportPath)
{
	$cert = Get-ChildItem -Path "cert:\LocalMachine\$Store\$ThumbPrint"
	$opvangbak = Export-Certificate -Cert $cert -FilePath "$ExportPath.cer" -Type CERT
	
	# the exported cert is in der-format and needs to be converted to pem-format to use for signature verification
	RunWithErrorCheck "$openSslLoc x509 -in $ExportPath.cer -inform der -out $ExportPath-pem.cer -outform pem"
	
	#opvangbak prevents black magic from occurring.
	return "$ExportPath-pem.cer"
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

#
# Start
#


write-host "Certificate-Verifier"
Pause

CheckNotWin7

write-warning "`nDid you do the following?`
- Add the Rsa ROOT- and Ecdsa thumbprint to this script?`
- Add the location of the verifier to this script?`
- Add the Rsa (non-root)- and Ecdsa thumbprint to the verifier appsettings.json?`
If not: abort this script with ctrl+c."
Pause

SetErrorToStop
CheckNotIse

write-host "`nCheck if HSM is accessible"
Pause

RunWithErrorCheck "`"$HSMAdminToolsDir\cngtool`" providerinfo"

write-host "`nGenerating testfile"
Pause

gentestfile

write-host "`nExtracting certificates"
Pause

$RsaRootCertLoc = ExtractCert -ThumbPrint $RsaRootCertThumbPrint -Store "root" -ExportPath "RsaRootCert"
$EcdsaCertLoc = ExtractCert -ThumbPrint $EcdsaCertThumbPrint -Store "my" -ExportPath "EcdsaCert"

#extract pubnlic key from ECDSA Key
RunWithErrorCheck "$openSslLoc x509 -in $EcdsaCertLoc -inform pem -noout -pubkey -out $EcdsaCertLoc.key"

write-host "`nSigning testfile with Verifier"
Pause

RunWithErrorCheck "$verifierLoc $testfileName"

write-host "`nChecking signature of signed testfiles"
Pause

Expand-Archive -Force -LiteralPath "$testfileNameNoExt-eks.zip" -DestinationPath ".\$testfileNameNoExt\" -ErrorAction Stop

write-host "`nRSA: "
RunWithErrorCheck "$openSslLoc cms -verify -CAfile $RsaRootCertLoc -in $script:testfileNameNoExt\content.sig -inform DER -binary -content $script:testfileNameNoExt\export.bin -purpose any"

write-host "`nECDSA: "
RunWithErrorCheck "$EksParserLoc $testfileNameNoExt-eks.zip"
RunWithErrorCheck "$openSslLoc dgst -sha256 -verify $EcdsaCertLoc.key -signature export.sig export.bin"

write-host "`nIf both checks return a succesful verification, then we're done!"
Pause