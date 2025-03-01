using System.Collections;
using System.Text;
using StatisticsAnalysisTool.Protocol16.Photon;

namespace StatisticsAnalysisTool.Protocol16;

public static class Protocol16Deserializer
{
    private static readonly ThreadLocal<byte[]> _byteBuffer = new ThreadLocal<byte[]>(() => new byte[sizeof(long)]);
    #region methods

    public static object Deserialize(Protocol16Stream input)
    {
        return Deserialize(input, (byte) input.ReadByte());
    }

    public static object Deserialize(Protocol16Stream input, byte typeCode)
    {
        switch ((Protocol16Type) typeCode)
        {
            case Protocol16Type.Unknown:
            case Protocol16Type.Null:
                return null;
            case Protocol16Type.Dictionary:
                return DeserializeDictionary(input);
            case Protocol16Type.StringArray:
                return DeserializeStringArray(input);
            case Protocol16Type.Byte:
                return DeserializeByte(input);
            case Protocol16Type.Double:
                return DeserializeDouble(input);
            case Protocol16Type.EventData:
                return DeserializeEventData(input);
            case Protocol16Type.Float:
                return DeserializeFloat(input);
            case Protocol16Type.Integer:
                return DeserializeInteger(input);
            case Protocol16Type.Hashtable:
                return DeserializeHashtable(input);
            case Protocol16Type.Short:
                return DeserializeShort(input);
            case Protocol16Type.Long:
                return DeserializeLong(input);
            case Protocol16Type.IntegerArray:
                return DeserializeIntArray(input);
            case Protocol16Type.Boolean:
                return DeserializeBoolean(input);
            case Protocol16Type.OperationResponse:
                return DeserializeOperationResponse(input);
            case Protocol16Type.OperationRequest:
                return DeserializeOperationRequest(input);
            case Protocol16Type.String:
                return DeserializeString(input);
            case Protocol16Type.ByteArray:
                return DeserializeByteArray(input);
            case Protocol16Type.Array:
                return DeserializeArray(input);
            case Protocol16Type.ObjectArray:
                return DeserializeObjectArray(input);
            default:
                throw new ArgumentException($"Type code: {typeCode} not implemented.");
        }
    }

    public static OperationRequest DeserializeOperationRequest(Protocol16Stream input)
    {
        byte operationCode = DeserializeByte(input);
        Dictionary<byte, object> parameters = DeserializeParameterTable(input);

        return new OperationRequest(operationCode, parameters);
    }

    public static OperationResponse DeserializeOperationResponse(Protocol16Stream input)
    {
        byte operationCode = DeserializeByte(input);
        short returnCode = DeserializeShort(input);
        string debugMessage = (Deserialize(input, DeserializeByte(input)) as string);
        Dictionary<byte, object> parameters = DeserializeParameterTable(input);

        return new OperationResponse(operationCode, returnCode, debugMessage, parameters);
    }

    public static EventData DeserializeEventData(Protocol16Stream input)
    {
        byte code = DeserializeByte(input);
        Dictionary<byte, object> parameters = DeserializeParameterTable(input);

        return new EventData(code, parameters);
    }
    #endregion

    #region private methods
    private static Type GetTypeOfCode(byte typeCode)
    {
        switch ((Protocol16Type) typeCode)
        {
            case Protocol16Type.Unknown:
            case Protocol16Type.Null:
                return typeof(object);
            case Protocol16Type.Dictionary:
                return typeof(IDictionary);
            case Protocol16Type.StringArray:
                return typeof(string[]);
            case Protocol16Type.Byte:
                return typeof(byte);
            case Protocol16Type.Double:
                return typeof(double);
            case Protocol16Type.EventData:
                return typeof(EventData);
            case Protocol16Type.Float:
                return typeof(float);
            case Protocol16Type.Integer:
                return typeof(int);
            case Protocol16Type.Short:
                return typeof(short);
            case Protocol16Type.Long:
                return typeof(long);
            case Protocol16Type.IntegerArray:
                return typeof(int[]);
            case Protocol16Type.Boolean:
                return typeof(bool);
            case Protocol16Type.OperationResponse:
                return typeof(OperationResponse);
            case Protocol16Type.OperationRequest:
                return typeof(OperationRequest);
            case Protocol16Type.String:
                return typeof(string);
            case Protocol16Type.ByteArray:
                return typeof(byte[]);
            case Protocol16Type.Array:
                return typeof(Array);
            case Protocol16Type.ObjectArray:
                return typeof(object[]);
            default:
                throw new ArgumentException($"Type code: {typeCode} not implemented.");
        }
    }

    private static byte DeserializeByte(Stream stream)
    {
        return (byte) stream.ReadByte();
    }

    private static bool DeserializeBoolean(Stream input)
    {
        return input.ReadByte() != 0;
    }

    public static short DeserializeShort(Protocol16Stream input)
    {
        var buffer = _byteBuffer.Value;
        _ = input.Read(buffer, 0, sizeof(short));

        return (short) (buffer[0] << 8 | buffer[1]);
    }

    private static int DeserializeInteger(Stream input)
    {
        var buffer = _byteBuffer.Value;
        _ = input.Read(buffer, 0, sizeof(int));

        return buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3];
    }

    private static object DeserializeHashtable(Protocol16Stream input)
    {
        int size = DeserializeShort(input);
        var output = new Hashtable(size);
        DeserializeDictionaryElements(input, output, size, (byte) Protocol16Type.Unknown, (byte) Protocol16Type.Unknown);
        return output;
    }

    private static long DeserializeLong(Stream input)
    {
        var buffer = _byteBuffer.Value;
        _ = input.Read(buffer, 0, sizeof(long));

        if (BitConverter.IsLittleEndian)
        {
            return (long) buffer[0] << 56 | (long) buffer[1] << 48 | (long) buffer[2] << 40 | (long) buffer[3] << 32 | (long) buffer[4] << 24 | (long) buffer[5] << 16 | (long) buffer[6] << 8 | buffer[7];
        }

        return BitConverter.ToInt64(buffer, 0);
    }

    private static float DeserializeFloat(Stream input)
    {
        var buffer = _byteBuffer.Value;
        _ = input.Read(buffer, 0, sizeof(float));

        if (BitConverter.IsLittleEndian)
        {
            byte b0 = buffer[0];
            byte b1 = buffer[1];
            buffer[0] = buffer[3];
            buffer[1] = buffer[2];
            buffer[2] = b1;
            buffer[3] = b0;
        }

        return BitConverter.ToSingle(buffer, 0);
    }

    private static double DeserializeDouble(Protocol16Stream input)
    {
        var buffer = _byteBuffer.Value;
        _ = input.Read(buffer, 0, sizeof(double));

        if (BitConverter.IsLittleEndian)
        {
            byte b0 = buffer[0];
            byte b1 = buffer[1];
            byte b2 = buffer[2];
            byte b3 = buffer[3];
            buffer[0] = buffer[7];
            buffer[1] = buffer[6];
            buffer[2] = buffer[5];
            buffer[3] = buffer[4];
            buffer[4] = b3;
            buffer[5] = b2;
            buffer[6] = b1;
            buffer[7] = b0;
        }

        return BitConverter.ToDouble(buffer, 0);
    }

    private static string DeserializeString(Protocol16Stream input)
    {
        int stringSize = DeserializeShort(input);
        if (stringSize == 0)
        {
            return string.Empty;
        }

        var buffer = new byte[stringSize];

        _ = input.Read(buffer, 0, stringSize);

        return Encoding.UTF8.GetString(buffer, 0, stringSize);
    }

    private static byte[] DeserializeByteArray(Protocol16Stream input)
    {
        int arraySize = DeserializeInteger(input);

        var buffer = new byte[arraySize];
        _ = input.Read(buffer, 0, arraySize);

        return buffer;
    }

    private static int[] DeserializeIntArray(Protocol16Stream input)
    {
        int arraySize = DeserializeInteger(input);

        var array = new int[arraySize];
        for (int i = 0; i < arraySize; i++)
        {
            array[i] = DeserializeInteger(input);
        }

        return array;
    }

    private static string[] DeserializeStringArray(Protocol16Stream input)
    {
        int arraySize = DeserializeShort(input);

        var array = new string[arraySize];
        for (int i = 0; i < arraySize; i++)
        {
            array[i] = DeserializeString(input);
        }

        return array;
    }

    private static object[] DeserializeObjectArray(Protocol16Stream input)
    {
        int arraySize = DeserializeShort(input);

        var array = new object[arraySize];
        for (int i = 0; i < arraySize; i++)
        {
            byte typeCode = (byte) input.ReadByte();
            array[i] = Deserialize(input, typeCode);
        }

        return array;
    }

    private static IDictionary DeserializeDictionary(Protocol16Stream input)
    {
        byte keyTypeCode = (byte) input.ReadByte();
        byte valueTypeCode = (byte) input.ReadByte();
        int dictionarySize = DeserializeShort(input);
        Type keyType = GetTypeOfCode(keyTypeCode);
        Type valueType = GetTypeOfCode(valueTypeCode);
        Type dictionaryType = typeof(Dictionary<,>).MakeGenericType(new Type[]
        {
                keyType,
                valueType
        });

        IDictionary output = Activator.CreateInstance(dictionaryType) as IDictionary;
        DeserializeDictionaryElements(input, output, dictionarySize, keyTypeCode, valueTypeCode);
        return output;
    }

    private static void DeserializeDictionaryElements(Protocol16Stream input, IDictionary output, int dictionarySize, byte keyTypeCode, byte valueTypeCode)
    {
        for (int i = 0; i < dictionarySize; i++)
        {
            object key = Deserialize(input, (keyTypeCode == 0 || keyTypeCode == 42) ? ((byte) input.ReadByte()) : keyTypeCode);
            object value = Deserialize(input, (valueTypeCode == 0 || valueTypeCode == 42) ? ((byte) input.ReadByte()) : valueTypeCode);
            output.Add(key, value);
        }
    }

    private static void DeserializeDictionaryArray(Protocol16Stream input, short size, out Array result)
    {
        var type = DeserializeDictionaryType(input, out var keyTypeCode, out var valueTypeCode);
        result = Array.CreateInstance(type, size);

        for (short i = 0; i < size; i++)
        {
            if (!(Activator.CreateInstance(type) is IDictionary dictionary))
            {
                return;
            }
            var arraySize = DeserializeShort(input);
            for (var j = 0; j < arraySize; j++)
            {
                object key;
                if (keyTypeCode > 0)
                {
                    key = Deserialize(input, keyTypeCode);
                }
                else
                {
                    byte nextKeyTypeCode = (byte) input.ReadByte();
                    key = Deserialize(input, nextKeyTypeCode);
                }
                object value;
                if (valueTypeCode > 0)
                {
                    value = Deserialize(input, valueTypeCode);
                }
                else
                {
                    byte nextValueTypeCode = (byte) input.ReadByte();
                    value = Deserialize(input, nextValueTypeCode);
                }
                dictionary.Add(key, value);
            }
            result.SetValue(dictionary, i);
        }
    }

    private static Array DeserializeArray(Protocol16Stream input)
    {
        short size = DeserializeShort(input);
        byte typeCode = (byte) input.ReadByte();
        switch ((Protocol16Type) typeCode)
        {
            case Protocol16Type.Array:
                {
                    Array array = DeserializeArray(input);
                    Type arrayType = array.GetType();
                    Array result = Array.CreateInstance(arrayType, size);
                    result.SetValue(array, 0);
                    for (short i = 1; i < size; i++)
                    {
                        array = DeserializeArray(input);
                        result.SetValue(array, i);
                    }

                    return result;
                }
            case Protocol16Type.ByteArray:
                {
                    byte[][] array = new byte[size][];
                    for (short i = 0; i < size; i++)
                    {
                        array[i] = DeserializeByteArray(input);
                    }

                    return array;
                }
            case Protocol16Type.Dictionary:
                {
                    DeserializeDictionaryArray(input, size, out Array result);

                    return result;
                }
            default:
                {
                    Type arrayType = GetTypeOfCode(typeCode);
                    Array result = Array.CreateInstance(arrayType, size);

                    for (short i = 0; i < size; i++)
                    {
                        result.SetValue(Deserialize(input, typeCode), i);
                    }

                    return result;
                }
        }
    }

    private static Type DeserializeDictionaryType(Protocol16Stream input, out byte keyTypeCode, out byte valueTypeCode)
    {
        keyTypeCode = (byte) input.ReadByte();
        valueTypeCode = (byte) input.ReadByte();
        Type keyType = GetTypeOfCode(keyTypeCode);
        Type valueType = GetTypeOfCode(valueTypeCode);

        return typeof(Dictionary<,>).MakeGenericType(new Type[]
        {
                keyType,
                valueType
        });
    }

    private static Dictionary<byte, object> DeserializeParameterTable(Protocol16Stream input)
    {
        int dictionarySize = DeserializeShort(input);
        var dictionary = new Dictionary<byte, object>(dictionarySize);
        for (int i = 0; i < dictionarySize; i++)
        {
            byte key = (byte) input.ReadByte();
            byte valueTypeCode = (byte) input.ReadByte();
            object value = Deserialize(input, valueTypeCode);
            dictionary[key] = value;
        }

        return dictionary;
    }
    #endregion
}