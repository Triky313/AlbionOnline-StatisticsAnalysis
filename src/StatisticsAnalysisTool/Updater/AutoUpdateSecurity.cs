using System;

namespace StatisticsAnalysisTool.Updater;

internal static class AutoUpdateSecurity
{
    private const string PublicKeyPlaceholder = "__SPARKLE_ED25519_PUBLIC_KEY__";

    public const string Ed25519PublicKey = PublicKeyPlaceholder;

    public static bool IsEd25519PublicKeyConfigured()
    {
        return !string.IsNullOrWhiteSpace(Ed25519PublicKey)
               && !string.Equals(Ed25519PublicKey, PublicKeyPlaceholder, StringComparison.Ordinal);
    }
}