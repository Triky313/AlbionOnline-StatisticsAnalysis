using StatisticsAnalysisTool.Common.UserSettings;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;

namespace StatisticsAnalysisTool.Localization;

public class Culture
{
    static Culture()
    {
        FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
    }

    public static void SetCulture(CultureInfo cultureInfo)
    {
        if (cultureInfo == null)
        {
            return;
        }

        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;

        Application.Current.Dispatcher.Invoke(() =>
        {
            XmlLanguage lang = XmlLanguage.GetLanguage(cultureInfo.IetfLanguageTag);
            foreach (FrameworkElement fe in Application.Current.Windows)
            {
                fe.Language = lang;
            }
        });

        SettingsController.CurrentSettings.CurrentCultureIetfLanguageTag = cultureInfo.IetfLanguageTag;
    }

    public static CultureInfo GetCultureByIetfLanguageTag(string ietfLanguageTag)
    {
        if (!string.IsNullOrEmpty(ietfLanguageTag))
        {
            return new CultureInfo(ietfLanguageTag);
        }

        return null;
    }
}