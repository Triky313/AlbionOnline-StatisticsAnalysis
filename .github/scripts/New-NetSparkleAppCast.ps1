param(
    [Parameter(Mandatory = $true)]
    [string] $TagName,

    [Parameter(Mandatory = $true)]
    [string] $Version,

    [Parameter(Mandatory = $true)]
    [string] $InstallerPath,

    [Parameter(Mandatory = $true)]
    [string] $Repository,

    [Parameter(Mandatory = $true)]
    [string] $OutputDirectory,

    [Parameter(Mandatory = $true)]
    [string] $AppCastFileName,

    [Parameter(Mandatory = $true)]
    [string] $ToolPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-NetSparkleSignature
{
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path,

        [Parameter(Mandatory = $true)]
        [string] $SignerPath
    )

    if (!(Test-Path -LiteralPath $Path))
    {
        throw "File to sign not found: $Path"
    }

    $output = & $SignerPath --generate-signature $Path
    if ($LASTEXITCODE -ne 0)
    {
        throw "NetSparkle signature generation failed for $Path."
    }

    foreach ($line in $output)
    {
        if ($line -match '^Signature:\s*(.+)$')
        {
            return $Matches[1].Trim()
        }
    }

    throw "NetSparkle signature output did not contain a signature for $Path."
}

function Write-AppCast
{
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path,

        [Parameter(Mandatory = $true)]
        [string] $Title,

        [Parameter(Mandatory = $true)]
        [string] $Description,

        [Parameter(Mandatory = $true)]
        [string] $ReleaseUrl,

        [Parameter(Mandatory = $true)]
        [string] $Version,

        [Parameter(Mandatory = $true)]
        [string] $DownloadUrl,

        [Parameter(Mandatory = $true)]
        [long] $Length,

        [Parameter(Mandatory = $true)]
        [string] $Signature
    )

    $settings = [System.Xml.XmlWriterSettings]::new()
    $settings.Encoding = [System.Text.UTF8Encoding]::new($false)
    $settings.Indent = $true
    $settings.NewLineChars = "`n"

    $sparkleNamespace = "http://www.andymatuschak.org/xml-namespaces/sparkle"

    $writer = [System.Xml.XmlWriter]::Create($Path, $settings)
    try
    {
        $writer.WriteStartDocument()
        $writer.WriteStartElement("rss")
        $writer.WriteAttributeString("xmlns", "dc", $null, "http://purl.org/dc/elements/1.1/")
        $writer.WriteAttributeString("xmlns", "sparkle", $null, $sparkleNamespace)
        $writer.WriteAttributeString("version", "2.0")
        $writer.WriteStartElement("channel")
        $writer.WriteElementString("title", $Title)
        $writer.WriteElementString("description", $Description)
        $writer.WriteElementString("language", "en")
        $writer.WriteStartElement("item")
        $writer.WriteElementString("title", "$Title v$Version")
        $writer.WriteStartElement("description")
        $writer.WriteCData("<a href=""$ReleaseUrl"">View full patch notes on GitHub</a>")
        $writer.WriteEndElement()
        $writer.WriteElementString("pubDate", [DateTimeOffset]::UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss +0000", [Globalization.CultureInfo]::InvariantCulture))
        $writer.WriteStartElement("enclosure")
        $writer.WriteAttributeString("url", $DownloadUrl)
        $writer.WriteAttributeString("sparkle", "version", $sparkleNamespace, $Version)
        $writer.WriteAttributeString("sparkle", "shortVersionString", $sparkleNamespace, $Version)
        $writer.WriteAttributeString("sparkle", "os", $sparkleNamespace, "windows-x64")
        $writer.WriteAttributeString("length", $Length.ToString([Globalization.CultureInfo]::InvariantCulture))
        $writer.WriteAttributeString("type", "application/octet-stream")
        $writer.WriteAttributeString("sparkle", "edSignature", $sparkleNamespace, $Signature)
        $writer.WriteAttributeString("sparkle", "signature", $sparkleNamespace, $Signature)
        $writer.WriteEndElement()
        $writer.WriteEndElement()
        $writer.WriteEndElement()
        $writer.WriteEndElement()
        $writer.WriteEndDocument()
    }
    finally
    {
        $writer.Dispose()
    }
}

if (!(Test-Path -LiteralPath $ToolPath))
{
    throw "NetSparkle appcast tool not found: $ToolPath"
}

$installer = Get-Item -LiteralPath $InstallerPath
$releaseUrl = "https://github.com/$Repository/releases/tag/$TagName"
$downloadUrl = "https://github.com/$Repository/releases/download/$TagName/$($installer.Name)"
$appCastPath = Join-Path $OutputDirectory $AppCastFileName
$installerSignature = Get-NetSparkleSignature -Path $installer.FullName -SignerPath $ToolPath

Write-AppCast `
    -Path $appCastPath `
    -Title "Statistics Analysis Tool" `
    -Description "Most recent changes with link to updates." `
    -ReleaseUrl $releaseUrl `
    -Version $Version `
    -DownloadUrl $downloadUrl `
    -Length $installer.Length `
    -Signature $installerSignature

$appCastSignature = Get-NetSparkleSignature -Path $appCastPath -SignerPath $ToolPath
Set-Content -LiteralPath "$appCastPath.signature" -Value $appCastSignature -NoNewline -Encoding UTF8
