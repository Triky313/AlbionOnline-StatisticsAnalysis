using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models;

public class LocalizedDescriptions
{
    [JsonPropertyName("EN-US")] public string EnUs { get; set; }

    [JsonPropertyName("DE-DE")] public string DeDe { get; set; }

    [JsonPropertyName("KO-KR")] public string KoKr { get; set; }

    [JsonPropertyName("RU-RUS")] public string RuRu { get; set; }

    [JsonPropertyName("PL-PL")] public string PlPl { get; set; }

    [JsonPropertyName("PT-BR")] public string PtBr { get; set; }

    [JsonPropertyName("FR-FR")] public string FrFr { get; set; }

    [JsonPropertyName("ES-ES")] public string EsEs { get; set; }

    [JsonPropertyName("ZH-CN")] public string ZhCn { get; set; }
}