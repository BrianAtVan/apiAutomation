Param(
    [parameter(Mandatory=$true)][string]$configPath,
    [parameter(Mandatory=$false)][string]$targetAPI = "QA"
    )

function DnnWriteLog($obj)
{
    Write-Host "$(Get-Date) => $obj"
}

$configFile = ([System.IO.FileInfo]$configPath).FullName
attrib -R $configFile

try
{
    $xml = [xml](get-content $configFile)

    foreach($n in $xml.selectnodes("//configuration/appSettings/add"))
    {
        switch($n.key)
        {
            "TargetApiEnvironment" { $n.value = $targetAPi }
        }
    }

    $xml.Save($configFile)
    exit 0
}
catch
{
    DnnWriteLog "Error: failed updating $configFile"
    Write-Error $error[0]
    exit 1
}
