namespace StatisticsAnalysisTool.Protocol16;

internal enum Protocol16Type : byte
{
    Unknown = 0,            // \0
    Null = 42,              // *
    Dictionary = 68,        // D
    StringArray = 97,       // a
    Byte = 98,              // b
    Double = 100,           // d
    EventData = 101,        // e
    Float = 102,            // f
    Integer = 105,          // i
    Hashtable = 104,        // j
    Short = 107,            // k
    Long = 108,             // l
    IntegerArray = 110,     // n
    Boolean = 111,          // o
    OperationResponse = 112,// p
    OperationRequest = 113, // q
    String = 115,           // s
    ByteArray = 120,        // x
    Array = 121,            // y
    ObjectArray = 122,      // z
}