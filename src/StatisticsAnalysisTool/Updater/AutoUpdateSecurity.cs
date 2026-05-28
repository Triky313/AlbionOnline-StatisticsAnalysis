namespace StatisticsAnalysisTool.Updater;

internal static class AutoUpdateSecurity
{
    public static bool IsSignatureVerificationRequired => AutoUpdateSecurityBuildSettings.IsSignatureVerificationRequired;

    public static bool TryGetAuthenticodePublisherThumbprint(out string thumbprint)
    {
        thumbprint = NormalizeThumbprint(AutoUpdateSecurityBuildSettings.AuthenticodePublisherThumbprint);
        return !string.IsNullOrWhiteSpace(thumbprint);
    }

    public static bool TryGetEd25519PublicKey(out string publicKey)
    {
        publicKey = AutoUpdateSecurityBuildSettings.Ed25519PublicKey.Trim();
        return !string.IsNullOrWhiteSpace(publicKey);
    }

    private static string NormalizeThumbprint(string thumbprint)
    {
        return string.IsNullOrWhiteSpace(thumbprint)
            ? string.Empty
            : thumbprint.Replace(" ", string.Empty).Replace(":", string.Empty).ToUpperInvariant();
    }
}