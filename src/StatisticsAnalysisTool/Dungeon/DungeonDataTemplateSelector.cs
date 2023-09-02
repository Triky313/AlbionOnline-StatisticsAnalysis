using StatisticsAnalysisTool.Dungeon.Models;
using System.Windows;
using System.Windows.Controls;

namespace StatisticsAnalysisTool.Dungeon;

public class DungeonDataTemplateSelector : DataTemplateSelector
{
    public DataTemplate RandomDungeonTemplate { get; set; }
    public DataTemplate HellGateTemplate { get; set; }
    public DataTemplate CorruptedTemplate { get; set; }
    public DataTemplate ExpeditionTemplate { get; set; }
    public DataTemplate MistsTemplate { get; set; }
    public DataTemplate MistsDungeonTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is not DungeonBaseFragment dungeon)
        {
            return base.SelectTemplate(item, container);
        }

        return dungeon switch
        {
            RandomDungeonFragment => RandomDungeonTemplate,
            HellGateFragment => HellGateTemplate,
            CorruptedFragment => CorruptedTemplate,
            ExpeditionFragment => ExpeditionTemplate,
            MistsFragment => MistsTemplate,
            MistsDungeonFragment => MistsDungeonTemplate,
            _ => base.SelectTemplate(dungeon, container)
        };
    }
}