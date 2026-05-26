param(
    [Parameter(Mandatory = $true)]
    [string] $SourceFile,

    [Parameter(Mandatory = $true)]
    [string] $PublicKey
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$placeholder = "__SPARKLE_ED25519_PUBLIC_KEY__"
$normalizedPublicKey = $PublicKey.Trim()

if ([string]::IsNullOrWhiteSpace($normalizedPublicKey))
{
    throw "SPARKLE_PUBLIC_KEY is required."
}

try
{
    [Convert]::FromBase64String($normalizedPublicKey) | Out-Null
}
catch
{
    throw "SPARKLE_PUBLIC_KEY must be a base64 encoded Ed25519 public key."
}

if (!(Test-Path -LiteralPath $SourceFile))
{
    throw "Source file not found: $SourceFile"
}

$content = Get-Content -LiteralPath $SourceFile -Raw
if (!$content.Contains($placeholder))
{
    throw "Public key placeholder was not found in $SourceFile."
}

$content = $content.Replace($placeholder, $normalizedPublicKey)
Set-Content -LiteralPath $SourceFile -Value $content -NoNewline -Encoding UTF8
