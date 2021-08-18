using StatisticsAnalysisTool.Annotations;
using StatisticsAnalysisTool.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.ViewModels
{
    public class DialogWindowViewModel : INotifyPropertyChanged
    {
        private string _title;
        private string _message;
        private DialogWindowTranslation _dialogWindowTranslation = new();

        public DialogWindowViewModel(string title, string message)
        {
            Title = title;
            Message = message;
        }

        public bool Canceled { get; set; }

        #region Binding

        public string Title {
            get => _title;
            set {
                _title = value;
                OnPropertyChanged();
            }
        }

        public string Message {
            get => _message;
            set {
                _message = value;
                OnPropertyChanged();
            }
        }

        public DialogWindowTranslation Translation {
            get => _dialogWindowTranslation;
            set {
                _dialogWindowTranslation = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}