using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using ListFlow.Models;
using System.Diagnostics;
using System.Collections.Generic;
using System;
using Microsoft.SqlServer.Management.SqlParser.Parser;

namespace ListFlow.Views
{
    /// <summary>
    /// Setup of the main template and the sub-templates.
    /// </summary>
    public partial class TemplateParametersView : Window, INotifyPropertyChanged
    {
        #region Fields

        private MainTemplate selectedMainTemplate;
        // True if the data has been modified by the user. 
        private bool dataUpdated;

        #endregion

        #region Constants

        private string MinSqlCode = "SELECT * FROM [{0}]";

        #endregion

        #region Command Routing

        public static readonly RoutedCommand QuerySaveCommand = new RoutedCommand();
        public static readonly RoutedCommand MainSaveCommand = new RoutedCommand();
        public static readonly RoutedCommand QueryUICommand = new RoutedCommand();
        public static readonly RoutedCommand OpenOrganFolder = new RoutedCommand();
        public static readonly RoutedCommand CloseWindowCommand = new RoutedCommand();

        #endregion

        #region Properties

        // Selected main template.
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

        // List of fields in the Excel file.
        public List<string> Fields
        {
            get => SelectedMainTemplate.ExcelData.SortedColumns;
        }

        #endregion

        #region Constructors

        
        /// <summary>
        /// Setup of the main template and the sub-templates.
        /// </summary>
        /// <param name="mainTemplate">Selected main template.</param>
        public TemplateParametersView(MainTemplate mainTemplate)
        {
            InitializeComponent();

            // Command Bindings.
            _ = CommandBindings.Add(new CommandBinding(MainSaveCommand, MainSaveCommand_Executed, MainSaveCommand_CanExecuted));
            _ = CommandBindings.Add(new CommandBinding(QuerySaveCommand, QuerySaveCommand_Executed, QuerySaveCommand_CanExecuted));
            _ = CommandBindings.Add(new CommandBinding(QueryUICommand, QueryUICommand_Executed, QueryUICommand_CanExecuted));
            _ = CommandBindings.Add(new CommandBinding(OpenOrganFolder, OpenOrganFolderCommand_Executed));
            _ = CommandBindings.Add(new CommandBinding(CloseWindowCommand, CloseWindowCommand_Executed));

            SelectedMainTemplate = mainTemplate;
            SelectedMainTemplate.IsParametersValueChanged = false;

            // Load the sub-templates.
            _ = SelectedMainTemplate.GetSubTemplates(false, SelectedMainTemplate.ExcelData.SheetName);

            DataContext = this;

            dataUpdated = false;
        }

        #endregion

        #region Commands Binding

        /// <summary>
        /// Opens the folder of the selected main model in the Windows file explorer.
        /// </summary>
        private void OpenOrganFolderCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Process explorer = new Process();
            explorer.StartInfo.FileName = "explorer.exe";
            explorer.StartInfo.Arguments = "/n, /e, /select," + SelectedMainTemplate.OrganFolder;
            _ = explorer.Start();
        }

        private void QuerySaveCommand_CanExecuted(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedMainTemplate.SelectedSubTemplate != null &&
                                SelectedMainTemplate.SelectedSubTemplate.IsQueryValueChanged &&
                                SelectedMainTemplate.SelectedSubTemplate.Query != null &&
                                !string.IsNullOrEmpty(SelectedMainTemplate.SelectedSubTemplate.Query.Trim());
        }

        /// <summary>
        /// Saves the SQL code in the selected sub-template after checking the syntax of the SQL code.
        /// </summary>
        private void QuerySaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Check the query syntax.
            ParseResult sqlParseResult = Parser.Parse(SelectedMainTemplate.SelectedSubTemplate.Query);
            if (sqlParseResult.Errors.Count() == 0)
            {
                if (SelectedMainTemplate.SelectedSubTemplate.Query.StartsWith(string.Format(MinSqlCode, SelectedMainTemplate.SelectedSubTemplate.SheetName), StringComparison.InvariantCultureIgnoreCase))
                {
                    try
                    {
                        // Save the SQL code in then selectec sub-template.
                        SelectedMainTemplate.SelectedSubTemplate.SaveQuery();
                        dataUpdated = true;
                    }
                    catch (Exception ex)
                    {
                        _ = Controls.MessageBoxUC.Show(null, Properties.Resources.Exception_MessageBox_TitleText, ex.Message, Controls.MessageBoxUC.MessageType.Error);
                    }
                }
                else
                {
                    _ = Controls.MessageBoxUC.Show(null, Properties.Resources.Exception_MessageBox_TitleText, 
                                                    $"{Properties.Resources.Exception_SqlNoMinimumClauses}\r\n\r\n{string.Format(MinSqlCode, SelectedMainTemplate.SelectedSubTemplate.SheetName)}",
                                                    Controls.MessageBoxUC.MessageType.Error);
                }
            }
            else
            {
                // Displays the list of errors present in the SQL code.
                SqlParserReportView dialog = new SqlParserReportView(sqlParseResult, Properties.Resources.SqlErrorsReport_UserMessage_SyntaxError, false)
                {
                    Left = Left + 50,
                    Top = Top + 50
                };

                _ = dialog.ShowDialog();
            }
        }

        private void QueryUICommand_CanExecuted(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedMainTemplate.SelectedSubTemplate != null;
        }

        /// <summary>
        /// Displays the data selection and sorting assistant for the creation/modification of the SQL query.
        /// </summary>
        private void QueryUICommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            bool? dialogResult = null;

            SortFilter sortFilter = new SortFilter();

            // Check the query syntax.
            ParseResult parseErrors = sortFilter.FlattenSQL(SelectedMainTemplate.SelectedSubTemplate.Query);

            if (parseErrors.Errors.Count() > 0)
            {
                // Displays the list of errors present in the SQL code.
                SqlParserReportView dialog = new SqlParserReportView(parseErrors, Properties.Resources.SqlErrorsReport_UserMessage_UI, true)
                {
                    Left = Left + 50,
                    Top = Top + 50
                };

                dialogResult  = dialog.ShowDialog();
            }

            if (dialogResult == null || dialogResult == true)
            {
                // Displays the data selection and sorting assistant screen.
                FilteringSortingView dialog = new FilteringSortingView(SelectedMainTemplate.ExcelData.SheetName, SelectedMainTemplate.ExcelData.ColumnDataTypes, sortFilter, SelectedMainTemplate.SelectedSubTemplate)
                {
                    Left = Left + 50,
                    Top = Top + 50
                };

                _ = dialog.ShowDialog();
            }
        }

        private void MainSaveCommand_CanExecuted(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedMainTemplate.IsParametersValueChanged && !string.IsNullOrEmpty(SelectedMainTemplate.Title.Trim());
        }

        /// <summary>
        /// Saves the parameters of the main template.
        /// </summary>
        private void MainSaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                SelectedMainTemplate.SaveParameters();
                dataUpdated = true;

            }
            catch (Exception ex)
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
