namespace StatisticsAnalysisTool.Updater;

internal static class AutoUpdateSecurity
{
    public static bool IsSignatureVerificationRequired => AutoUpdateSecurityBuildSettings.IsSignatureVerificationRequired;

    public static bool TryGetEd25519PublicKey(out string publicKey)
    {
        publicKey = AutoUpdateSecurityBuildSettings.Ed25519PublicKey.Trim();
        return !string.IsNullOrWhiteSpace(publicKey);
    }
}