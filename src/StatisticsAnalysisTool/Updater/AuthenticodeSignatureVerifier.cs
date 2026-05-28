using Serilog;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace StatisticsAnalysisTool.Updater;

internal static class AuthenticodeSignatureVerifier
{
    private static readonly Guid WinTrustActionGenericVerifyV2 = new("00AAC56B-CD44-11D0-8CC2-00C04FC295EE");

    public static AuthenticodeVerificationResult VerifyFile(string filePath, string expectedPublisherThumbprint)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return AuthenticodeVerificationResult.Invalid;
        }

        if (string.IsNullOrWhiteSpace(expectedPublisherThumbprint))
        {
            return AuthenticodeVerificationResult.NotConfigured;
        }

        if (!OperatingSystem.IsWindows())
        {
            return AuthenticodeVerificationResult.Invalid;
        }

        var trustResult = VerifyEmbeddedSignature(filePath);
        if (trustResult != 0)
        {
            Log.Warning("Authenticode verification failed with WinVerifyTrust result 0x{Result:X8}.", trustResult);
            return AuthenticodeVerificationResult.Invalid;
        }

        try
        {
#pragma warning disable SYSLIB0057
            using var signerCertificate = new X509Certificate2(X509Certificate.CreateFromSignedFile(filePath));
#pragma warning restore SYSLIB0057
            var actualThumbprint = NormalizeThumbprint(signerCertificate.Thumbprint);
            var expectedThumbprint = NormalizeThumbprint(expectedPublisherThumbprint);

            if (string.Equals(actualThumbprint, expectedThumbprint, StringComparison.OrdinalIgnoreCase))
            {
                return AuthenticodeVerificationResult.Valid;
            }

            Log.Warning("Authenticode publisher thumbprint mismatch. Expected {ExpectedThumbprint}, got {ActualThumbprint}.", expectedThumbprint, actualThumbprint);
            return AuthenticodeVerificationResult.Invalid;
        }
        catch (CryptographicException e)
        {
            Log.Warning(e, "Failed to read Authenticode signer certificate.");
            return AuthenticodeVerificationResult.Invalid;
        }
    }

    private static uint VerifyEmbeddedSignature(string filePath)
    {
        var fileInfo = new WinTrustFileInfo(filePath);
        var fileInfoPointer = IntPtr.Zero;
        var trustDataPointer = IntPtr.Zero;

        try
        {
            fileInfoPointer = Marshal.AllocCoTaskMem(Marshal.SizeOf<WinTrustFileInfo>());
            Marshal.StructureToPtr(fileInfo, fileInfoPointer, false);

            var trustData = new WinTrustData(fileInfoPointer);
            trustDataPointer = Marshal.AllocCoTaskMem(Marshal.SizeOf<WinTrustData>());
            Marshal.StructureToPtr(trustData, trustDataPointer, false);

            var action = WinTrustActionGenericVerifyV2;
            return WinVerifyTrust(IntPtr.Zero, ref action, trustDataPointer);
        }
        finally
        {
            if (trustDataPointer != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(trustDataPointer);
            }

            if (fileInfoPointer != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(fileInfoPointer);
            }
        }
    }

    private static string NormalizeThumbprint(string thumbprint)
    {
        return string.IsNullOrWhiteSpace(thumbprint)
            ? string.Empty
            : thumbprint.Replace(" ", string.Empty).Replace(":", string.Empty).ToUpperInvariant();
    }

    [DllImport("wintrust.dll", ExactSpelling = true, SetLastError = true)]
    private static extern uint WinVerifyTrust(IntPtr windowHandle, [MarshalAs(UnmanagedType.LPStruct)] ref Guid actionId, IntPtr trustData);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private sealed class WinTrustFileInfo
    {
        private readonly uint _cbStruct = (uint) Marshal.SizeOf<WinTrustFileInfo>();
        private readonly string _pcwszFilePath;
        private readonly IntPtr _hFile = IntPtr.Zero;
        private readonly IntPtr _pgKnownSubject = IntPtr.Zero;

        public WinTrustFileInfo(string filePath)
        {
            _pcwszFilePath = filePath;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private sealed class WinTrustData
    {
        private readonly uint _cbStruct = (uint) Marshal.SizeOf<WinTrustData>();
        private readonly IntPtr _pPolicyCallbackData = IntPtr.Zero;
        private readonly IntPtr _pSIPClientData = IntPtr.Zero;
        private readonly uint _dwUIChoice = 2;
        private readonly uint _fdwRevocationChecks = 0;
        private readonly uint _dwUnionChoice = 1;
        private readonly IntPtr _pFile;
        private readonly uint _dwStateAction = 0;
        private readonly IntPtr _hWVTStateData = IntPtr.Zero;
        private readonly IntPtr _pwszURLReference = IntPtr.Zero;
        private readonly uint _dwProvFlags = 0;
        private readonly uint _dwUIContext = 0;
        private readonly IntPtr _pSignatureSettings = IntPtr.Zero;

        public WinTrustData(IntPtr fileInfoPointer)
        {
            _pFile = fileInfoPointer;
        }
    }
}

internal enum AuthenticodeVerificationResult
{
    NotConfigured,
    Valid,
    Invalid
}
