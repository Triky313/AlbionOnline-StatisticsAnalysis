using StatisticsAnalysisTool.Models.ItemDetailsModel;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace StatisticsAnalysisTool.Common.Controls;

public sealed class SpellDescriptionTextBlock : TextBlock
{
    public static readonly DependencyProperty SegmentsProperty = DependencyProperty.Register(
        nameof(Segments),
        typeof(IEnumerable<ItemSpellDescriptionSegment>),
        typeof(SpellDescriptionTextBlock),
        new PropertyMetadata(null, OnSegmentsChanged));

    public IEnumerable<ItemSpellDescriptionSegment> Segments
    {
        get => (IEnumerable<ItemSpellDescriptionSegment>) GetValue(SegmentsProperty);
        set => SetValue(SegmentsProperty, value);
    }

    private static void OnSegmentsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
    {
        if (dependencyObject is SpellDescriptionTextBlock textBlock)
        {
            textBlock.UpdateInlines();
        }
    }

    private void UpdateInlines()
    {
        Inlines.Clear();
        if (Segments == null)
        {
            return;
        }

        foreach (var segment in Segments)
        {
            var run = new Run(segment.Text);
            if (TryGetForeground(segment, out var foreground))
            {
                run.Foreground = foreground;
            }

            if (segment.IsBold)
            {
                run.FontWeight = FontWeights.Bold;
            }

            Inlines.Add(run);
        }
    }

    private bool TryGetForeground(ItemSpellDescriptionSegment segment, out Brush foreground)
    {
        if (TryCreateColorBrush(segment.ColorHex, out foreground))
        {
            return true;
        }

        var resourceKey = GetForegroundResourceKey(segment.TypeKey);
        foreground = !string.IsNullOrWhiteSpace(resourceKey)
            ? TryFindResource(resourceKey) as Brush
            : null;
        return foreground != null;
    }

    private static bool TryCreateColorBrush(string colorHex, out Brush brush)
    {
        brush = null;
        if (colorHex?.Length != 6
            || !byte.TryParse(colorHex[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var red)
            || !byte.TryParse(colorHex[2..4], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var green)
            || !byte.TryParse(colorHex[4..6], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var blue))
        {
            return false;
        }

        var solidColorBrush = new SolidColorBrush(Color.FromRgb(red, green, blue));
        solidColorBrush.Freeze();
        brush = solidColorBrush;
        return true;
    }


    private static string GetForegroundResourceKey(string typeKey)
    {
        return typeKey switch
        {
            "damage" => "Spell.Type.Damage",
            "heal" => "Spell.Type.Heal",
            "crowdcontrol" => "Spell.Type.CrowdControl",
            "debuff" => "Spell.Type.Debuff",
            "buff" => "Spell.Type.Buff",
            "movement" => "Spell.Type.Mobility",
            "other" => "Spell.Type.Other",
            _ => string.Empty
        };
    }
}