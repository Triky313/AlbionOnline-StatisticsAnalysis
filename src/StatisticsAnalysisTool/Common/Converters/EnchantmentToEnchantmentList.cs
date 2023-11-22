using StatisticsAnalysisTool.Models.ItemsJsonModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Common.Converters;

public class EnchantmentToEnchantmentList : JsonConverter<List<Enchantment>>
{
    public override List<Enchantment> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject && reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("JSON payload expected to start with StartObject or StartArray token.");
        }

        List<Enchantment> enchantmentList;

        try
        {
            enchantmentList = reader.TokenType switch
            {
                JsonTokenType.StartArray => SetEnchantmentArray(ref reader),
                JsonTokenType.StartObject => SetEnchantmentObject(ref reader),
                _ => new List<Enchantment>()
            };
        }
        catch (Exception)
        {
            return new List<Enchantment>();
        }

        return enchantmentList;
    }

    public override void Write(Utf8JsonWriter writer, List<Enchantment> value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    private static List<Enchantment> SetEnchantmentArray(ref Utf8JsonReader reader)
    {
        var enchantmentList = new List<Enchantment>();
        var enchantment = new Enchantment();
        var startDepth = reader.CurrentDepth;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray && reader.CurrentDepth == startDepth)
            {
                return enchantmentList;
            }

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                enchantment.Reset();
            }

            if (reader.TokenType == JsonTokenType.EndObject)
            {
                enchantmentList.Add(enchantment);
                enchantment = new Enchantment();
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();

                if (propertyName == "@enchantmentlevel")
                {
                    reader.Read();
                    enchantment.EnchantmentLevel = reader.GetString();
                }
                else if (propertyName == "@abilitypower")
                {
                    reader.Read();
                    enchantment.AbilityPower = reader.GetString();
                }
                else if (propertyName == "@dummyitempower")
                {
                    reader.Read();
                    enchantment.DummyItemPower = reader.GetString();
                }
                else if (propertyName == "@consumespell")
                {
                    reader.Read();
                    enchantment.ConsumeSpell = reader.GetString();
                }
                else if (propertyName == "@durability")
                {
                    reader.Read();
                    if (double.TryParse(reader.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var durabilityResult))
                    {
                        enchantment.Durability = durabilityResult;
                    }
                    else
                    {
                        enchantment.Durability = 0;
                    }
                }
                else if (propertyName == "craftingrequirements")
                {
                    reader.Read();
                    enchantment.CraftingRequirements = reader.TokenType switch
                    {
                        JsonTokenType.StartArray => CraftingRequirementsToCraftingRequirementsList.SetCraftingRequirementsArray(ref reader),
                        JsonTokenType.StartObject => CraftingRequirementsToCraftingRequirementsList.SetCraftingRequirementsObject(ref reader),
                        _ => enchantment.CraftingRequirements
                    };
                }
                else if (propertyName == "upgraderequirements")
                {
                    reader.Read();
                    enchantment.UpgradeRequirements = SetUpgradeRequirements(ref reader);
                }
            }
        }

        return enchantmentList;
    }

    private static List<Enchantment> SetEnchantmentObject(ref Utf8JsonReader reader)
    {
        var enchantmentList = new List<Enchantment>();
        var enchantment = new Enchantment();
        var startDepth = reader.CurrentDepth;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject && reader.CurrentDepth == startDepth)
            {
                enchantmentList.Add(enchantment);
                return enchantmentList;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();

                switch (propertyName)
                {
                    case "@enchantmentlevel":
                        reader.Read();
                        enchantment.EnchantmentLevel = reader.GetString();
                        break;
                    case "@abilitypower":
                        reader.Read();
                        enchantment.AbilityPower = reader.GetString();
                        break;
                    case "@dummyitempower":
                        reader.Read();
                        enchantment.DummyItemPower = reader.GetString();
                        break;
                    case "@consumespell":
                        reader.Read();
                        enchantment.ConsumeSpell = reader.GetString();
                        break;
                    case "craftingrequirements":
                        reader.Read();
                        enchantment.CraftingRequirements = reader.TokenType switch
                        {
                            JsonTokenType.StartArray => CraftingRequirementsToCraftingRequirementsList.SetCraftingRequirementsArray(ref reader),
                            JsonTokenType.StartObject => CraftingRequirementsToCraftingRequirementsList.SetCraftingRequirementsObject(ref reader),
                            _ => enchantment.CraftingRequirements
                        };
                        break;
                    case "upgraderequirements":
                        reader.Read();
                        enchantment.UpgradeRequirements = SetUpgradeRequirements(ref reader);
                        break;
                }
            }
        }

        return enchantmentList;
    }

    private static UpgradeRequirements SetUpgradeRequirements(ref Utf8JsonReader reader)
    {
        var upgradeRequirements = new UpgradeRequirements();
        var startDepth = reader.CurrentDepth;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject && reader.CurrentDepth == startDepth)
            {
                return upgradeRequirements;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                switch (propertyName)
                {
                    case "upgraderesource":
                        reader.Read();
                        upgradeRequirements.UpgradeResource = SetUpgradeResource(ref reader);
                        break;
                }
            }
        }

        return upgradeRequirements;
    }

    private static UpgradeResource SetUpgradeResource(ref Utf8JsonReader reader)
    {
        var upgradeResource = new UpgradeResource();
        var startDepth = reader.CurrentDepth;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject && reader.CurrentDepth == startDepth)
            {
                return upgradeResource;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                switch (propertyName)
                {
                    case "@uniquename":
                        reader.Read();
                        upgradeResource.UniqueName = reader.GetString();
                        break;
                    case "@count":
                        reader.Read();
                        upgradeResource.Count = reader.GetString();
                        break;
                }
            }
        }

        return upgradeResource;
    }
}