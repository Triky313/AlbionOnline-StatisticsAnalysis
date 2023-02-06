using System.Windows;
using System.Windows.Controls;
using StatisticsAnalysisTool.Models.BindingModel;

namespace StatisticsAnalysisTool.Gathering;

public class GatheringDataTemplateSelector : DataTemplateSelector
{
    public DataTemplate GenerallyTemplate { get; set; }
    public DataTemplate HideTemplate { get; set; }
    public DataTemplate OreTemplate { get; set; }
    public DataTemplate RockTemplate { get; set; }
    public DataTemplate FiberTemplate { get; set; }
    public DataTemplate WoodTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is not GatheringStats gatheringStats)
        {
            return base.SelectTemplate(item, container);
        }

        return gatheringStats.GatheringFilterType switch
        {
            GatheringFilterType.Generally => GenerallyTemplate,
            GatheringFilterType.Hide => HideTemplate,
            GatheringFilterType.Wood => WoodTemplate,
            GatheringFilterType.Fiber => FiberTemplate,
            GatheringFilterType.Ore => OreTemplate,
            GatheringFilterType.Rock => RockTemplate,
            _ => GenerallyTemplate
        };
    }
}