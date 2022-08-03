using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using ListFlow.Controls;
using System.Windows.Documents;
using Microsoft.SqlServer.Management.SqlParser.Parser;
using System.Windows.Media;
using System;

namespace ListFlow.Views
{
    /// <summary>
    /// Interaction logic for SqlParserReportView.xaml
    /// </summary>
    public partial class SqlParserReportView : Window
    {
        #region Command Routing
        
        public static readonly RoutedCommand CloseWindowCommand = new RoutedCommand();
        public static readonly RoutedCommand CancelCommand = new RoutedCommand();
        public static readonly RoutedCommand ResetCommand = new RoutedCommand();

        #endregion

        #region Properties

        public ParseResult ParseErrors { get; set; }
        public FlowDocument FlowDocSql { get; set; }
        public string UserMessage { get; set; }

        #endregion

        #region Constructors

        public SqlParserReportView(ParseResult errors, string userMessage)
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

            DataContext = this;
        }

        #endregion

        #region Commands Binding

        private void ResetCommand_CanExecuted(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

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
