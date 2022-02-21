using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using StatisticsAnalysisTool.Annotations;

namespace StatisticsAnalysisTool.Models
{
    public class TaskTextObject : INotifyPropertyChanged
    {
        private string _statusIcon = "Solid_CircleNotch";
        private bool _statusIconSpin = true;
        private bool _isTaskDone;
        private string _text;

        public TaskTextObject(string text)
        {
            Text = text;
            CreateAt = DateTime.UtcNow;
        }

        public void SetStatus(bool done)
        {
            if (done)
            {
                StatusIcon = "Regular_CheckCircle";
                StatusIconSpin = false;
                IsTaskDone = true;
            }
            else
            {
                StatusIcon = "Solid_CircleNotch";
                StatusIconSpin = true;
                IsTaskDone = false;
            }
        }

        public bool IsTaskDone
        {
            get => _isTaskDone;
            set
            {
                _isTaskDone = value;
                OnPropertyChanged();
            }
        }

        public string StatusIcon
        {
            get => _statusIcon;
            set
            {
                _statusIcon = value;
                OnPropertyChanged();
            }
        }

        public bool StatusIconSpin
        {
            get => _statusIconSpin;
            set
            {
                _statusIconSpin = value;
                OnPropertyChanged();
            }
        }
        
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                OnPropertyChanged();
            }
        }
        
        public DateTime CreateAt { get; }
        
        protected bool Equals(TaskTextObject other)
        {
            return CreateAt.Equals(other.CreateAt) && Text == other.Text;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(CreateAt, Text);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    };
}