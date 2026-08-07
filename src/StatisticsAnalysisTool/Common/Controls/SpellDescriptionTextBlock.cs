using StatisticsAnalysisTool.Models.ItemDetailsModel;
using System.Collections.Generic;
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
            var resourceKey = GetForegroundResourceKey(segment.TypeKey);
            if (!string.IsNullOrWhiteSpace(resourceKey)
                && TryFindResource(resourceKey) is Brush foreground)
            {
                run.Foreground = foreground;
                run.FontWeight = FontWeights.Bold;
            }

            Inlines.Add(run);
        }
    }

    private static string GetForegroundResourceKey(string typeKey)
    {
        return typeKey switch
        {
            "damage" => "ItemDetails.Information.SpellType.Damage",
            "heal" => "ItemDetails.Information.SpellType.Heal",
            "crowdcontrol" => "ItemDetails.Information.SpellType.CrowdControl",
            "debuff" => "ItemDetails.Information.SpellType.Debuff",
            "buff" => "ItemDetails.Information.SpellType.Buff",
            "movement" => "ItemDetails.Information.SpellType.Mobility",
            "other" => "ItemDetails.Information.SpellType.Other",
            _ => string.Empty
        };
    }
}