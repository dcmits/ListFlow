﻿using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using ListFlow.Models;
using System.Diagnostics;
using System.Collections.Generic;
using System;

namespace ListFlow.Views
{
    /// <summary>
    /// Interaction logic for FilteringSortingView.xaml
    /// </summary>
    public partial class FilteringSortingView : Window, INotifyPropertyChanged
    {
        #region Fields

        private MainTemplate selectedMainTemplate;
        private bool dataUpdated;
        private string sheet;
        private Dictionary<string, Type> fieldContentTypes;

        //private List<string> filterFields;
        //private List<string> filterLogicals;
        //private List<string> filterComparaisons;

        //private List<string> filters;

        private SortFilter sortAndFilter;
        private List<string> fields;

        #endregion

        #region Command Routing

        public static readonly RoutedCommand QuerySaveCommand = new RoutedCommand();
        public static readonly RoutedCommand QueryResetCommand = new RoutedCommand();
        public static readonly RoutedCommand MainSaveCommand = new RoutedCommand();
        public static readonly RoutedCommand QueryUICommand = new RoutedCommand();
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

        public SortFilter SortAndFilter
        {
            get => sortAndFilter;
            set
            {
                if (sortAndFilter != value)
                {
                    sortAndFilter = value;
                    OnPropertyChanged(nameof(sortAndFilter));
                }
            }
        }

        public List<string> Fields
        {
            get => fields;
        }

        #endregion

        #region Constructors

        public FilteringSortingView(string sheetName, Dictionary<string, Type> columnDataTypes, SubTemplate subTemplate)
        {
            InitializeComponent();

            sortAndFilter = new SortFilter();
            SortAndFilter = sortAndFilter;

            // Format the fields list.
            fields = columnDataTypes.Keys.ToList();
            _ = fields.Remove("1");
            fields.Sort();
            fields.Insert(0, Properties.Resources.None);

            fieldContentTypes = columnDataTypes;
            sheet = sheetName;

            // Command Bindings.
            _ = CommandBindings.Add(new CommandBinding(QuerySaveCommand, QuerySaveCommand_Executed, QuerySaveCommand_CanExecuted));
            _ = CommandBindings.Add(new CommandBinding(QueryResetCommand, QueryResetCommand_Executed, QueryResetCommand_CanExecuted));
            _ = CommandBindings.Add(new CommandBinding(CloseWindowCommand, CloseWindowCommand_Executed));

            DataContext = this;

            // Reset Controls.
            Reset(true);
            Reset(false);

            dataUpdated = false;
        }

        #endregion

        #region Commands Binding

        private void QuerySaveCommand_CanExecuted(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            //e.CanExecute = SelectedMainTemplate.SelectedSubTemplate.IsQueryValueChanged && SelectedMainTemplate.SelectedSubTemplate.Query != null && !string.IsNullOrEmpty(SelectedMainTemplate.SelectedSubTemplate.Query.Trim());
        }

        private void QuerySaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            System.Console.WriteLine(SortAndFilter.GetSQL(sheet, fieldContentTypes));



            //try
            //{
            //    SelectedMainTemplate.SelectedSubTemplate.SaveQuery();
            //    dataUpdated = true;
            //}
            //catch (System.Exception ex)
            //{
            //    _ = Controls.MessageBoxUC.Show(null, Properties.Resources.Exception_MessageBox_TitleText, ex.Message, Controls.MessageBoxUC.MessageType.Error);
            //}
        }

        private void QueryResetCommand_CanExecuted(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void QueryResetCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Reset(tbc.SelectedIndex == 0);

            System.Console.WriteLine(SortAndFilter);
        }

        private void CloseWindowCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DialogResult = dataUpdated;
            SystemCommands.CloseWindow(this);
        }

        #endregion

        #region Methods

        private void Reset(bool filter)
        {
            if (filter)
            {
                for (int i = 1; i < sortAndFilter.FilterLogics.Count; i++)
                {
                    (grdFilter.FindName($"cbxFilterLogic_{i}") as ComboBox).IsEnabled = false;
                    (grdFilter.FindName($"cbxFilterField_{i}") as ComboBox).IsEnabled = false;
                    (grdFilter.FindName($"cbxFilterComp_{i}") as ComboBox).IsEnabled = false;
                    (grdFilter.FindName($"tbxFilterValue_{i}") as TextBox).IsEnabled = false;
                }

                (grdFilter.FindName("cbxFilterComp_0") as ComboBox).IsEnabled = false;
                (grdFilter.FindName("tbxFilterValue_0") as TextBox).IsEnabled = false;

                SortAndFilter.ResetFilter();
            }
            else
            {
                for (int i = 1; i < sortAndFilter.FilterLogics.Count; i++)
                {
                    (grdSort.FindName($"cbxFilterLogic_{i}") as ComboBox).IsEnabled = false;
                    (grdSort.FindName($"cbxFilterField_{i}") as ComboBox).IsEnabled = false;
                }

                SortAndFilter.ResetSort();
            }
        }

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

            //if (SelectedMainTemplate.SubTemplates.Count > 0)
            //{
            //    SelectedMainTemplate.SelectedSubTemplate = SelectedMainTemplate.SubTemplates.First();
            //}
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

        private void cbxFilterComp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbxComp = sender as ComboBox;
            TextBox tbxValue = ((cbxComp.Parent as StackPanel).Parent as Grid).FindName(cbxComp.Name.Replace("cbxFilterComp", "tbxFilterValue")) as TextBox;

            System.Console.WriteLine($"{cbxComp.Name}");

            if (cbxComp.SelectedItem != null)
            {
                string selectedValue = ((KeyValuePair<string, string>)cbxComp.SelectedItem).Value;
                if (selectedValue.CompareTo(Properties.Resources.Filter_Comparison_Nblk) == 0 | selectedValue.CompareTo(Properties.Resources.Filter_Comparison_Blk) == 0)
                {
                    tbxValue.Text = string.Empty;
                    tbxValue.IsEnabled = false;
                }
                else
                {
                    tbxValue.IsEnabled = true;
                }
            }

            System.Console.WriteLine(sender.ToString());
        }

        private void cbxFilterField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbxField = sender as ComboBox;
            _ = int.TryParse(cbxField.Name.Replace("cbxFilterField_", string.Empty), out int index);
            ComboBox cbxComp = ((cbxField.Parent as StackPanel).Parent as Grid).FindName(cbxField.Name.Replace("cbxFilterField", "cbxFilterComp")) as ComboBox;
            TextBox tbxValue = ((cbxField.Parent as StackPanel).Parent as Grid).FindName(cbxField.Name.Replace("cbxFilterField", "tbxFilterValue")) as TextBox;
            ComboBox cbxLogic = ((cbxField.Parent as StackPanel).Parent as Grid).FindName($"cbxFilterLogic_{++index}") as ComboBox;
            ComboBox cbxNextField = ((cbxField.Parent as StackPanel).Parent as Grid).FindName($"cbxFilterField_{index}") as ComboBox;

            if (cbxField.SelectedIndex > 0)
            {
                cbxComp.IsEnabled = true;
                cbxComp.SelectedIndex = 0;
                tbxValue.IsEnabled = true;
                tbxValue.Text = string.Empty;
                cbxLogic.IsEnabled = true;
                cbxLogic.SelectedIndex = 0;
                cbxNextField.IsEnabled = true;
                cbxNextField.SelectedIndex = -1;
            }
            else
            {
                cbxComp.IsEnabled = false;
                cbxComp.SelectedIndex = -1;
                tbxValue.IsEnabled = false;
                tbxValue.Text = string.Empty;
                cbxLogic.IsEnabled = false;
                cbxLogic.SelectedIndex = -1;
                cbxNextField.IsEnabled = false;
                cbxNextField.SelectedIndex = -1;
            }



            System.Console.WriteLine(sender.ToString());
        }

        private void cbxSortField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Console.WriteLine(sender.ToString());
        }
    }
}