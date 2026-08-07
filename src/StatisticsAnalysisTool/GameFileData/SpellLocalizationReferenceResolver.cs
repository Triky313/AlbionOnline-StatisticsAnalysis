using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace StatisticsAnalysisTool.GameFileData;

internal static class SpellLocalizationReferenceResolver
{
    private const string MissingValue = "-";
    private static readonly Regex DynamicAttributeRegex = new(
        @"^(?<name>.+)_(?<selector>start|end|max|min)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static string Resolve(
        string reference,
        string currentSpellUniqueName,
        IReadOnlyDictionary<string, XElement> spellElementsByUniqueName)
    {
        if (string.IsNullOrWhiteSpace(reference))
        {
            return MissingValue;
        }

        if (!reference.Contains('$'))
        {
            return reference;
        }

        var path = reference.Trim('$');
        var separatorIndex = path.IndexOf('.', StringComparison.Ordinal);
        if (separatorIndex <= 0)
        {
            return MissingValue;
        }

        var rootName = path[..separatorIndex];
        if (!spellElementsByUniqueName.ContainsKey(rootName))
        {
            path = currentSpellUniqueName + "." + path;
            separatorIndex = path.IndexOf('.', StringComparison.Ordinal);
            rootName = path[..separatorIndex];
        }

        if (!spellElementsByUniqueName.TryGetValue(rootName, out var currentElement))
        {
            return MissingValue;
        }

        var segments = path[(separatorIndex + 1)..].Split('.');
        for (var index = 0; index < segments.Length - 1; index++)
        {
            currentElement = GetElement(currentElement, segments[index]);
            if (currentElement == null)
            {
                return MissingValue;
            }
        }

        var attributeName = segments[^1];
        var value = ResolveAttributeValue(currentElement, attributeName);
        return FormatValue(value, attributeName, currentElement);
    }

    public static string GetChannelingDuration(XElement channelingElement)
    {
        return TryCalculateSequenceDuration(
            channelingElement,
            "effectcount",
            "initialeffectinterval",
            "effectinterval",
            out var duration)
            ? duration.ToString("0.##", CultureInfo.InvariantCulture)
            : string.Empty;
    }

    private static XElement GetElement(XElement parent, string pathSegment)
    {
        var elementIndex = 0;
        var elementName = pathSegment;
        var bracketIndex = pathSegment.IndexOf('[', StringComparison.Ordinal);
        if (bracketIndex >= 0)
        {
            var closingBracketIndex = pathSegment.IndexOf(']', bracketIndex);
            if (closingBracketIndex > bracketIndex)
            {
                int.TryParse(
                    pathSegment[(bracketIndex + 1)..closingBracketIndex],
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out elementIndex);
            }

            elementName = pathSegment[..bracketIndex];
        }

        elementName = string.Equals(elementName, "channeling", StringComparison.OrdinalIgnoreCase)
            ? "channelingspell"
            : elementName;

        var directElements = parent.Elements()
            .Where(element => string.Equals(element.Name.LocalName, elementName, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (directElements.Length > elementIndex)
        {
            return directElements[elementIndex];
        }

        return parent.Descendants()
            .Where(element => string.Equals(element.Name.LocalName, elementName, StringComparison.OrdinalIgnoreCase))
            .ElementAtOrDefault(elementIndex);
    }

    private static string ResolveAttributeValue(XElement element, string attributeName)
    {
        var value = element.Attributes()
            .FirstOrDefault(attribute => string.Equals(attribute.Name.LocalName, attributeName, StringComparison.OrdinalIgnoreCase))?
            .Value;
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        if (string.Equals(attributeName, "totalduration", StringComparison.OrdinalIgnoreCase))
        {
            return ResolveTotalDuration(element);
        }

        if (string.Equals(attributeName, "totalchange", StringComparison.OrdinalIgnoreCase))
        {
            return ResolveTotalChange(element);
        }

        var dynamicAttributeMatch = DynamicAttributeRegex.Match(attributeName);
        if (!dynamicAttributeMatch.Success)
        {
            return MissingValue;
        }

        var sourceAttributeName = dynamicAttributeMatch.Groups["name"].Value;
        var sourceValue = element.Attributes()
            .FirstOrDefault(attribute => string.Equals(attribute.Name.LocalName, sourceAttributeName, StringComparison.OrdinalIgnoreCase))?
            .Value;
        return ResolveDynamicValue(sourceValue, dynamicAttributeMatch.Groups["selector"].Value);
    }

    private static string ResolveTotalDuration(XElement element)
    {
        if (string.Equals(element.Name.LocalName, "channelingspell", StringComparison.OrdinalIgnoreCase)
            && TryCalculateSequenceDuration(
                element,
                "effectcount",
                "initialeffectinterval",
                "effectinterval",
                out var channelingDuration))
        {
            return channelingDuration.ToString("0.##", CultureInfo.InvariantCulture);
        }

        if (string.Equals(element.Name.LocalName, "attributechangeovertime", StringComparison.OrdinalIgnoreCase)
            && TryCalculateSequenceDuration(element, "count", "initialinterval", "interval", out var effectDuration))
        {
            return effectDuration.ToString("0.##", CultureInfo.InvariantCulture);
        }

        return MissingValue;
    }

    private static string ResolveTotalChange(XElement element)
    {
        if (!TryGetNumber(element, "change", out var change))
        {
            return MissingValue;
        }

        var count = TryGetNumber(element, "count", out var parsedCount) ? parsedCount : 1;
        return (change * count).ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static string ResolveDynamicValue(string sourceValue, string selector)
    {
        if (string.IsNullOrWhiteSpace(sourceValue))
        {
            return MissingValue;
        }

        if (double.TryParse(sourceValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var singleValue))
        {
            return singleValue.ToString("0.##", CultureInfo.InvariantCulture);
        }

        var values = sourceValue
            .Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(value => double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number)
                ? number
                : double.NaN)
            .Where(number => !double.IsNaN(number))
            .ToArray();
        if (values.Length == 0)
        {
            return MissingValue;
        }

        var selectedValue = selector.ToLowerInvariant() switch
        {
            "end" => values[^1],
            "max" => values.Max(),
            "min" => values.Min(),
            _ => values[0]
        };
        return selectedValue.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static bool TryCalculateSequenceDuration(
        XElement element,
        string countAttributeName,
        string initialIntervalAttributeName,
        string intervalAttributeName,
        out double duration)
    {
        duration = 0;
        if (element == null
            || !TryGetNumber(element, countAttributeName, out var count)
            || !TryGetNumber(element, intervalAttributeName, out var interval))
        {
            return false;
        }

        var initialInterval = TryGetNumber(element, initialIntervalAttributeName, out var parsedInitialInterval)
            ? parsedInitialInterval
            : interval;
        duration = initialInterval + Math.Max(count - 1, 0) * interval;
        return true;
    }

    private static bool TryGetNumber(XElement element, string attributeName, out double value)
    {
        var rawValue = element?.Attributes()
            .FirstOrDefault(attribute => string.Equals(attribute.Name.LocalName, attributeName, StringComparison.OrdinalIgnoreCase))?
            .Value;
        return double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    private static string FormatValue(string value, string attributeName, XElement element)
    {
        if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
        {
            return string.IsNullOrWhiteSpace(value) ? MissingValue : value;
        }

        if (attributeName.EndsWith("change", StringComparison.OrdinalIgnoreCase))
        {
            number = Math.Abs(number);
        }

        if (IsPercentage(attributeName, element))
        {
            return (number * 100).ToString("0.##", CultureInfo.CurrentCulture) + "%";
        }

        var formattedNumber = number.ToString("0.##", CultureInfo.CurrentCulture);
        if (attributeName.Contains("time", StringComparison.OrdinalIgnoreCase)
            || attributeName.Contains("duration", StringComparison.OrdinalIgnoreCase)
            || attributeName.Contains("interval", StringComparison.OrdinalIgnoreCase)
            || attributeName.Contains("delay", StringComparison.OrdinalIgnoreCase))
        {
            return formattedNumber + "s";
        }

        return IsDistance(attributeName) ? formattedNumber + "m" : formattedNumber;
    }

    private static bool IsPercentage(string attributeName, XElement element)
    {
        if (attributeName.Contains("percent", StringComparison.OrdinalIgnoreCase)
            || attributeName.Contains("factor", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (attributeName.EndsWith("change", StringComparison.OrdinalIgnoreCase))
        {
            var changeType = element.Attribute("changetype")?.Value;
            return changeType?.StartsWith("relative", StringComparison.OrdinalIgnoreCase) == true;
        }

        return string.Equals(attributeName, "value", StringComparison.OrdinalIgnoreCase)
               && (element.Name.LocalName.Contains("buff", StringComparison.OrdinalIgnoreCase)
                   || element.Name.LocalName.Contains("debuff", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsDistance(string attributeName)
    {
        return attributeName.Contains("distance", StringComparison.OrdinalIgnoreCase)
               || attributeName.Contains("radius", StringComparison.OrdinalIgnoreCase)
               || attributeName.Contains("range", StringComparison.OrdinalIgnoreCase)
               || attributeName.Contains("width", StringComparison.OrdinalIgnoreCase)
               || attributeName.Contains("height", StringComparison.OrdinalIgnoreCase);
    }
}
