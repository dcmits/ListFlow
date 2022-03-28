using System.Windows.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Collections.ObjectModel;

namespace Update
{
    public class Action : INotifyPropertyChanged
    {
        private Status status;
        private string description;
        private SolidColorBrush actionInError;

        public enum Status
        {
            Comment,
            NotStarted,
            Started,
            Ended,
            Error
        }

        public string Description
        {
            get => description;
            set
            {
                if (description != value)
                {
                    description = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

        public DrawingImage Icon
        {
            get
            {
                DrawingImage drawingImage = null;

                switch (status)
                {
                    case Status.Started:
                        drawingImage = Application.Current.Resources["Process_Running_Large"] as DrawingImage;
                        break;
                    case Status.NotStarted:
                        drawingImage = Application.Current.Resources["Process_Large"] as DrawingImage;
                        break;
                    case Status.Ended:
                        drawingImage = Application.Current.Resources["Process_Ok_Large"] as DrawingImage;
                        break;
                    case Status.Error:
                        drawingImage = Application.Current.Resources["Process_Error_Large"] as DrawingImage;
                        break;
                    case Status.Comment:
                        break;
                    default:
                        break;
                }

                return drawingImage;
            }
            set
            {
                OnPropertyChanged(nameof(Icon));
            }
        }
        public SolidColorBrush ActionInError
        { 
            get => actionInError;
            set
            {
                if (value != actionInError)
                {
                    actionInError = value;
                    OnPropertyChanged(nameof(ActionInError));
                }
            }
        }

        public string ErrorDescription { get; set; }

        public Action(string description, Status status = Status.Error)
        {
            Description = description;
            this.status = status;
            ActionInError = this.status == Status.Error ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Green);
        }

        #region Properties Change (Events)

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion

    }
}
