using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace ListFlow.Views
{
    /// <summary>
    /// Update and help.
    /// </summary>
    public partial class HelpAndUpdateView : Window
    {
        #region Fields

        private string currentVersion;
        private string availableVersion;

        #endregion

        #region Properties

        // Version of the running application.
        public string CurrentVersion
        {
            get => currentVersion;
            set
            {
                if (currentVersion != value)
                {
                    currentVersion = value;

                    OnPropertyChanged(nameof(CurrentVersion));

                }
            }
        }

        // New application version available.
        public string AvailableVersion
        {
            get => availableVersion;
            set
            {
                if (availableVersion != value)
                {
                    availableVersion = value;
                    OnPropertyChanged(nameof(AvailableVersion));
                }
            }
        }

        #endregion

        #region Command Routing

        public static readonly RoutedCommand CloseWindowCommand = new RoutedCommand();

        #endregion

        #region Constructors

        /// <summary>
        /// Update and help.
        /// </summary>
        public HelpAndUpdateView()
        {
            InitializeComponent();

            _ = CommandBindings.Add(new CommandBinding(CloseWindowCommand, CloseWindowCommand_Executed));

            DataContext = this;

            CurrentVersion = GetType().Assembly.GetName().Version.ToString();
            AvailableVersion = CheckForNewVersion();
        }

        /// <summary>
        /// Check if a new version is available.
        /// </summary>
        /// <returns></returns>
        private string CheckForNewVersion()
        {
            return Properties.Resources.Update_NoNewVersionAvailable;
        }

        #endregion

        #region Commands Binding

        private void CloseWindowCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        #endregion

        #region Events

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Ajust Window Height to the content.
            Height -= 24d;

        }

        #region Properties Change (Events)

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion

        #endregion
    }
}
