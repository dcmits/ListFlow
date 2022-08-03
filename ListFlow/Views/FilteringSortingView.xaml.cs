using System.ComponentModel;
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

        private SubTemplate selectedSubTemplate;
        private readonly bool dataUpdated;
        private readonly string sheet;
        private readonly Dictionary<string, Type> fieldContentTypes;

        private SortFilter sortAndFilter;
        private readonly List<string> fields;

        #endregion

        #region Command Routing

        public static readonly RoutedCommand QuerySaveCommand = new RoutedCommand();
        public static readonly RoutedCommand QueryResetCommand = new RoutedCommand();
        public static readonly RoutedCommand MainSaveCommand = new RoutedCommand();
        public static readonly RoutedCommand QueryUICommand = new RoutedCommand();
        public static readonly RoutedCommand CloseWindowCommand = new RoutedCommand();

        #endregion

        #region Properties

        public SubTemplate SelectedSubTemplate
        {
            get => selectedSubTemplate;
            set
            {
                if (selectedSubTemplate != value)
                {
                    selectedSubTemplate = value;
                    OnPropertyChanged(nameof(SelectedSubTemplate));
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

        public FilteringSortingView(string sheetName, Dictionary<string, Type> columnDataTypes, SortFilter sortFilter, SubTemplate selectedSubTemplate)
        {
            InitializeComponent();

            // Sort and Format the fields list.
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

            SelectedSubTemplate = selectedSubTemplate;

            DataContext = this;

            // Reset Controls.
            Reset(true, sortFilter.FilterFields.Count());
            Reset(false, sortFilter.SortFields.Count());

            SortAndFilter = sortFilter;

            dataUpdated = false;
        }

        #endregion

        #region Commands Binding

        private void QuerySaveCommand_CanExecuted(object sender, CanExecuteRoutedEventArgs e)
        {
            //e.CanExecute = true;

            e.CanExecute = SortAndFilter.IsValueChanged;
        }

        private void QuerySaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Console.WriteLine(SortAndFilter.BuildSQL(sheet, fieldContentTypes));

            Console.WriteLine();

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
            if (tbc.SelectedIndex == 0)
            {
                Reset(true, SortAndFilter.FilterFields.Count());
                SortAndFilter.ResetFilter();
            }
            else
            {
                Reset(false, SortAndFilter.SortFields.Count());
                SortAndFilter.ResetSort();
            }
        }

        private void CloseWindowCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DialogResult = dataUpdated;
            SystemCommands.CloseWindow(this);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Reset Filters or Sorts.
        /// </summary>
        /// <param name="filters">True = reset filters, False = reset sorts.</param>
        /// <param name="itemCount">Number of items to reset.</param>
        private void Reset(bool filters, int itemCount)
        {
            if (filters)
            {
                for (int i = 1; i < itemCount; i++)
                {
                    (grdFilter.FindName($"cbxFilterLogic_{i}") as ComboBox).IsEnabled = false;
                    (grdFilter.FindName($"cbxFilterField_{i}") as ComboBox).IsEnabled = false;
                    (grdFilter.FindName($"cbxFilterComp_{i}") as ComboBox).IsEnabled = false;
                    (grdFilter.FindName($"tbxFilterValue_{i}") as TextBox).IsEnabled = false;
                }

                (grdFilter.FindName("cbxFilterComp_0") as ComboBox).IsEnabled = false;
                (grdFilter.FindName("tbxFilterValue_0") as TextBox).IsEnabled = false;
            }
            else
            {
                for (int i = 1; i < itemCount; i++)
                {
                    (grdSort.FindName($"rbnSortAsc_{i}") as RadioButton).IsChecked = true;
                    (grdSort.FindName($"rbnSortDesc_{i}") as RadioButton).IsChecked = false;
                    (grdSort.FindName($"rbnSortAsc_{i}") as RadioButton).IsEnabled = false;
                    (grdSort.FindName($"rbnSortDesc_{i}") as RadioButton).IsEnabled = false;
                    (grdSort.FindName($"cbxSortField_{i}") as ComboBox).IsEnabled = false;
                }
            }
        }

        #endregion

        #region Events

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Ajust Window Height to the content.
            Height -= 24d;

            SortAndFilter.IsValueChanged = false;
        }

        private void cbxFilterComp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbxComp = sender as ComboBox;
            TextBox tbxValue = grdFilter.FindName(cbxComp.Name.Replace("cbxFilterComp", "tbxFilterValue")) as TextBox;

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

                SortAndFilter.IsValueChanged = true;
            }
        }

                
        private void cbxFilterField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Current Field control.
            ComboBox cbxField = sender as ComboBox;
            // Index of the selected control.
            _ = int.TryParse(cbxField.Name.Replace("cbxFilterField_", string.Empty), out int index);
            // Comparaison control.
            ComboBox cbxComp = grdFilter.FindName($"cbxFilterComp_{index}") as ComboBox;
            // Criteria control.
            TextBox tbxValue = grdFilter.FindName($"tbxFilterValue_{index}") as TextBox;
            // Next Logic control.
            ComboBox cbxNextLogic = grdFilter.FindName($"cbxFilterLogic_{++index}") as ComboBox;
            // Next Field control.
            ComboBox cbxNextField = grdFilter.FindName($"cbxFilterField_{index}") as ComboBox;
           
            if (cbxField.SelectedIndex > 0)
            {
                // Add a new filter item or change a existing filter item.
                if (cbxNextField != null && cbxNextField.SelectedIndex == -1)
                {
                    if (!cbxComp.IsEnabled)
                    {
                        cbxComp.IsEnabled = true;
                        cbxComp.SelectedIndex = 0;

                        tbxValue.IsEnabled = true;
                        tbxValue.Text = string.Empty;
                    }

                    if (cbxNextLogic != null)
                    {
                        cbxNextLogic.IsEnabled = true;

                        // Select AND as default filter logic if next filter item is not defined.
                        if (index < sortAndFilter.FilterFields.Count - 1 && string.IsNullOrEmpty(sortAndFilter.FilterFields[index + 1]))
                        {
                            cbxNextLogic.SelectedIndex = 0;
                        }
                    }

                    if (cbxNextField != null)
                    {
                        cbxNextField.IsEnabled = true;
                        cbxNextField.SelectedIndex = -1;
                    }
                }
                else
                {
                    // Last filter item.
                    cbxComp.IsEnabled = cbxField.SelectedIndex > 0;
                }
            }
            else
            {
                // Remove the selected filter item.
                int noneIndex = sortAndFilter.FilterFields.IndexOf(fields[0]);

                if (noneIndex == -1)
                {
                    cbxComp.IsEnabled = false;
                    cbxComp.SelectedIndex = -1;
                    tbxValue.IsEnabled = false;
                    tbxValue.Text = string.Empty;

                    if (cbxNextLogic != null)
                    {
                        cbxNextLogic.IsEnabled = false;
                        cbxNextLogic.SelectedIndex = -1;
                    }

                    if (cbxNextField != null)
                    {
                        cbxNextField.IsEnabled = false;
                        cbxNextField.SelectedIndex = -1;
                    }
                }
                else
                {
                    // Moves up one position the criteria located after the disabled criterion (field = [none]).
                    int lastUsedIndex = sortAndFilter.FilterFields.IndexOf(string.Empty);

                    if (lastUsedIndex == -1)
                    {
                        lastUsedIndex = sortAndFilter.FilterFields.Count - 1;
                    }

                    if (lastUsedIndex - noneIndex == 1)
                    {
                        SortAndFilter.FilterFields[noneIndex] = string.Empty;
                    }
                    else
                    {
                        int nextIndex;

                        for (int i = noneIndex; i < lastUsedIndex; i++)
                        {
                            nextIndex = i + 1;
                            SortAndFilter.FilterLogics[i] = SortAndFilter.FilterLogics[nextIndex];
                            SortAndFilter.FilterFields[i] = SortAndFilter.FilterFields[nextIndex];
                            SortAndFilter.FilterComparisons[i] = SortAndFilter.FilterComparisons[nextIndex];
                            SortAndFilter.FilterComparesTo[i] = SortAndFilter.FilterComparesTo[nextIndex];
                        }

                        if (lastUsedIndex == sortAndFilter.FilterFields.Count - 1)
                        {
                            SortAndFilter.FilterLogics[lastUsedIndex] = SortAndFilter.Logics.First().Key;
                            SortAndFilter.FilterFields[lastUsedIndex] = string.Empty;
                        }
                    }
                }
            }

            SortAndFilter.IsValueChanged = true;
        }

        private void cbxSortField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Current Field control.
            ComboBox cbxField = sender as ComboBox;
            // Index of the selected control.
            _ = int.TryParse(cbxField.Name.Replace("cbxSortField_", string.Empty), out int index);            
            // Asc sort direction control.
            RadioButton rbnAsc = grdSort.FindName(cbxField.Name.Replace("cbxSortField", "rbnSortAsc")) as RadioButton;
            // Desc sort direction control.
            RadioButton rbnDesc = grdSort.FindName(cbxField.Name.Replace("cbxSortField", "rbnSortDesc")) as RadioButton;
            // Next Field control.
            ComboBox cbxNextField = grdSort.FindName($"cbxSortField_{++index}") as ComboBox;

            // Add a new sort item or change a existing sort item.
            if (cbxField.SelectedIndex > 0)
            {
                rbnAsc.IsEnabled = true;
                rbnDesc.IsEnabled = true;

                if (cbxNextField != null)
                {
                    cbxNextField.IsEnabled = true;

                    if (index < sortAndFilter.SortFields.Count - 1 && string.IsNullOrEmpty(sortAndFilter.SortFields[index + 1]))
                    {
                        cbxNextField.SelectedIndex = -1;
                    }
                }
            }
            else
            {
                // Remove the selected sort item.
                int noneIndex = sortAndFilter.SortFields.IndexOf(fields[0]);

                if (noneIndex != -1)
                {
                    // Moves up one position the criteria located after the disabled criterion (field = [none]).
                    int lastUsedIndex = sortAndFilter.SortFields.IndexOf(string.Empty);

                    if (lastUsedIndex == -1)
                    {
                        lastUsedIndex = sortAndFilter.SortFields.Count - 1;
                    }

                    if (lastUsedIndex - noneIndex == 1)
                    {
                        SortAndFilter.SortFields[noneIndex] = string.Empty;
                    }
                    else
                    {
                        int nextIndex;

                        for (int i = noneIndex; i < lastUsedIndex; i++)
                        {
                            nextIndex = i + 1;
                            SortAndFilter.SortDirections[i] = SortAndFilter.SortDirections[nextIndex];
                            SortAndFilter.SortFields[i] = SortAndFilter.SortFields[nextIndex];

                            (grdSort.FindName($"rbnSortAsc_{nextIndex}") as RadioButton).IsEnabled = false;
                            (grdSort.FindName($"rbnSortDesc_{nextIndex}") as RadioButton).IsEnabled = false;
                        }

                        if (lastUsedIndex == sortAndFilter.SortFields.Count - 1)
                        {
                            SortAndFilter.SortFields[lastUsedIndex] = string.Empty;
                            SortAndFilter.SortDirections[lastUsedIndex] = true;

                            (grdSort.FindName($"rbnSortAsc_{lastUsedIndex}") as RadioButton).IsEnabled = false;
                            (grdSort.FindName($"rbnSortDesc_{lastUsedIndex}") as RadioButton).IsEnabled = false;
                        }
                    }
                }
            }

            SortAndFilter.IsValueChanged = true;
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
