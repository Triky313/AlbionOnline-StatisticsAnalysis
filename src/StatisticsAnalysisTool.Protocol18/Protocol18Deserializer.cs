using StatisticsAnalysisTool.Protocol18.Photon;
using System.Collections;
using System.Text;

namespace StatisticsAnalysisTool.Protocol18;

public static class Protocol18Deserializer
{
    private const byte MaxSlimCustomTypeCode = 228;

    private static readonly ThreadLocal<byte[]> ScalarBuffer = new(() => new byte[sizeof(long)]);

    private static readonly byte[] BoolMasks =
    [
        1,
        2,
        4,
        8,
        16,
        32,
        64,
        128
    ];

    public static object? Deserialize(Protocol18Stream input)
    {
        return Deserialize(input, ReadByte(input));
    }

    public static object? Deserialize(Protocol18Stream input, byte typeCode)
    {
        if (typeCode >= (byte) Protocol18Type.CustomTypeSlim && typeCode <= MaxSlimCustomTypeCode)
        {
            return DeserializeCustomType(input, typeCode);
        }

        return (Protocol18Type) typeCode switch
        {
            Protocol18Type.Boolean => DeserializeBoolean(input),
            Protocol18Type.Byte => ReadByte(input),
            Protocol18Type.Short => DeserializeShort(input),
            Protocol18Type.Float => DeserializeFloat(input),
            Protocol18Type.Double => DeserializeDouble(input),
            Protocol18Type.String => DeserializeString(input),
            Protocol18Type.Null => null,
            Protocol18Type.CompressedInt => ReadCompressedInt32(input),
            Protocol18Type.CompressedLong => ReadCompressedInt64(input),
            Protocol18Type.Int1 => ReadInt1(input, signNegative: false),
            Protocol18Type.Int1Negative => ReadInt1(input, signNegative: true),
            Protocol18Type.Int2 => ReadInt2(input, signNegative: false),
            Protocol18Type.Int2Negative => ReadInt2(input, signNegative: true),
            Protocol18Type.Long1 => ReadLong1(input, signNegative: false),
            Protocol18Type.Long1Negative => ReadLong1(input, signNegative: true),
            Protocol18Type.Long2 => ReadLong2(input, signNegative: false),
            Protocol18Type.Long2Negative => ReadLong2(input, signNegative: true),
            Protocol18Type.Custom => DeserializeCustomType(input),
            Protocol18Type.Dictionary => DeserializeDictionary(input),
            Protocol18Type.Hashtable => DeserializeHashtable(input),
            Protocol18Type.ObjectArray => DeserializeObjectArray(input),
            Protocol18Type.OperationRequest => DeserializeOperationRequest(input),
            Protocol18Type.OperationResponse => DeserializeOperationResponse(input),
            Protocol18Type.EventData => DeserializeEventData(input),
            Protocol18Type.BooleanFalse => false,
            Protocol18Type.BooleanTrue => true,
            Protocol18Type.ShortZero => (short) 0,
            Protocol18Type.IntZero => 0,
            Protocol18Type.LongZero => 0L,
            Protocol18Type.FloatZero => 0f,
            Protocol18Type.DoubleZero => 0d,
            Protocol18Type.ByteZero => (byte) 0,
            Protocol18Type.Array => DeserializeArrayInArray(input),
            Protocol18Type.BooleanArray => DeserializeBooleanArray(input),
            Protocol18Type.ByteArray => DeserializeByteArray(input),
            Protocol18Type.ShortArray => DeserializeShortArray(input),
            Protocol18Type.FloatArray => DeserializeFloatArray(input),
            Protocol18Type.DoubleArray => DeserializeDoubleArray(input),
            Protocol18Type.StringArray => DeserializeStringArray(input),
            Protocol18Type.CompressedIntArray => DeserializeCompressedIntArray(input),
            Protocol18Type.CompressedLongArray => DeserializeCompressedLongArray(input),
            Protocol18Type.CustomTypeArray => DeserializeCustomTypeArray(input),
            Protocol18Type.DictionaryArray => DeserializeDictionaryArray(input),
            Protocol18Type.HashtableArray => DeserializeHashtableArray(input),
            _ => throw new ArgumentException($"Protocol18 type code {typeCode} is not supported."),
        };
    }

    public static OperationRequest DeserializeOperationRequest(Protocol18Stream input)
    {
        byte operationCode = ReadByte(input);
        Dictionary<byte, object> parameters = DeserializeParameterTable(input);

        return new OperationRequest(operationCode, parameters);
    }

    public static OperationResponse DeserializeOperationResponse(Protocol18Stream input)
    {
        byte operationCode = ReadByte(input);
        short returnCode = DeserializeShort(input);
        string debugMessage = Deserialize(input, ReadByte(input)) as string ?? string.Empty;
        Dictionary<byte, object> parameters = DeserializeParameterTable(input);

        return new OperationResponse(operationCode, returnCode, debugMessage, parameters);
    }

    public static EventData DeserializeEventData(Protocol18Stream input)
    {
        byte code = ReadByte(input);
        Dictionary<byte, object> parameters = DeserializeParameterTable(input);

        return new EventData(code, parameters);
    }

    public static short DeserializeShort(Protocol18Stream input)
    {
        byte[] buffer = GetScalarBuffer();
        ReadExactly(input, buffer, sizeof(short));

        return (short) (buffer[0] | (buffer[1] << 8));
    }

    private static bool DeserializeBoolean(Stream input)
    {
        return ReadByte(input) != 0;
    }

    private static float DeserializeFloat(Stream input)
    {
        byte[] buffer = GetScalarBuffer();
        ReadExactly(input, buffer, sizeof(float));

        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(buffer, 0, sizeof(float));
        }

        return BitConverter.ToSingle(buffer, 0);
    }

    private static double DeserializeDouble(Stream input)
    {
        byte[] buffer = GetScalarBuffer();
        ReadExactly(input, buffer, sizeof(double));

        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(buffer, 0, sizeof(double));
        }

        return BitConverter.ToDouble(buffer, 0);
    }

    private static string DeserializeString(Protocol18Stream input)
    {
        int stringLength = checked((int) ReadCompressedUInt32(input));
        if (stringLength == 0)
        {
            return string.Empty;
        }

        byte[] buffer = new byte[stringLength];
        ReadExactly(input, buffer, stringLength);

        return Encoding.UTF8.GetString(buffer, 0, stringLength);
    }

    private static byte[] DeserializeByteArray(Protocol18Stream input)
    {
        int arrayLength = checked((int) ReadCompressedUInt32(input));
        if (arrayLength == 0)
        {
            return [];
        }

        byte[] buffer = new byte[arrayLength];
        ReadExactly(input, buffer, arrayLength);

        return buffer;
    }

    private static short[] DeserializeShortArray(Protocol18Stream input)
    {
        int arrayLength = checked((int) ReadCompressedUInt32(input));
        var array = new short[arrayLength];
        for (int i = 0; i < arrayLength; i++)
        {
            array[i] = DeserializeShort(input);
        }

        return array;
    }

    private static float[] DeserializeFloatArray(Protocol18Stream input)
    {
        int arrayLength = checked((int) ReadCompressedUInt32(input));
        int byteLength = checked(arrayLength * sizeof(float));
        var array = new float[arrayLength];
        if (byteLength == 0)
        {
            return array;
        }

        byte[] buffer = new byte[byteLength];
        ReadExactly(input, buffer, byteLength);

        if (!BitConverter.IsLittleEndian)
        {
            for (int i = 0; i < byteLength; i += sizeof(float))
            {
                Array.Reverse(buffer, i, sizeof(float));
            }
        }

        Buffer.BlockCopy(buffer, 0, array, 0, byteLength);
        return array;
    }

    private static double[] DeserializeDoubleArray(Protocol18Stream input)
    {
        int arrayLength = checked((int) ReadCompressedUInt32(input));
        int byteLength = checked(arrayLength * sizeof(double));
        var array = new double[arrayLength];
        if (byteLength == 0)
        {
            return array;
        }

        byte[] buffer = new byte[byteLength];
        ReadExactly(input, buffer, byteLength);

        if (!BitConverter.IsLittleEndian)
        {
            for (int i = 0; i < byteLength; i += sizeof(double))
            {
                Array.Reverse(buffer, i, sizeof(double));
            }
        }

        Buffer.BlockCopy(buffer, 0, array, 0, byteLength);
        return array;
    }

    private static bool[] DeserializeBooleanArray(Protocol18Stream input)
    {
        int arrayLength = checked((int) ReadCompressedUInt32(input));
        var array = new bool[arrayLength];
        int fullByteCount = arrayLength / 8;
        int index = 0;

        for (int i = 0; i < fullByteCount; i++)
        {
            byte value = ReadByte(input);
            array[index++] = (value & 1) == 1;
            array[index++] = (value & 2) == 2;
            array[index++] = (value & 4) == 4;
            array[index++] = (value & 8) == 8;
            array[index++] = (value & 16) == 16;
            array[index++] = (value & 32) == 32;
            array[index++] = (value & 64) == 64;
            array[index++] = (value & 128) == 128;
        }

        if (index < arrayLength)
        {
            byte value = ReadByte(input);
            int bitIndex = 0;
            while (index < arrayLength)
            {
                array[index++] = (value & BoolMasks[bitIndex]) == BoolMasks[bitIndex];
                bitIndex++;
            }
        }

        return array;
    }

    private static string[] DeserializeStringArray(Protocol18Stream input)
    {
        int arrayLength = checked((int) ReadCompressedUInt32(input));
        var array = new string[arrayLength];
        for (int i = 0; i < arrayLength; i++)
        {
            array[i] = DeserializeString(input);
        }

        return array;
    }

    private static int[] DeserializeCompressedIntArray(Protocol18Stream input)
    {
        int arrayLength = checked((int) ReadCompressedUInt32(input));
        var array = new int[arrayLength];
        for (int i = 0; i < arrayLength; i++)
        {
            array[i] = ReadCompressedInt32(input);
        }

        return array;
    }

    private static long[] DeserializeCompressedLongArray(Protocol18Stream input)
    {
        int arrayLength = checked((int) ReadCompressedUInt32(input));
        var array = new long[arrayLength];
        for (int i = 0; i < arrayLength; i++)
        {
            array[i] = ReadCompressedInt64(input);
        }

        return array;
    }

    private static object[] DeserializeObjectArray(Protocol18Stream input)
    {
        int arrayLength = checked((int) ReadCompressedUInt32(input));
        var array = new object[arrayLength];
        for (int i = 0; i < arrayLength; i++)
        {
            array[i] = Deserialize(input)!;
        }

        return array;
    }

    private static Hashtable[] DeserializeHashtableArray(Protocol18Stream input)
    {
        int arrayLength = checked((int) ReadCompressedUInt32(input));
        var array = new Hashtable[arrayLength];
        for (int i = 0; i < arrayLength; i++)
        {
            array[i] = DeserializeHashtable(input);
        }

        return array;
    }

    private static IDictionary[] DeserializeDictionaryArray(Protocol18Stream input)
    {
        Type dictionaryType = DeserializeDictionaryType(input, out Protocol18Type keyTypeCode, out Protocol18Type valueTypeCode);
        int arrayLength = checked((int) ReadCompressedUInt32(input));
        var array = (IDictionary[]) Array.CreateInstance(dictionaryType, arrayLength);

        for (int i = 0; i < arrayLength; i++)
        {
            if (Activator.CreateInstance(dictionaryType) is not IDictionary dictionary)
            {
                throw new InvalidOperationException($"Could not create dictionary type '{dictionaryType}'.");
            }

            DeserializeDictionaryElements(input, dictionary, keyTypeCode, valueTypeCode);
            array[i] = dictionary;
        }

        return array;
    }

    private static Array? DeserializeArrayInArray(Protocol18Stream input)
    {
        int arrayLength = checked((int) ReadCompressedUInt32(input));
        Array? result = null;
        Type? resultType = null;

        for (int i = 0; i < arrayLength; i++)
        {
            object? value = Deserialize(input);
            if (value is not Array nestedArray)
            {
                continue;
            }

            if (result == null)
            {
                resultType = nestedArray.GetType();
                result = Array.CreateInstance(resultType, arrayLength);
            }

            if (resultType != null && resultType.IsAssignableFrom(nestedArray.GetType()))
            {
                result.SetValue(nestedArray, i);
            }
        }

        return result;
    }

    private static Hashtable DeserializeHashtable(Protocol18Stream input)
    {
        int size = checked((int) ReadCompressedUInt32(input));
        var output = new Hashtable(size);

        for (int i = 0; i < size; i++)
        {
            object? key = Deserialize(input);
            object? value = Deserialize(input);
            if (key != null)
            {
                output[key] = value;
            }
        }

        return output;
    }

    private static IDictionary DeserializeDictionary(Protocol18Stream input)
    {
        Type dictionaryType = DeserializeDictionaryType(input, out Protocol18Type keyTypeCode, out Protocol18Type valueTypeCode);
        if (Activator.CreateInstance(dictionaryType) is not IDictionary dictionary)
        {
            throw new InvalidOperationException($"Could not create dictionary type '{dictionaryType}'.");
        }

        DeserializeDictionaryElements(input, dictionary, keyTypeCode, valueTypeCode);
        return dictionary;
    }

    private static void DeserializeDictionaryElements(Protocol18Stream input, IDictionary dictionary, Protocol18Type keyTypeCode, Protocol18Type valueTypeCode)
    {
        int size = checked((int) ReadCompressedUInt32(input));
        for (int i = 0; i < size; i++)
        {
            object? key = keyTypeCode == Protocol18Type.Unknown
                ? Deserialize(input)
                : Deserialize(input, (byte) keyTypeCode);
            object? value = valueTypeCode == Protocol18Type.Unknown
                ? Deserialize(input)
                : Deserialize(input, (byte) valueTypeCode);

            if (key != null)
            {
                dictionary.Add(key, value);
            }
        }
    }

    private static Type DeserializeDictionaryType(Protocol18Stream input, out Protocol18Type keyTypeCode, out Protocol18Type valueTypeCode)
    {
        keyTypeCode = (Protocol18Type) ReadByte(input);
        valueTypeCode = (Protocol18Type) ReadByte(input);

        Type keyType = keyTypeCode == Protocol18Type.Unknown
            ? typeof(object)
            : GetAllowedDictionaryKeyType(keyTypeCode);

        Type valueType = valueTypeCode switch
        {
            Protocol18Type.Unknown => typeof(object),
            Protocol18Type.Dictionary => DeserializeDictionaryType(input),
            Protocol18Type.Array => GetDictionaryArrayType(input),
            Protocol18Type.ObjectArray => typeof(object[]),
            Protocol18Type.HashtableArray => typeof(Hashtable[]),
            _ => GetClrArrayType(valueTypeCode),
        };

        if (valueTypeCode == Protocol18Type.Array)
        {
            valueTypeCode = Protocol18Type.Unknown;
        }

        return typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
    }

    private static Type DeserializeDictionaryType(Protocol18Stream input)
    {
        Protocol18Type keyTypeCode = (Protocol18Type) ReadByte(input);
        Protocol18Type valueTypeCode = (Protocol18Type) ReadByte(input);

        Type keyType = keyTypeCode == Protocol18Type.Unknown
            ? typeof(object)
            : GetAllowedDictionaryKeyType(keyTypeCode);

        Type valueType = valueTypeCode switch
        {
            Protocol18Type.Unknown => typeof(object),
            Protocol18Type.Dictionary => DeserializeDictionaryType(input),
            Protocol18Type.Array => GetDictionaryArrayType(input),
            _ => GetClrArrayType(valueTypeCode),
        };

        return typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
    }

    private static Type GetDictionaryArrayType(Protocol18Stream input)
    {
        Protocol18Type typeCode = (Protocol18Type) ReadByte(input);
        int nestedArrayDepth = 0;

        while (typeCode == Protocol18Type.Array)
        {
            nestedArrayDepth++;
            typeCode = (Protocol18Type) ReadByte(input);
        }

        Type arrayType = GetClrArrayType(typeCode).MakeArrayType();
        for (int i = 0; i < nestedArrayDepth; i++)
        {
            arrayType = arrayType.MakeArrayType();
        }

        return arrayType;
    }

    private static Type GetAllowedDictionaryKeyType(Protocol18Type typeCode)
    {
        return typeCode switch
        {
            Protocol18Type.Byte or Protocol18Type.ByteZero => typeof(byte),
            Protocol18Type.Short or Protocol18Type.ShortZero => typeof(short),
            Protocol18Type.Float or Protocol18Type.FloatZero => typeof(float),
            Protocol18Type.Double or Protocol18Type.DoubleZero => typeof(double),
            Protocol18Type.String => typeof(string),
            Protocol18Type.CompressedInt or Protocol18Type.Int1 or Protocol18Type.Int1Negative or Protocol18Type.Int2 or Protocol18Type.Int2Negative or Protocol18Type.IntZero => typeof(int),
            Protocol18Type.CompressedLong or Protocol18Type.Long1 or Protocol18Type.Long1Negative or Protocol18Type.Long2 or Protocol18Type.Long2Negative or Protocol18Type.LongZero => typeof(long),
            _ => throw new InvalidDataException($"Protocol18 type '{typeCode}' is not valid as a dictionary key."),
        };
    }

    private static Type GetClrArrayType(Protocol18Type typeCode)
    {
        return typeCode switch
        {
            Protocol18Type.Boolean or Protocol18Type.BooleanFalse or Protocol18Type.BooleanTrue => typeof(bool),
            Protocol18Type.Byte or Protocol18Type.ByteZero => typeof(byte),
            Protocol18Type.Short or Protocol18Type.ShortZero => typeof(short),
            Protocol18Type.Float or Protocol18Type.FloatZero => typeof(float),
            Protocol18Type.Double or Protocol18Type.DoubleZero => typeof(double),
            Protocol18Type.String => typeof(string),
            Protocol18Type.CompressedInt or Protocol18Type.Int1 or Protocol18Type.Int1Negative or Protocol18Type.Int2 or Protocol18Type.Int2Negative or Protocol18Type.IntZero => typeof(int),
            Protocol18Type.CompressedLong or Protocol18Type.Long1 or Protocol18Type.Long1Negative or Protocol18Type.Long2 or Protocol18Type.Long2Negative or Protocol18Type.LongZero => typeof(long),
            Protocol18Type.Hashtable => typeof(Hashtable),
            Protocol18Type.OperationRequest => typeof(OperationRequest),
            Protocol18Type.OperationResponse => typeof(OperationResponse),
            Protocol18Type.EventData => typeof(EventData),
            Protocol18Type.BooleanArray => typeof(bool[]),
            Protocol18Type.ByteArray => typeof(byte[]),
            Protocol18Type.ShortArray => typeof(short[]),
            Protocol18Type.FloatArray => typeof(float[]),
            Protocol18Type.DoubleArray => typeof(double[]),
            Protocol18Type.StringArray => typeof(string[]),
            Protocol18Type.ObjectArray => typeof(object[]),
            Protocol18Type.HashtableArray => typeof(Hashtable[]),
            Protocol18Type.CompressedIntArray => typeof(int[]),
            Protocol18Type.CompressedLongArray => typeof(long[]),
            _ => throw new InvalidDataException($"Protocol18 type '{typeCode}' cannot be mapped to a CLR array type."),
        };
    }

    private static Dictionary<byte, object> DeserializeParameterTable(Protocol18Stream input)
    {
        int size = ReadByte(input);
        var parameters = new Dictionary<byte, object>(size);
        for (int i = 0; i < size; i++)
        {
            byte key = ReadByte(input);
            byte valueTypeCode = ReadByte(input);
            parameters[key] = Deserialize(input, valueTypeCode)!;
        }

        return parameters;
    }

    private static Protocol18CustomType DeserializeCustomType(Protocol18Stream input, byte slimTypeCode = 0)
    {
        byte typeCode = slimTypeCode == 0
            ? ReadByte(input)
            : (byte) (slimTypeCode - (byte) Protocol18Type.CustomTypeSlim);
        int length = checked((int) ReadCompressedUInt32(input));
        byte[] data = ReadBytes(input, length);

        return new Protocol18CustomType(typeCode, data);
    }

    private static Protocol18CustomType[] DeserializeCustomTypeArray(Protocol18Stream input)
    {
        int arrayLength = checked((int) ReadCompressedUInt32(input));
        byte typeCode = ReadByte(input);
        var array = new Protocol18CustomType[arrayLength];

        for (int i = 0; i < arrayLength; i++)
        {
            int length = checked((int) ReadCompressedUInt32(input));
            byte[] data = ReadBytes(input, length);
            array[i] = new Protocol18CustomType(typeCode, data);
        }

        return array;
    }

    private static int ReadInt1(Stream input, bool signNegative)
    {
        int value = ReadByte(input);
        return signNegative ? -value : value;
    }

    private static int ReadInt2(Protocol18Stream input, bool signNegative)
    {
        int value = ReadUShort(input);
        return signNegative ? -value : value;
    }

    private static long ReadLong1(Stream input, bool signNegative)
    {
        long value = ReadByte(input);
        return signNegative ? -value : value;
    }

    private static long ReadLong2(Protocol18Stream input, bool signNegative)
    {
        long value = ReadUShort(input);
        return signNegative ? -value : value;
    }

    private static int ReadCompressedInt32(Protocol18Stream input)
    {
        return DecodeZigZag32(ReadCompressedUInt32(input));
    }

    private static long ReadCompressedInt64(Protocol18Stream input)
    {
        return DecodeZigZag64(ReadCompressedUInt64(input));
    }

    private static uint ReadCompressedUInt32(Protocol18Stream input)
    {
        uint value = 0;
        int shift = 0;

        while (shift != 35)
        {
            byte current = ReadByte(input);
            value |= (uint) (current & 0x7F) << shift;
            shift += 7;

            if ((current & 0x80) == 0)
            {
                return value;
            }
        }

        return value;
    }

    private static ulong ReadCompressedUInt64(Protocol18Stream input)
    {
        ulong value = 0;
        int shift = 0;

        while (shift != 70)
        {
            byte current = ReadByte(input);
            value |= (ulong) (current & 0x7F) << shift;
            shift += 7;

            if ((current & 0x80) == 0)
            {
                return value;
            }
        }

        return value;
    }

    private static ushort ReadUShort(Protocol18Stream input)
    {
        byte[] buffer = GetScalarBuffer();
        ReadExactly(input, buffer, sizeof(ushort));

        return (ushort) (buffer[0] | (buffer[1] << 8));
    }

    private static byte ReadByte(Stream input)
    {
        int value = input.ReadByte();
        if (value < 0)
        {
            throw new EndOfStreamException("Failed to read a byte from the Protocol18 payload.");
        }

        return (byte) value;
    }

    private static byte[] ReadBytes(Stream input, int length)
    {
        if (length == 0)
        {
            return [];
        }

        byte[] buffer = new byte[length];
        ReadExactly(input, buffer, length);
        return buffer;
    }

    private static void ReadExactly(Stream input, byte[] buffer, int count)
    {
        int offset = 0;
        while (offset < count)
        {
            int read = input.Read(buffer, offset, count - offset);
            if (read <= 0)
            {
                throw new EndOfStreamException($"Failed to read {count} bytes from the Protocol18 payload.");
            }

            offset += read;
        }
    }

    private static byte[] GetScalarBuffer()
    {
        return ScalarBuffer.Value ?? throw new InvalidOperationException("The scalar Protocol18 buffer could not be created.");
    }

    private static int DecodeZigZag32(uint value)
    {
        return (int) ((value >> 1) ^ (0u - (value & 1u)));
    }

    private static long DecodeZigZag64(ulong value)
    {
        return (long) ((value >> 1) ^ (0UL - (value & 1UL)));
    }
}

internal sealed class Protocol18CustomType(byte typeCode, byte[] data)
{
    public byte TypeCode { get; } = typeCode;

    public byte[] Data { get; } = data;

    public override string ToString()
    {
        return $"Protocol18CustomType({TypeCode}, {Data.Length} bytes)";
    }
}