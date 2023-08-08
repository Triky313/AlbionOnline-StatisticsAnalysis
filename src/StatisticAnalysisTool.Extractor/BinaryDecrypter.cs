using System.IO.Compression;
using System.Security.Cryptography;

namespace StatisticAnalysisTool.Extractor;

public static class BinaryDecrypter
{
    private static readonly byte[] Key = { 48, 239, 114, 71, 66, 242, 4, 50 };
    private static readonly byte[] Iv = { 14, 166, 220, 137, 219, 237, 220, 79 };

    public static void DecryptBinaryFile(string inputPath, Stream outputStream)
    {
        using var inputFile = File.OpenRead(inputPath);
        var fileBuffer = new byte[inputFile.Length];
        int _ = inputFile.Read(fileBuffer, 0, fileBuffer.Length);

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

        using GZipStream decompression = new GZipStream(new MemoryStream(outBuffer), CompressionMode.Decompress);
        while ((bytesRead = decompression.Read(buffer, 0, buffer.Length)) > 0)
        {
            outputStream.Write(buffer, 0, bytesRead);
        }
    }

    public static byte[] DecryptAndDecompress(string inputPath)
    {
        using var inputFile = File.OpenRead(inputPath);
        var fileBuffer = new byte[inputFile.Length];
        int bytesRead = inputFile.Read(fileBuffer, 0, fileBuffer.Length);

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

        using GZipStream decompression = new GZipStream(new MemoryStream(outBuffer), CompressionMode.Decompress);
        using MemoryStream outputMemoryStream = new MemoryStream();

        while ((decompressedBytesRead = decompression.Read(buffer, 0, buffer.Length)) > 0)
        {
            outputMemoryStream.Write(buffer, 0, decompressedBytesRead);
        }

        return outputMemoryStream.ToArray();
    }
}