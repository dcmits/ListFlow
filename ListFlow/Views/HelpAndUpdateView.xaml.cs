using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ListFlow.Views
{
    /// <summary>
    /// Interaction logic for HelpAndUpdateView.xaml
    /// </summary>
    public partial class HelpAndUpdateView : Window
    {
        #region Fields

        private string currentVersion;
        private string availableVersion;

        #endregion

        #region Properties

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

        public HelpAndUpdateView()
        {
            InitializeComponent();

            _ = CommandBindings.Add(new CommandBinding(CloseWindowCommand, CloseWindowCommand_Executed));

            DataContext = this;

            CurrentVersion = GetType().Assembly.GetName().Version.ToString();
            AvailableVersion = CheckForNewVersion();
        }

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
