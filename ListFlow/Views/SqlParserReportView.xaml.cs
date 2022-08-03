using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Documents;
using Microsoft.SqlServer.Management.SqlParser.Parser;
using System.Windows.Media;

namespace ListFlow.Views
{
    /// <summary>
    /// Result of parsing of the SQL query.
    /// </summary>
    public partial class SqlParserReportView : Window
    {
        #region Command Routing
        
        public static readonly RoutedCommand CloseWindowCommand = new RoutedCommand();
        public static readonly RoutedCommand CancelCommand = new RoutedCommand();
        public static readonly RoutedCommand ResetCommand = new RoutedCommand();

        #endregion

        #region Properties

        // List of errors during the parsing process.
        public ParseResult ParseErrors { get; set; }
        // FlowDocument to display the text of the SQL query to point to the location of the error.
        public FlowDocument FlowDocSql { get; set; }
        // Explanatory message, in the header of the window, for the user's attention.
        public string UserMessage { get; set; }
        // True: validation of the SQL code before the wizard for creating sorting parameters and data selection. false: validation of the SQL code before saving the query.
        // Disable/enable buttons depending on the context.
        public bool SortFilterUI { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Result of parsing of the SQL query.
        /// </summary>
        /// <param name="errors">List of errors during SQL code parsing.</param>
        /// <param name="userMessage">Explanatory message, in the header of the window, for the user's attention.</param>
        /// <param name="sortFilterUI">Disable/enable buttons depending on the context.</param>
        public SqlParserReportView(ParseResult errors, string userMessage, bool sortFilterUI)
        {
            InitializeComponent();

            _ = CommandBindings.Add(new CommandBinding(CloseWindowCommand, CloseWindowCommand_Executed));
            _ = CommandBindings.Add(new CommandBinding(ResetCommand, ResetCommand_Executed, ResetCommand_CanExecuted));

            ParseErrors = errors;

            FlowDocSql = new FlowDocument
            {
                FontFamily = new FontFamily("Calibri"),
                Foreground = FindResource("TextBoxForeground") as SolidColorBrush,
                Background = FindResource("TextBoxBackground") as SolidColorBrush,                
                FontSize = 12,
                TextAlignment = TextAlignment.Left
            };

            UserMessage = userMessage;
            SortFilterUI = sortFilterUI;

            DataContext = this;
        }

        #endregion

        #region Commands Binding

        private void ResetCommand_CanExecuted(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        /// <summary>
        /// Displays the data sorting and selection screen.
        /// </summary>
        private void ResetCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DialogResult = true;

            SystemCommands.CloseWindow(this);
        }

        private void CloseWindowCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DialogResult = false;
        }

        #endregion

        #region Events

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Ajust Window Height to the content.
            Height -= 24d;

        }

        /// <summary>
        /// Highlights the location in the SQL code of the error selected in the list.
        /// </summary>
        private void ListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            System.Windows.Controls.ListBox lbx = (System.Windows.Controls.ListBox)sender;
            Error selectedError = (Error)(lbx.SelectedItem);

            FlowDocSql.Blocks.Clear();

            Paragraph p = new Paragraph
            {
                Margin = new Thickness(0, 10, 0, 0)
            };

            Run run;

            if (selectedError.Start.Offset == 1)
            {
                run = new Run(ParseErrors.Script.Sql.Substring(0, selectedError.Start.Offset))
                {
                    Background = FindResource("TextErrorBackgroundBrush") as SolidColorBrush,
                    Foreground = FindResource("TextForegroundBrush") as SolidColorBrush
                };
                p.Inlines.Add(run);

                if (selectedError.End.Offset < ParseErrors.Script.Sql.Length - 1)
                {
                    run = new Run(ParseErrors.Script.Sql.Substring(selectedError.End.Offset));
                    p.Inlines.Add(run);
                }
            }
            else
            {
                run = new Run(ParseErrors.Script.Sql.Substring(0, selectedError.Start.Offset));
                p.Inlines.Add(run);

                run = new Run(ParseErrors.Script.Sql.Substring(selectedError.Start.Offset, selectedError.End.Offset - selectedError.Start.Offset))
                {
                    Background = FindResource("TextErrorBackgroundBrush") as SolidColorBrush,
                    Foreground = FindResource("TextForegroundBrush") as SolidColorBrush
                };

                p.Inlines.Add(run);
                if (selectedError.End.Offset < ParseErrors.Script.Sql.Length - 1)
                {
                    run = new Run(ParseErrors.Script.Sql.Substring(selectedError.End.Offset));
                    p.Inlines.Add(run);
                }
            }

            FlowDocSql.Blocks.Add(p);
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
