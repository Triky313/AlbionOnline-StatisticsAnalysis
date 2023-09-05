using System.IO.Compression;
using System.Security.Cryptography;

namespace StatisticAnalysisTool.Extractor;

internal static class BinaryDecrypter
{
    private static readonly byte[] Key = { 48, 239, 114, 71, 66, 242, 4, 50 };
    private static readonly byte[] Iv = { 14, 166, 220, 137, 219, 237, 220, 79 };

    public static async Task DecryptBinaryFileAsync(string inputPath, Stream outputStream)
    {
        await using var inputFile = File.OpenRead(inputPath);
        var fileBuffer = new byte[inputFile.Length];
        _ = await inputFile.ReadAsync(fileBuffer, 0, fileBuffer.Length);

        var tDes = new DESCryptoServiceProvider
        {
            IV = Iv,
            Mode = CipherMode.CBC,
            Key = Key
        };
        var outBuffer = tDes.CreateDecryptor().TransformFinalBlock(fileBuffer, 0, fileBuffer.Length);

        const int size = 4096;
        var buffer = new byte[size];
        int bytesRead;

        await using GZipStream decompression = new GZipStream(new MemoryStream(outBuffer), CompressionMode.Decompress);
        while ((bytesRead = decompression.Read(buffer, 0, buffer.Length)) > 0)
        {
            await outputStream.WriteAsync(buffer, 0, bytesRead);
        }
    }

    public static async Task<MemoryStream> DecryptAndDecompressAsync(string inputPath)
    {
        await using var inputFile = File.OpenRead(inputPath);
        var fileBuffer = new byte[inputFile.Length];
        int bytesRead = await inputFile.ReadAsync(fileBuffer, 0, fileBuffer.Length);

        var tDes = new DESCryptoServiceProvider
        {
            IV = Iv,
            Mode = CipherMode.CBC,
            Key = Key
        };
        var outBuffer = tDes.CreateDecryptor().TransformFinalBlock(fileBuffer, 0, bytesRead);

        const int size = 4096;
        var buffer = new byte[size];
        int decompressedBytesRead;

        await using GZipStream decompression = new GZipStream(new MemoryStream(outBuffer), CompressionMode.Decompress);
        await using MemoryStream outputMemoryStream = new MemoryStream();

        while ((decompressedBytesRead = await decompression.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            await outputMemoryStream.WriteAsync(buffer, 0, decompressedBytesRead);
        }

        return outputMemoryStream;
    }
}