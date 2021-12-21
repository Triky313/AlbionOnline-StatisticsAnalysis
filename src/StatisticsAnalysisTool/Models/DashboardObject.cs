using StatisticsAnalysisTool.Annotations;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.Models;

public class DashboardObject : INotifyPropertyChanged
{
    private double _famePerHour;
    private double _reSpecPointsPerHour;
    private double _silverPerHour;
    private double _mightPerHour;
    private double _favorPerHour;
    private double _totalGainedFameInSessionInSession;
    private double _totalGainedReSpecPointsInSessionInSession;
    private double _totalGainedSilverInSessionInSession;
    private double _totalGainedMightInSession;
    private double _totalGainedFavorInSession;
    private double _highestValue;
    private double _fameInPercent;
    private double _silverInPercent;
    private double _reSpecPointsInPercent;
    private double _mightInPercent;
    private double _favorInPercent;
    private string _fameString;
    private string _silverString;
    private string _reSpecPointsString;
    private string _mightString;
    private string _favorString;

    public double GetHighestValue()
    {
        var values = new List<double>()
        {
            TotalGainedFameInSession,
            TotalGainedSilverInSession,
            TotalGainedReSpecPointsInSession,
            TotalGainedMightInSession,
            TotalGainedFavorInSession
        };
        
        return values.Max<double>();
    }

    public void Reset()
    {
        HighestValue = 0;

        FamePerHour = 0;
        SilverPerHour = 0;
        ReSpecPointsPerHour = 0;
        MightPerHour = 0;
        FavorPerHour = 0;

        TotalGainedFameInSession = 0;
        TotalGainedSilverInSession = 0;
        TotalGainedReSpecPointsInSession = 0;
        TotalGainedMightInSession = 0;
        TotalGainedFavorInSession = 0;
    }

    public double HighestValue
    {
        get => _highestValue;
        set
        {
            _highestValue = value;
            OnPropertyChanged();
        }
    }
    
    public double FamePerHour
    {
        get => _famePerHour;
        set
        {
            _famePerHour = value;
            OnPropertyChanged();
        }
    }

    public double SilverPerHour
    {
        get => _silverPerHour;
        set
        {
            _silverPerHour = value;
            OnPropertyChanged();
        }
    }

    public double ReSpecPointsPerHour
    {
        get => _reSpecPointsPerHour;
        set
        {
            _reSpecPointsPerHour = value;
            OnPropertyChanged();
        }
    }

    public double MightPerHour
    {
        get => _mightPerHour;
        set
        {
            _mightPerHour = value;
            OnPropertyChanged();
        }
    }

    public double FavorPerHour
    {
        get => _favorPerHour;
        set
        {
            _favorPerHour = value;
            OnPropertyChanged();
        }
    }

    #region Percent values


    public double FameInPercent
    {
        get => _fameInPercent;
        set
        {
            _fameInPercent = value;
            OnPropertyChanged();
        }
    }

    public double SilverInPercent
    {
        get => _silverInPercent;
        set
        {
            _silverInPercent = value;
            OnPropertyChanged();
        }
    }

    public double ReSpecPointsInPercent
    {
        get => _reSpecPointsInPercent;
        set
        {
            _reSpecPointsInPercent = value;
            OnPropertyChanged();
        }
    }

    public double MightInPercent
    {
        get => _mightInPercent;
        set
        {
            _mightInPercent = value;
            OnPropertyChanged();
        }
    }

    public double FavorInPercent
    {
        get => _favorInPercent;
        set
        {
            _favorInPercent = value;
            OnPropertyChanged();
        }
    }

    #endregion

    public double TotalGainedFameInSession
    {
        get => _totalGainedFameInSessionInSession;
        set
        {
            _totalGainedFameInSessionInSession = value;
            HighestValue = GetHighestValue();
            FameInPercent = _totalGainedFameInSessionInSession / HighestValue * 100;
            OnPropertyChanged();
        }
    }

    public double TotalGainedSilverInSession
    {
        get => _totalGainedSilverInSessionInSession;
        set
        {
            _totalGainedSilverInSessionInSession = value;
            HighestValue = GetHighestValue();
            SilverInPercent = _totalGainedSilverInSessionInSession / HighestValue * 100;
            OnPropertyChanged();
        }
    }

    public double TotalGainedReSpecPointsInSession
    {
        get => _totalGainedReSpecPointsInSessionInSession;
        set
        {
            _totalGainedReSpecPointsInSessionInSession = value;
            HighestValue = GetHighestValue();
            ReSpecPointsInPercent = _totalGainedReSpecPointsInSessionInSession / HighestValue * 100;
            OnPropertyChanged();
        }
    }

    public double TotalGainedMightInSession
    {
        get => _totalGainedMightInSession;
        set
        {
            _totalGainedMightInSession = value;
            HighestValue = GetHighestValue();
            MightInPercent = _totalGainedMightInSession / HighestValue * 100;
            OnPropertyChanged();
        }
    }

    public double TotalGainedFavorInSession
    {
        get => _totalGainedFavorInSession;
        set
        {
            _totalGainedFavorInSession = value;
            HighestValue = GetHighestValue();
            FavorInPercent = _totalGainedFavorInSession / HighestValue * 100;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}