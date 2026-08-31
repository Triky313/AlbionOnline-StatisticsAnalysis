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

    private delegate TResult ReaderDeserializer<TResult>(ref Protocol18Reader input);
    private delegate TResult StatefulReaderDeserializer<TState, TResult>(ref Protocol18Reader input, TState state);

    public static object? Deserialize(Protocol18Stream input)
    {
        return DeserializeFromStream(input, Deserialize);
    }

    public static object? Deserialize(ReadOnlySpan<byte> input)
    {
        Protocol18Reader reader = new(input);
        return Deserialize(ref reader);
    }

    public static object? Deserialize(Protocol18Stream input, byte typeCode)
    {
        return DeserializeFromStream(input, typeCode, Deserialize);
    }

    public static object? Deserialize(ReadOnlySpan<byte> input, byte typeCode)
    {
        Protocol18Reader reader = new(input);
        return Deserialize(ref reader, typeCode);
    }

    public static OperationRequest DeserializeOperationRequest(Protocol18Stream input)
    {
        return DeserializeFromStream(input, DeserializeOperationRequest);
    }

    public static OperationRequest DeserializeOperationRequest(ReadOnlySpan<byte> input)
    {
        Protocol18Reader reader = new(input);
        return DeserializeOperationRequest(ref reader);
    }

    public static OperationResponse DeserializeOperationResponse(Protocol18Stream input)
    {
        return DeserializeFromStream(input, DeserializeOperationResponse);
    }

    public static OperationResponse DeserializeOperationResponse(ReadOnlySpan<byte> input)
    {
        Protocol18Reader reader = new(input);
        return DeserializeOperationResponse(ref reader);
    }

    public static EventData DeserializeEventData(Protocol18Stream input)
    {
        return DeserializeFromStream(input, DeserializeEventData);
    }

    public static EventData DeserializeEventData(ReadOnlySpan<byte> input)
    {
        Protocol18Reader reader = new(input);
        return DeserializeEventData(ref reader);
    }

    public static short DeserializeShort(Protocol18Stream input)
    {
        return DeserializeFromStream(input, DeserializeShort);
    }

    public static short DeserializeShort(ReadOnlySpan<byte> input)
    {
        Protocol18Reader reader = new(input);
        return DeserializeShort(ref reader);
    }

    private static TResult DeserializeFromStream<TResult>(
        Protocol18Stream input,
        ReaderDeserializer<TResult> deserialize)
    {
        Protocol18Reader reader = new(input.RemainingSpan);

        try
        {
            return deserialize(ref reader);
        }
        finally
        {
            input.Advance(reader.Position);
        }
    }

    private static TResult DeserializeFromStream<TState, TResult>(
        Protocol18Stream input,
        TState state,
        StatefulReaderDeserializer<TState, TResult> deserialize)
    {
        Protocol18Reader reader = new(input.RemainingSpan);

        try
        {
            return deserialize(ref reader, state);
        }
        finally
        {
            input.Advance(reader.Position);
        }
    }

    private static object? Deserialize(ref Protocol18Reader input)
    {
        return Deserialize(ref input, ReadByte(ref input));
    }

    private static object? Deserialize(ref Protocol18Reader input, byte typeCode)
    {
        if (typeCode >= (byte) Protocol18Type.CustomTypeSlim && typeCode <= MaxSlimCustomTypeCode)
        {
            return DeserializeCustomType(ref input, typeCode);
        }

        return (Protocol18Type) typeCode switch
        {
            Protocol18Type.Boolean => DeserializeBoolean(ref input),
            Protocol18Type.Byte => ReadByte(ref input),
            Protocol18Type.Short => DeserializeShort(ref input),
            Protocol18Type.Float => DeserializeFloat(ref input),
            Protocol18Type.Double => DeserializeDouble(ref input),
            Protocol18Type.String => DeserializeString(ref input),
            Protocol18Type.Null => null,
            Protocol18Type.CompressedInt => ReadCompressedInt32(ref input),
            Protocol18Type.CompressedLong => ReadCompressedInt64(ref input),
            Protocol18Type.Int1 => ReadInt1(ref input, signNegative: false),
            Protocol18Type.Int1Negative => ReadInt1(ref input, signNegative: true),
            Protocol18Type.Int2 => ReadInt2(ref input, signNegative: false),
            Protocol18Type.Int2Negative => ReadInt2(ref input, signNegative: true),
            Protocol18Type.Long1 => ReadLong1(ref input, signNegative: false),
            Protocol18Type.Long1Negative => ReadLong1(ref input, signNegative: true),
            Protocol18Type.Long2 => ReadLong2(ref input, signNegative: false),
            Protocol18Type.Long2Negative => ReadLong2(ref input, signNegative: true),
            Protocol18Type.Custom => DeserializeCustomType(ref input),
            Protocol18Type.Dictionary => DeserializeDictionary(ref input),
            Protocol18Type.Hashtable => DeserializeHashtable(ref input),
            Protocol18Type.ObjectArray => DeserializeObjectArray(ref input),
            Protocol18Type.OperationRequest => DeserializeOperationRequest(ref input),
            Protocol18Type.OperationResponse => DeserializeOperationResponse(ref input),
            Protocol18Type.EventData => DeserializeEventData(ref input),
            Protocol18Type.BooleanFalse => false,
            Protocol18Type.BooleanTrue => true,
            Protocol18Type.ShortZero => (short) 0,
            Protocol18Type.IntZero => 0,
            Protocol18Type.LongZero => 0L,
            Protocol18Type.FloatZero => 0f,
            Protocol18Type.DoubleZero => 0d,
            Protocol18Type.ByteZero => (byte) 0,
            Protocol18Type.Array => DeserializeArrayInArray(ref input),
            Protocol18Type.BooleanArray => DeserializeBooleanArray(ref input),
            Protocol18Type.ByteArray => DeserializeByteArray(ref input),
            Protocol18Type.ShortArray => DeserializeShortArray(ref input),
            Protocol18Type.FloatArray => DeserializeFloatArray(ref input),
            Protocol18Type.DoubleArray => DeserializeDoubleArray(ref input),
            Protocol18Type.StringArray => DeserializeStringArray(ref input),
            Protocol18Type.CompressedIntArray => DeserializeCompressedIntArray(ref input),
            Protocol18Type.CompressedLongArray => DeserializeCompressedLongArray(ref input),
            Protocol18Type.CustomTypeArray => DeserializeCustomTypeArray(ref input),
            Protocol18Type.DictionaryArray => DeserializeDictionaryArray(ref input),
            Protocol18Type.HashtableArray => DeserializeHashtableArray(ref input),
            _ => throw new ArgumentException($"Protocol18 type code {typeCode} is not supported."),
        };
    }

    private static OperationRequest DeserializeOperationRequest(ref Protocol18Reader input)
    {
        byte operationCode = ReadByte(ref input);
        Dictionary<byte, object> parameters = DeserializeParameterTable(ref input);

        return new OperationRequest(operationCode, parameters);
    }

    private static OperationResponse DeserializeOperationResponse(ref Protocol18Reader input)
    {
        byte operationCode = ReadByte(ref input);
        short returnCode = DeserializeShort(ref input);
        string debugMessage = Deserialize(ref input, ReadByte(ref input)) as string ?? string.Empty;
        Dictionary<byte, object> parameters = DeserializeParameterTable(ref input);

        return new OperationResponse(operationCode, returnCode, debugMessage, parameters);
    }

    private static EventData DeserializeEventData(ref Protocol18Reader input)
    {
        byte code = ReadByte(ref input);
        Dictionary<byte, object> parameters = DeserializeParameterTable(ref input);

        return new EventData(code, parameters);
    }

    private static short DeserializeShort(ref Protocol18Reader input)
    {
        byte[] buffer = GetScalarBuffer();
        ReadExactly(ref input, buffer, sizeof(short));

        return (short) (buffer[0] | (buffer[1] << 8));
    }

    private static bool DeserializeBoolean(ref Protocol18Reader input)
    {
        return ReadByte(ref input) != 0;
    }

    private static float DeserializeFloat(ref Protocol18Reader input)
    {
        byte[] buffer = GetScalarBuffer();
        ReadExactly(ref input, buffer, sizeof(float));

        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(buffer, 0, sizeof(float));
        }

        return BitConverter.ToSingle(buffer, 0);
    }

    private static double DeserializeDouble(ref Protocol18Reader input)
    {
        byte[] buffer = GetScalarBuffer();
        ReadExactly(ref input, buffer, sizeof(double));

        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(buffer, 0, sizeof(double));
        }

        return BitConverter.ToDouble(buffer, 0);
    }

    private static string DeserializeString(ref Protocol18Reader input)
    {
        int stringLength = checked((int) ReadCompressedUInt32(ref input));
        if (stringLength == 0)
        {
            return string.Empty;
        }

        byte[] buffer = new byte[stringLength];
        ReadExactly(ref input, buffer, stringLength);

        return Encoding.UTF8.GetString(buffer, 0, stringLength);
    }

    private static byte[] DeserializeByteArray(ref Protocol18Reader input)
    {
        int arrayLength = checked((int) ReadCompressedUInt32(ref input));
        if (arrayLength == 0)
        {
            return [];
        }

        byte[] buffer = new byte[arrayLength];
        ReadExactly(ref input, buffer, arrayLength);

        return buffer;
    }

    private static short[] DeserializeShortArray(ref Protocol18Reader input)
    {
        int arrayLength = checked((int) ReadCompressedUInt32(ref input));
        var array = new short[arrayLength];
        for (int i = 0; i < arrayLength; i++)
        {
            array[i] = DeserializeShort(ref input);
        }

        return array;
    }

    private static float[] DeserializeFloatArray(ref Protocol18Reader input)
    {
        int arrayLength = checked((int) ReadCompressedUInt32(ref input));
        int byteLength = checked(arrayLength * sizeof(float));
        var array = new float[arrayLength];
        if (byteLength == 0)
        {
            return array;
        }

        byte[] buffer = new byte[byteLength];
        ReadExactly(ref input, buffer, byteLength);

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

    private static double[] DeserializeDoubleArray(ref Protocol18Reader input)
    {
        int arrayLength = checked((int) ReadCompressedUInt32(ref input));
        int byteLength = checked(arrayLength * sizeof(double));
        var array = new double[arrayLength];
        if (byteLength == 0)
        {
            return array;
        }

        byte[] buffer = new byte[byteLength];
        ReadExactly(ref input, buffer, byteLength);

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

    private static bool[] DeserializeBooleanArray(ref Protocol18Reader input)
    {
        int arrayLength = checked((int) ReadCompressedUInt32(ref input));
        var array = new bool[arrayLength];
        int fullByteCount = arrayLength / 8;
        int index = 0;

        for (int i = 0; i < fullByteCount; i++)
        {
            byte value = ReadByte(ref input);
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
            byte value = ReadByte(ref input);
            int bitIndex = 0;
            while (index < arrayLength)
            {
                array[index++] = (value & BoolMasks[bitIndex]) == BoolMasks[bitIndex];
                bitIndex++;
            }
        }

        return array;
    }

    private static string[] DeserializeStringArray(ref Protocol18Reader input)
    {
        int arrayLength = checked((int) ReadCompressedUInt32(ref input));
        var array = new string[arrayLength];
        for (int i = 0; i < arrayLength; i++)
        {
            array[i] = DeserializeString(ref input);
        }

        return array;
    }

    private static int[] DeserializeCompressedIntArray(ref Protocol18Reader input)
    {
        int arrayLength = checked((int) ReadCompressedUInt32(ref input));
        var array = new int[arrayLength];
        for (int i = 0; i < arrayLength; i++)
        {
            array[i] = ReadCompressedInt32(ref input);
        }

        return array;
    }

    private static long[] DeserializeCompressedLongArray(ref Protocol18Reader input)
    {
        int arrayLength = checked((int) ReadCompressedUInt32(ref input));
        var array = new long[arrayLength];
        for (int i = 0; i < arrayLength; i++)
        {
            array[i] = ReadCompressedInt64(ref input);
        }

        return array;
    }

    private static object[] DeserializeObjectArray(ref Protocol18Reader input)
    {
        int arrayLength = checked((int) ReadCompressedUInt32(ref input));
        var array = new object[arrayLength];
        for (int i = 0; i < arrayLength; i++)
        {
            array[i] = Deserialize(ref input)!;
        }

        return array;
    }

    private static Hashtable[] DeserializeHashtableArray(ref Protocol18Reader input)
    {
        int arrayLength = checked((int) ReadCompressedUInt32(ref input));
        var array = new Hashtable[arrayLength];
        for (int i = 0; i < arrayLength; i++)
        {
            array[i] = DeserializeHashtable(ref input);
        }

        return array;
    }

    private static IDictionary[] DeserializeDictionaryArray(ref Protocol18Reader input)
    {
        Type dictionaryType = DeserializeDictionaryType(ref input, out Protocol18Type keyTypeCode, out Protocol18Type valueTypeCode);
        int arrayLength = checked((int) ReadCompressedUInt32(ref input));
        var array = (IDictionary[]) Array.CreateInstance(dictionaryType, arrayLength);

        for (int i = 0; i < arrayLength; i++)
        {
            if (Activator.CreateInstance(dictionaryType) is not IDictionary dictionary)
            {
                throw new InvalidOperationException($"Could not create dictionary type '{dictionaryType}'.");
            }

            DeserializeDictionaryElements(ref input, dictionary, keyTypeCode, valueTypeCode);
            array[i] = dictionary;
        }

        return array;
    }

    private static Array? DeserializeArrayInArray(ref Protocol18Reader input)
    {
        int arrayLength = checked((int) ReadCompressedUInt32(ref input));
        Array? result = null;
        Type? resultType = null;

        for (int i = 0; i < arrayLength; i++)
        {
            object? value = Deserialize(ref input);
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

    private static Hashtable DeserializeHashtable(ref Protocol18Reader input)
    {
        int size = checked((int) ReadCompressedUInt32(ref input));
        var output = new Hashtable(size);

        for (int i = 0; i < size; i++)
        {
            object? key = Deserialize(ref input);
            object? value = Deserialize(ref input);
            if (key != null)
            {
                output[key] = value;
            }
        }

        return output;
    }

    private static IDictionary DeserializeDictionary(ref Protocol18Reader input)
    {
        Type dictionaryType = DeserializeDictionaryType(ref input, out Protocol18Type keyTypeCode, out Protocol18Type valueTypeCode);
        if (Activator.CreateInstance(dictionaryType) is not IDictionary dictionary)
        {
            throw new InvalidOperationException($"Could not create dictionary type '{dictionaryType}'.");
        }

        DeserializeDictionaryElements(ref input, dictionary, keyTypeCode, valueTypeCode);
        return dictionary;
    }

    private static void DeserializeDictionaryElements(ref Protocol18Reader input, IDictionary dictionary, Protocol18Type keyTypeCode, Protocol18Type valueTypeCode)
    {
        int size = checked((int) ReadCompressedUInt32(ref input));
        for (int i = 0; i < size; i++)
        {
            object? key = keyTypeCode == Protocol18Type.Unknown
                ? Deserialize(ref input)
                : Deserialize(ref input, (byte) keyTypeCode);
            object? value = valueTypeCode == Protocol18Type.Unknown
                ? Deserialize(ref input)
                : Deserialize(ref input, (byte) valueTypeCode);

            if (key != null)
            {
                dictionary.Add(key, value);
            }
        }
    }

    private static Type DeserializeDictionaryType(ref Protocol18Reader input, out Protocol18Type keyTypeCode, out Protocol18Type valueTypeCode)
    {
        keyTypeCode = (Protocol18Type) ReadByte(ref input);
        valueTypeCode = (Protocol18Type) ReadByte(ref input);

        Type keyType = keyTypeCode == Protocol18Type.Unknown
            ? typeof(object)
            : GetAllowedDictionaryKeyType(keyTypeCode);

        Type valueType = valueTypeCode switch
        {
            Protocol18Type.Unknown => typeof(object),
            Protocol18Type.Dictionary => DeserializeDictionaryType(ref input),
            Protocol18Type.Array => GetDictionaryArrayType(ref input),
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

    private static Type DeserializeDictionaryType(ref Protocol18Reader input)
    {
        Protocol18Type keyTypeCode = (Protocol18Type) ReadByte(ref input);
        Protocol18Type valueTypeCode = (Protocol18Type) ReadByte(ref input);

        Type keyType = keyTypeCode == Protocol18Type.Unknown
            ? typeof(object)
            : GetAllowedDictionaryKeyType(keyTypeCode);

        Type valueType = valueTypeCode switch
        {
            Protocol18Type.Unknown => typeof(object),
            Protocol18Type.Dictionary => DeserializeDictionaryType(ref input),
            Protocol18Type.Array => GetDictionaryArrayType(ref input),
            _ => GetClrArrayType(valueTypeCode),
        };

        return typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
    }

    private static Type GetDictionaryArrayType(ref Protocol18Reader input)
    {
        Protocol18Type typeCode = (Protocol18Type) ReadByte(ref input);
        int nestedArrayDepth = 0;

        while (typeCode == Protocol18Type.Array)
        {
            nestedArrayDepth++;
            typeCode = (Protocol18Type) ReadByte(ref input);
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

    private static Dictionary<byte, object> DeserializeParameterTable(ref Protocol18Reader input)
    {
        int size = ReadByte(ref input);
        var parameters = new Dictionary<byte, object>(size);
        for (int i = 0; i < size; i++)
        {
            byte key = ReadByte(ref input);
            byte valueTypeCode = ReadByte(ref input);
            parameters[key] = Deserialize(ref input, valueTypeCode)!;
        }

        return parameters;
    }

    private static Protocol18CustomType DeserializeCustomType(ref Protocol18Reader input, byte slimTypeCode = 0)
    {
        byte typeCode = slimTypeCode == 0
            ? ReadByte(ref input)
            : (byte) (slimTypeCode - (byte) Protocol18Type.CustomTypeSlim);
        int length = checked((int) ReadCompressedUInt32(ref input));
        byte[] data = ReadBytes(ref input, length);

        return new Protocol18CustomType(typeCode, data);
    }

    private static Protocol18CustomType[] DeserializeCustomTypeArray(ref Protocol18Reader input)
    {
        int arrayLength = checked((int) ReadCompressedUInt32(ref input));
        byte typeCode = ReadByte(ref input);
        var array = new Protocol18CustomType[arrayLength];

        for (int i = 0; i < arrayLength; i++)
        {
            int length = checked((int) ReadCompressedUInt32(ref input));
            byte[] data = ReadBytes(ref input, length);
            array[i] = new Protocol18CustomType(typeCode, data);
        }

        return array;
    }

    private static int ReadInt1(ref Protocol18Reader input, bool signNegative)
    {
        int value = ReadByte(ref input);
        return signNegative ? -value : value;
    }

    private static int ReadInt2(ref Protocol18Reader input, bool signNegative)
    {
        int value = ReadUShort(ref input);
        return signNegative ? -value : value;
    }

    private static long ReadLong1(ref Protocol18Reader input, bool signNegative)
    {
        long value = ReadByte(ref input);
        return signNegative ? -value : value;
    }

    private static long ReadLong2(ref Protocol18Reader input, bool signNegative)
    {
        long value = ReadUShort(ref input);
        return signNegative ? -value : value;
    }

    private static int ReadCompressedInt32(ref Protocol18Reader input)
    {
        return DecodeZigZag32(ReadCompressedUInt32(ref input));
    }

    private static long ReadCompressedInt64(ref Protocol18Reader input)
    {
        return DecodeZigZag64(ReadCompressedUInt64(ref input));
    }

    private static uint ReadCompressedUInt32(ref Protocol18Reader input)
    {
        uint value = 0;
        int shift = 0;

        while (shift != 35)
        {
            byte current = ReadByte(ref input);
            value |= (uint) (current & 0x7F) << shift;
            shift += 7;

            if ((current & 0x80) == 0)
            {
                return value;
            }
        }

        return value;
    }

    private static ulong ReadCompressedUInt64(ref Protocol18Reader input)
    {
        ulong value = 0;
        int shift = 0;

        while (shift != 70)
        {
            byte current = ReadByte(ref input);
            value |= (ulong) (current & 0x7F) << shift;
            shift += 7;

            if ((current & 0x80) == 0)
            {
                return value;
            }
        }

        return value;
    }

    private static ushort ReadUShort(ref Protocol18Reader input)
    {
        byte[] buffer = GetScalarBuffer();
        ReadExactly(ref input, buffer, sizeof(ushort));

        return (ushort) (buffer[0] | (buffer[1] << 8));
    }

    private static byte ReadByte(ref Protocol18Reader input)
    {
        return input.ReadByte();
    }

    private static byte[] ReadBytes(ref Protocol18Reader input, int length)
    {
        if (length == 0)
        {
            return [];
        }

        byte[] buffer = new byte[length];
        ReadExactly(ref input, buffer, length);
        return buffer;
    }

    private static void ReadExactly(ref Protocol18Reader input, byte[] buffer, int count)
    {
        input.ReadSpan(count).CopyTo(buffer);
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
