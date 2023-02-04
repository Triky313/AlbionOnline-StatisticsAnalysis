using StatisticsAnalysisTool.Models.BindingModel;
using System.Windows;
using System.Windows.Controls;

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

        switch (gatheringStats.GatheringFilterType)
        {
            case GatheringFilterType.Generally:
                return GenerallyTemplate;
            case GatheringFilterType.Hide:
                return HideTemplate;
            case GatheringFilterType.Wood:
                return WoodTemplate;
            case GatheringFilterType.Fiber:
                return FiberTemplate;
            case GatheringFilterType.Ore:
                return OreTemplate;
            case GatheringFilterType.Rock:
                return RockTemplate;
            default:
                return base.SelectTemplate(item, container);
        }
    }
}