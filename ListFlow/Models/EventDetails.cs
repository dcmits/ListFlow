using System.ComponentModel;

namespace ListFlow.Models
{
    /// <summary>
    /// Event details.
    /// </summary>
    public class EventDetails : INotifyPropertyChanged
    {
        #region Fields

        private string title;
        private string date;
        private string location;
        private bool fieldsFilledOut;
        private Usage fieldsUsage;

        #endregion

        #region Enums

        public enum Usage
        {
            Optional,
            Mandatory,
            Hidden
        }

        #endregion

        #region Properties

        // Event title.
        public string Title
        {
            get => title;
            set
            {
                if (title != value)
                {
                    title = value;
                    CheckOptionalFields();
                    OnPropertyChanged(nameof(Title));
                }
            }
        }
        // Event date (in text form).
        public string Date
        {
            get => date;
            set
            {
                if (date != value)
                {
                    date = value;
                    CheckOptionalFields();
                    OnPropertyChanged(nameof(Date));
                }
            }
        }
        // Event location.
        public string Location
        {
            get => location;
            set
            {
                if (location != value)
                {
                    location = value;
                    CheckOptionalFields();
                    OnPropertyChanged(nameof(Location));
                }
            }
        }

        // True if all fields are filled out.
        public bool OptionalFieldsFilledOut
        {
            get => fieldsFilledOut;
            set
            {
                if (fieldsFilledOut != value)
                {
                    fieldsFilledOut = value;
                    OnPropertyChanged(nameof(OptionalFieldsFilledOut));
                }
            }
        }

        #endregion

        #region Constructors

        public EventDetails(Usage eventDetailFields)
        {
            fieldsUsage = eventDetailFields;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Check if all Required fields are filled out.
        /// </summary>
        /// <returns>True if done, false in all other cases.</returns>
        private void CheckOptionalFields()
        {
            OptionalFieldsFilledOut = fieldsUsage == Usage.Mandatory && !string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(date) && !string.IsNullOrEmpty(location);
        }

        /// <summary>
        /// Reset (clean) all event fields.
        /// </summary>
        public void Reset()
        {
            Date = string.Empty;
            Location = string.Empty;
            Title = string.Empty;
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }
}
