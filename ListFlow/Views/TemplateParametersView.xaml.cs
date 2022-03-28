using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using ListFlow.Models;
using System.Diagnostics;
using System.Collections.Generic;

namespace ListFlow.Views
{
    /// <summary>
    /// Interaction logic for TemplateParametersView.xaml
    /// </summary>
    public partial class TemplateParametersView : Window, INotifyPropertyChanged
    {
        #region Fields

        private MainTemplate selectedMainTemplate;
        private bool dataUpdated;
        private List<string> fields;

        #endregion

        #region Command Routing

        public static readonly RoutedCommand QuerySaveCommand = new RoutedCommand();
        public static readonly RoutedCommand MainSaveCommand = new RoutedCommand();
        public static readonly RoutedCommand OpenOrganFolder = new RoutedCommand();
        public static readonly RoutedCommand CloseWindowCommand = new RoutedCommand();

        #endregion

        #region Properties

        public MainTemplate SelectedMainTemplate
        {
            get => selectedMainTemplate;
            set
            {
                if (selectedMainTemplate != value)
                {
                    selectedMainTemplate = value;
                    OnPropertyChanged(nameof(SelectedMainTemplate));
                }
            }
        }

        public List<string> Fields
        {
            get => fields;
            set
            {
                if (fields != value)
                {
                    fields = value;
                    OnPropertyChanged(nameof(Fields));
                }
            }
        }

        #endregion

        #region Constructors

        public TemplateParametersView(MainTemplate mainTemplate)
        {
            InitializeComponent();

            // Command Bindings.
            _ = CommandBindings.Add(new CommandBinding(MainSaveCommand, MainSaveCommand_Executed, MainSaveCommand_CanExecuted));
            _ = CommandBindings.Add(new CommandBinding(QuerySaveCommand, QuerySaveCommand_Executed, QuerySaveCommand_CanExecuted));
            _ = CommandBindings.Add(new CommandBinding(OpenOrganFolder, OpenOrganFolderCommand_Executed));
            _ = CommandBindings.Add(new CommandBinding(CloseWindowCommand, CloseWindowCommand_Executed));

            SelectedMainTemplate = mainTemplate;
            SelectedMainTemplate.IsParametersValueChanged = false;

            // Load the sub-templates.
            _ = SelectedMainTemplate.GetSubTemplates(false);

            // Format the fields list.
            Fields = mainTemplate.ExcelData.ColumnFieldNames.Keys.ToList();
            _ = Fields.Remove("1");
            Fields.Sort();
            Fields.Insert(0, Properties.Resources.None);

            DataContext = this;

            dataUpdated = false;
        }

        #endregion

        #region Commands Binding

        private void OpenOrganFolderCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Process explorer = new Process();
            explorer.StartInfo.FileName = "explorer.exe";
            explorer.StartInfo.Arguments = "/n, /e, /select," + SelectedMainTemplate.OrganFolder;
            _ = explorer.Start();
        }

        private void QuerySaveCommand_CanExecuted(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedMainTemplate.SelectedSubTemplate.IsQueryValueChanged && SelectedMainTemplate.SelectedSubTemplate.Query != null && !string.IsNullOrEmpty(SelectedMainTemplate.SelectedSubTemplate.Query.Trim());
        }

        private void QuerySaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                SelectedMainTemplate.SelectedSubTemplate.SaveQuery();
                dataUpdated = true;
            }
            catch (System.Exception ex)
            {
                _ = Controls.MessageBoxUC.Show(null, Properties.Resources.Exception_MessageBox_TitleText, ex.Message, Controls.MessageBoxUC.MessageType.Error);
            }
        }

        private void MainSaveCommand_CanExecuted(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedMainTemplate.IsParametersValueChanged && !string.IsNullOrEmpty(SelectedMainTemplate.Title.Trim());
        }

        private void MainSaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                SelectedMainTemplate.SaveParameters();
                dataUpdated = true;

            }
            catch (System.Exception ex)
            {
                _ = Controls.MessageBoxUC.Show(null, Properties.Resources.Exception_MessageBox_TitleText, ex.Message, Controls.MessageBoxUC.MessageType.Error);
            }
        }

        private void CloseWindowCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DialogResult = dataUpdated;
            SystemCommands.CloseWindow(this);
        }

        #endregion

        #region Methods

        #endregion

        #region Events

        private void SubTemplate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedMainTemplate.SelectedSubTemplate.IsQueryValueChanged = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Ajust Window Height to the content.
            Height -= 24d;

            if (SelectedMainTemplate.SubTemplates.Count > 0)
            {
                SelectedMainTemplate.SelectedSubTemplate = SelectedMainTemplate.SubTemplates.First();
            }
        }

        /// <summary>
        /// Scroll to the selected item to be sure is visible.
        /// </summary>
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((ListBox)sender).ScrollIntoView(e.AddedItems[0]);
        }

        #region Properties Change (Events)

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #endregion
    }
}
