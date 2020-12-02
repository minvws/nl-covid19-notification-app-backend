# Not designed for Powershell ISE
# Not designed for Windows 7
# Double-check you are allowed to run custom scripts.

$TestfileName
$TestfileNameNoExt
$date
$VariablesSource = "Development"

if("#{Deploy.HSMScripting.OpenSslLoc}#" -like "*Deploy.HSMScripting.OpenSslLoc*")
{
	#dev
    $OpenSslLoc = "`"C:\Program Files\OpenSSL-Win64\bin\openssl.exe`""
    $HSMAdminToolsDir = "C:\Program Files\Utimaco\CryptoServer\Administration"
    $SignerLoc = "..\..\SigTestFileCreator\SigTestFileCreator.exe"
    $Environment = "Ontw"
    
	$EcdsaCertThumbPrint = "a9102034f7056621155c608925b7c4eae7b241b1"
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
	$Environment = "#{Deploy.HSMScripting.Environment}#"
    
	$EcdsaCertThumbPrint = "#{Deploy.HSMScripting.EcdsaCertThumbPrint}#"
	$Hsm1Address = "#{Deploy.HSMScripting.HSM1Address}#"
	$Hsm2Address = "#{Deploy.HSMScripting.HSM2Address}#"
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

function CheckNotWin7
{
	if([int](Get-WmiObject Win32_OperatingSystem).BuildNumber -lt 9000)
	{
		write-warning "`nAutomated export of certificates is not possible under Windows 7!`nThis script will not function."
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

function GenTestFile
{
	$contentstring = read-host "Please input a test string that will be used for signing"
	$script:date = Get-Date -Format "MM_dd_HH-mm-ss"
	$fileContent = "Key verification file ($script:Environment)`r`n$contentstring"
	$script:TestfileNameNoExt = "KeyExtractionRawData"
	$script:TestfileName = "$script:TestfileNameNoExt.txt"
	
	New-Item -force -name ($script:TestfileName) -path ".\Temp$script:date\" -ItemType File -Value $fileContent -ErrorAction Stop
	New-Item -force -name "Result$script:date" -path "." -ItemType Directory -ErrorAction Stop
}

function ExtractKey ([String] $ThumbPrint, [String] $Store, [String] $ExportPath)
{
	$cert = Get-ChildItem -Path "cert:\LocalMachine\$Store\$ThumbPrint"
	#collector prevents black magic from occurring.
	$collector = Export-Certificate -Cert $cert -FilePath ".\Temp$script:date\$ExportPath.cer" -Type CERT
	
	# the exported cert is in der-format and needs to be converted to pem-format to use for signature verification
	RunWithErrorCheck "$openSslLoc x509 -in .\temp$script:date\$ExportPath.cer -inform der -pubkey -noout -out .\Temp$script:date\$script:Environment-Ecdsa.pub"
	RunWithErrorCheck "$openSslLoc ec -in .\Temp$script:date\$script:Environment-Ecdsa.pub -text -pubin -out .\Result$script:date\$script:Environment-Ecdsa.txt"
}

function TestHsmConnection
{
	RunWithErrorCheck "`"$HSMAdminToolsDir\csadm`" Dev=$Hsm1Address GetState" 
	
	if($Hsm2Address -ne "")
	{
		RunWithErrorCheck "`"$HSMAdminToolsDir\csadm`" Dev=$Hsm2Address GetState" 
	}
}


#
# Start
#


write-host "Certificate-Verifier"
CheckNotWin7
CheckNotIse

write-warning "`nPlease check the following:`
- Using variables from $VariablesSource. Correct?`
- The Ecdsa-thumbprint is $EcdsaCertThumbPrint. Correct?`
- Signer is located at $SignerLoc. Correct?`
- HSM-addresses are $Hsm1Address and $Hsm2Address. Correct?`
- Did you add the Rsa (non-root)- and NEW Ecdsa thumbprint to the signer appsettings.json?`
- Does the signer appsettings.json have the correct AppBundleId, VerificationKeyId and VerificationKeyVersion?`
If not: abort this script with Ctrl+C."
Pause

SetErrorToStop

write-host "`nCheck if HSM is accessible"
Pause

TestHsmConnection

write-host "`nGenerating testfile"
Pause

gentestfile

write-host "`nExtracting public key"
Pause

ExtractKey -ThumbPrint $EcdsaCertThumbPrint -Store "my" -ExportPath "EcdsaCert"

write-host "`nSigning testfile with SigTestFileCreator"
Pause

RunWithErrorCheck "$SignerLoc .\Temp$script:date\$TestfileName"

Expand-Archive -Force -LiteralPath ".\Temp$script:date\$testfileNameNoExt-eks.zip" -DestinationPath ".\Result$script:date\" -ErrorAction Stop

write-host "`nDone!`nYou can take export.bin, export.sig and the .txt-file from the results folder now."
Pause
