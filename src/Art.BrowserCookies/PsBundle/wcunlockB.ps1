function Invoke-UnprotectSystem {
    Param(
    $InputFile,
    $EntropyFile,
    $OutputFile
    )
    $Script = {
        $SourceData = Get-Content -Path "$($args[0])" -Encoding Byte -Raw
        $EntropyData = Get-Content -Path "$($args[1])" -Encoding Byte -Raw
        Add-Type -AssemblyName System.Security
        $Unprotected = [Security.Cryptography.ProtectedData]::Unprotect($SourceData, $EntropyData, [Security.Cryptography.DataProtectionScope]::CurrentUser)
        Set-Content -Path "$($args[2])" -Encoding Byte -Value $Unprotected
    }
    Invoke-CommandAs -ScriptBlock $Script -ArgumentList $InputFile,$EntropyFile,$OutputFile -AsSystem
}

function Invoke-DecryptBufferWithKey {
    Param(
    $InputFile,
    $KeyName,
    $OutputFile
    )
    $Script = {
        Add-Type -AssemblyName System.Security
        $Kn = "$($args[2])"
        $Msksp = [Security.Cryptography.CngProvider]::MicrosoftSoftwareKeyStorageProvider
        $Key = [Security.Cryptography.CngKey]::Open($Kn, $Msksp, 0)
        if (-Not [Security.Cryptography.CngKey]::Exists($Kn, $Msksp)) {
            exit
        }
        $SourceData = Get-Content -Path "$($args[0])" -Encoding Byte -Raw
        switch ($Key.Algorithm.Algorithm) {
            "AES" {
                $Aes = New-Object Security.Cryptography.AesCng($Kn, $Msksp, 0)
                $Aes.IV = [byte[]]::new(16)
                $Aes.Mode = [System.Security.Cryptography.CipherMode]::CBC
                $Aes.Padding = [System.Security.Cryptography.PaddingMode]::None
                $Decryptor = $Aes.CreateDecryptor()
                $Stream = New-Object System.IO.MemoryStream
                $CryptoStream = New-Object System.Security.Cryptography.CryptoStream($Stream, $Decryptor, [System.Security.Cryptography.CryptoStreamMode]::Write)
                $CryptoStream.Write($SourceData, 0, $SourceData.Length)
                $CryptoStream.Flush()
                Set-Content -Path "$($args[2])" -Encoding Byte -Value $Stream.ToArray()
            }
        }
    }
    Invoke-CommandAs -ScriptBlock $Script -ArgumentList $InputFile,$KeyName,$OutputFile -AsSystem
}
