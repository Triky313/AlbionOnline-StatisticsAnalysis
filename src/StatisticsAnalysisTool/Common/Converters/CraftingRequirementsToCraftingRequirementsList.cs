using StatisticsAnalysisTool.Models.ItemsJsonModel;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Common.Converters;

public class CraftingRequirementsToCraftingRequirementsList : JsonConverter<List<CraftingRequirements>>
{
    public override List<CraftingRequirements> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject && reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("JSON payload expected to start with StartObject or StartArray token.");
        }

        var craftingRequirements = new List<CraftingRequirements>();
        
        try
        {
            craftingRequirements = reader.TokenType switch
            {
                JsonTokenType.StartArray => SetCraftingRequirementsArray(ref reader),
                JsonTokenType.StartObject => SetCraftingRequirementsObject(ref reader),
                _ => craftingRequirements
            };
        }
        catch (Exception)
        {
            return craftingRequirements;
        }

        return craftingRequirements;
    }

    public override void Write(Utf8JsonWriter writer, List<CraftingRequirements> value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }


    public static List<CraftingRequirements> SetCraftingRequirementsArray(ref Utf8JsonReader reader)
    {
        var craftingRequirements = new List<CraftingRequirements>();
        var craftingRequirement = new CraftingRequirements();
        var startDepth = reader.CurrentDepth;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray && reader.CurrentDepth == startDepth)
            {
                return craftingRequirements;
            }

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                craftingRequirement = new CraftingRequirements();
            }

            if (reader.TokenType == JsonTokenType.EndObject)
            {
                craftingRequirements.Add(craftingRequirement);
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();

                switch (propertyName)
                {
                    case "@silver":
                        reader.Read();
                        craftingRequirement.Silver = reader.GetString();
                        break;
                    case "@time":
                        reader.Read();
                        craftingRequirement.Time = reader.GetString();
                        break;
                    case "@craftingfocus":
                        reader.Read();
                        craftingRequirement.CraftingFocus = reader.GetString();
                        break;
                    case "craftresource":
                        reader.Read();
                        craftingRequirement.CraftResource = reader.TokenType switch
                        {
                            JsonTokenType.StartArray => SetCraftResourceArray(ref reader),
                            JsonTokenType.StartObject => SetCraftResourceObject(ref reader),
                            _ => craftingRequirement.CraftResource
                        };
                        break;
                    case "@swaptransaction":
                        reader.Read();
                        craftingRequirement.SwapTransaction = reader.GetString();
                        break;
                    case "playerfactionstanding":
                        reader.Read();
                        craftingRequirement.PlayerFactionStanding = SetPlayerFactionStanding(ref reader);
                        break;
                    case "currency":
                        reader.Read();
                        craftingRequirement.Currency = SetCurrency(ref reader);
                        break;
                    case "@amountcrafted":
                        reader.Read();
                        craftingRequirement.AmountCrafted = reader.GetString();
                        break;
                    case "@forcesinglecraft":
                        reader.Read();
                        craftingRequirement.ForceSingleCraft = reader.GetString();
                        break;
                }
            }
        }

        return craftingRequirements;
    }

    public static List<CraftingRequirements> SetCraftingRequirementsObject(ref Utf8JsonReader reader)
    {
        var craftingRequirements = new List<CraftingRequirements>();
        var craftingRequirement = new CraftingRequirements();
        var startDepth = reader.CurrentDepth;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject && reader.CurrentDepth == startDepth)
            {
                craftingRequirements.Add(craftingRequirement);
                return craftingRequirements;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();

                switch (propertyName)
                {
                    case "@silver":
                        reader.Read();
                        craftingRequirement.Silver = reader.GetString() ?? string.Empty;
                        break;
                    case "@time":
                        reader.Read();
                        craftingRequirement.Time = reader.GetString() ?? string.Empty;
                        break;
                    case "@craftingfocus":
                        reader.Read();
                        craftingRequirement.CraftingFocus = reader.GetString() ?? string.Empty;
                        break;
                    case "craftresource":
                        reader.Read();
                        craftingRequirement.CraftResource = reader.TokenType switch
                        {
                            JsonTokenType.StartArray => SetCraftResourceArray(ref reader),
                            JsonTokenType.StartObject => SetCraftResourceObject(ref reader),
                            _ => craftingRequirement.CraftResource
                        };
                        break;
                    case "@swaptransaction":
                        reader.Read();
                        craftingRequirement.SwapTransaction = reader.GetString() ?? string.Empty;
                        break;
                    case "playerfactionstanding":
                        reader.Read();
                        craftingRequirement.PlayerFactionStanding = SetPlayerFactionStanding(ref reader) ?? new PlayerFactionStanding();
                        break;
                    case "currency":
                        reader.Read();
                        craftingRequirement.Currency = SetCurrency(ref reader) ?? new Currency();
                        break;
                    case "@amountcrafted":
                        reader.Read();
                        craftingRequirement.AmountCrafted = reader.GetString() ?? string.Empty;
                        break;
                    case "@forcesinglecraft":
                        reader.Read();
                        craftingRequirement.ForceSingleCraft = reader.GetString() ?? string.Empty;
                        break;
                }
            }
        }

        return craftingRequirements;
    }

    private static List<CraftResource> SetCraftResourceArray(ref Utf8JsonReader reader)
    {
        var craftResources = new List<CraftResource>();
        var craftResource = new CraftResource();
        var startDepth = reader.CurrentDepth;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray && reader.CurrentDepth == startDepth)
            {
                return craftResources;
            }

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                craftResource = new CraftResource();
            }

            if (reader.TokenType == JsonTokenType.EndObject)
            {
                craftResources.Add(craftResource);
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();

                switch (propertyName)
                {
                    case "@uniquename":
                        reader.Read();
                        craftResource.UniqueName = reader.GetString();
                        break;
                    case "@count":
                        reader.Read();
                        if (int.TryParse(reader.GetString(), out var count))
                        {
                            craftResource.Count = count;
                            break;
                        }
                        craftResource.Count = 0;
                        break;
                    case "@maxreturnamount":
                        reader.Read();
                        craftResource.MaxReturnAmount = reader.GetString();
                        break;
                    case "@enchantmentlevel":
                        reader.Read();
                        craftResource.EnchantmentLevel = reader.GetString();
                        break;
                }
            }
        }

        return craftResources;
    }

    private static List<CraftResource> SetCraftResourceObject(ref Utf8JsonReader reader)
    {
        var craftResources = new List<CraftResource>();
        var newCraftResource = new CraftResource();
        var startDepth = reader.CurrentDepth;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject && reader.CurrentDepth == startDepth)
            {
                craftResources.Add(newCraftResource);
                return craftResources;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();

                switch (propertyName)
                {
                    case "@uniquename":
                        reader.Read();
                        newCraftResource.UniqueName = reader.GetString();
                        break;
                    case "@count":
                        reader.Read();
                        if (int.TryParse(reader.GetString(), out var count))
                        {
                            newCraftResource.Count = count;
                            break;
                        }
                        newCraftResource.Count = 0;
                        break;
                    case "@maxreturnamount":
                        reader.Read();
                        newCraftResource.MaxReturnAmount = reader.GetString();
                        break;
                    case "@enchantmentlevel":
                        reader.Read();
                        newCraftResource.EnchantmentLevel = reader.GetString();
                        break;
                }
            }
        }

        return craftResources;
    }

    private static PlayerFactionStanding SetPlayerFactionStanding(ref Utf8JsonReader reader)
    {
        var playerFactionStanding = new PlayerFactionStanding();
        var startDepth = reader.CurrentDepth;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject && reader.CurrentDepth == startDepth)
            {
                return playerFactionStanding;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                switch (propertyName)
                {
                    case "@faction":
                        reader.Read();
                        playerFactionStanding.Faction = reader.GetString();
                        break;
                    case "@minstanding":
                        reader.Read();
                        playerFactionStanding.MinStanding = reader.GetString();
                        break;
                }
            }
        }

        return playerFactionStanding;
    }
    
    private static Currency SetCurrency(ref Utf8JsonReader reader)
    {
        var currency = new Currency();
        var startDepth = reader.CurrentDepth;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject && reader.CurrentDepth == startDepth)
            {
                return currency;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                switch (propertyName)
                {
                    case "@uniquename":
                        reader.Read();
                        currency.UniqueName = reader.GetString();
                        break;
                    case "@amount":
                        reader.Read();
                        currency.Amount = reader.GetString();
                        break;
                }
            }
        }

        return currency;
    }
}