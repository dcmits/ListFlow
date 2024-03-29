﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using ListFlow.Controls;

namespace ListFlow.Views
{
    /// <summary>
    /// Displays the list of column names in the file that are not unique.
    /// </summary>
    public partial class DuplicateColumnView : Window
    {
        #region Fields

        private string message;
        private Dictionary<string, string> duplicateColumns;

        #endregion

        #region Properties

        // Message to the user displayed in the header of the window.
        public string Message
        {
            get => message;
            set
            {
                if (message != value)
                {
                    message = value;

                    OnPropertyChanged(nameof(message));

                }
            }
        }

        // List of non-unique columns, key: name of the column in Excel format, value: title of the column.
        public Dictionary<string, string> DuplicateColumns
        {
            get => duplicateColumns;
            set
            {
                if (duplicateColumns != value)
                {
                    duplicateColumns = value;
                    OnPropertyChanged(nameof(duplicateColumns));
                }
            }
        }

        #endregion

        #region Command Routing

        public static readonly RoutedCommand CloseWindowCommand = new RoutedCommand();
        public static readonly RoutedCommand ExitCommand = new RoutedCommand();

        #endregion

        #region Constructors

        /// <summary>
        /// Displays the list of column names in the file that are not unique.
        /// </summary>
        /// <param name="duplicateColumns">List of non-unique columns, key: name of the column in Excel format, value: title of the column.</param>
        public DuplicateColumnView(Dictionary<string, string> duplicateColumns)
        {
            InitializeComponent();

            _ = CommandBindings.Add(new CommandBinding(CloseWindowCommand, CloseWindowCommand_Executed));
            _ = CommandBindings.Add(new CommandBinding(ExitCommand, ExitCommand_Executed, ExitCommand_CanExecuted));

            this.duplicateColumns = duplicateColumns;

            DataContext = this;

            Message = duplicateColumns.Count > 1 ? string.Format(Properties.Resources.DupColumns_Message, duplicateColumns.Count) : Properties.Resources.DupColumn_Message;
        }

        #endregion

        #region Commands Binding

        private void ExitCommand_CanExecuted(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ExitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

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

        #region SortableListView (Events)

        private void SortableListViewColumnHeaderClicked(object sender, RoutedEventArgs e)
        {
            ((ListViewSortable)sender).ListViewColumnHeaderClick(sender, e);
        }

        #endregion

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
