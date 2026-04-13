using StatisticsAnalysisTool.ViewModels;
using System.Windows;

namespace StatisticsAnalysisTool.Models.BindingModel;

public class UserTrackingBindings : BaseViewModel
{
    public string Username
    {
        get;
        set
        {
            field = value;
            UsernameInformationVisibility = !string.IsNullOrEmpty(field) ? Visibility.Visible : Visibility.Collapsed;
            OnPropertyChanged();
        }
    }

    public string GuildName
    {
        get;
        set
        {
            field = value;
            GuildInformationVisibility = !string.IsNullOrEmpty(field) ? Visibility.Visible : Visibility.Collapsed;
            OnPropertyChanged();
        }
    }

    public string AllianceName
    {
        get;
        set
        {
            field = value;
            AllianceInformationVisibility = !string.IsNullOrEmpty(field) ? Visibility.Visible : Visibility.Collapsed;
            OnPropertyChanged();
        }
    }

    public CurrentMapInfoBinding CurrentMapInfoBinding
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public Visibility UsernameInformationVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public Visibility GuildInformationVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public Visibility AllianceInformationVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }
}