using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ListFlow.Models
{
    public class SortFilter : INotifyPropertyChanged
    {
        #region Fields

        private ObservableCollection<string> filterLogics;
        private ObservableCollection<string> filterFields;
        private ObservableCollection<string> filterComparisons;
        private ObservableCollection<string> filterComparesTo;
        private ObservableCollection<bool> filterHasValue;

        private ObservableCollection<string> sortFields;
        private ObservableCollection<bool> sortDirections;

        #endregion

        #region Properties

        public ObservableCollection<string> FilterComparisons
        {
            get => filterComparisons;
            set
            {
                if (filterComparisons != value)
                {
                    filterComparisons = value;
                    OnPropertyChanged(nameof(FilterComparisons));
                }
            }
        }
        public ObservableCollection<string> FilterLogics
        {
            get => filterLogics;
            set
            {
                if (filterLogics != value)
                {
                    filterLogics = value;
                    OnPropertyChanged(nameof(FilterLogics));
                }
            }
        }
        public ObservableCollection<string> FilterFields
        {
            get => filterFields;
            set
            {
                if (filterFields != value)
                {
                    filterFields = value;
                    OnPropertyChanged(nameof(FilterFields));
                }
            }
        }
        public ObservableCollection<string> FilterComparesTo
        {
            get => filterComparesTo;
            set
            {
                if (filterComparesTo != value)
                {
                    filterComparesTo = value;
                    OnPropertyChanged(nameof(FilterComparesTo));
                }
            }
        }
        public ObservableCollection<bool> FilterHasValue => filterHasValue;

        public ObservableCollection<string> SortFields
        {
            get => sortFields;
            set
            {
                if (sortFields != value)
                {
                    sortFields = value;
                    OnPropertyChanged(nameof(SortFields));
                }
            }
        }
        public ObservableCollection<bool> SortDirections
        {
            get => sortDirections;
            set
            {
                if (sortDirections != value)
                {
                    sortDirections = value;
                    OnPropertyChanged(nameof(SortDirections));
                }
            }
        }

        public Dictionary<string, string> Logics { get; set; }
        public Dictionary<string, string> Comparisons { get; set; }

        #endregion

        #region Constructors

        public SortFilter()
        {
            FilterComparisons = new ObservableCollection<string>(new string[8].ToList());
            FilterLogics = new ObservableCollection<string>(new string[8].ToList());
            FilterFields = new ObservableCollection<string>(new string[8].ToList());
            FilterComparesTo = new ObservableCollection<string>(new string[8].ToList());
            filterHasValue = new ObservableCollection<bool>(new bool[8].ToList());

            SortFields = new ObservableCollection<string>(new string[8].ToList());
            SortDirections = new ObservableCollection<bool>(new bool[8].ToList());

            ResetFilter();
            ResetSort();

            FillLists();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Reset user defined sorts.
        /// </summary>
        public void ResetSort()
        {
            for (int i = 0; i < SortFields.Count; i++)
            {
                SortFields[i] = string.Empty;
                SortDirections[i] = true;
            }
        }

        /// <summary>
        /// Reset user defined filters.
        /// </summary>
        public void ResetFilter()
        {
            for (int i = 0; i < FilterFields.Count; i++)
            {
                FilterLogics[i] = string.Empty;
                FilterFields[i] = string.Empty;
                FilterComparisons[i] = string.Empty;
                FilterComparesTo[i] = string.Empty;
                FilterHasValue[i] = false;
            }
        }

        /// <summary>
        /// Fill the sorts and filters comparaisons lists.
        /// </summary>
        private void FillLists()
        {
            // Fill Sort criteria list.
            Logics = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "AND", Properties.Resources.Filter_And },
                { "OR", Properties.Resources.Filter_Or }
            };

            // Fill Comparisons criteria list.
            Comparisons = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "=", Properties.Resources.Filter_Comparison_Eq },
                { "<>",  Properties.Resources.Filter_Comparison_Neq },
                { "<", Properties.Resources.Filter_Comparison_Lt },
                { ">", Properties.Resources.Filter_Comparison_Gt },
                { "<=", Properties.Resources.Filter_Comparison_Lte },
                { ">=", Properties.Resources.Filter_Comparison_Gte },
                { "IS NULL", Properties.Resources.Filter_Comparison_Blk },
                { "IS NOT NULL", Properties.Resources.Filter_Comparison_Nblk },
                { "LIKE", Properties.Resources.Filter_Comparison_Contains },
                { "NOT LIKE", Properties.Resources.Filter_Comparison_NotContains }
            };
        }

        /// <summary>
        /// Builds the SQL code according to the parameters defined by the user.
        /// </summary>
        /// <param name="sheetName">Name of the Sheet in Excel.</param>
        /// <param name="fieldContentTypes">List of fields with their data types.</param>
        /// <returns>SQL code.</returns>
        public string GetSQL(string sheetName, Dictionary<string, Type> fieldContentTypes)
        {
            StringBuilder sql = new StringBuilder($"SELECT * FROM `{sheetName}$` ");

            // Filters (WHERE SQL part).
            if (filterFields.Any(x => !string.IsNullOrEmpty(x)))
            {
                _ = sql.Append("WHERE ");

                for (int i = 0; i < filterFields.Count; i++)
                {
                    if (!string.IsNullOrEmpty(filterFields[i]))
                    {
                        switch (filterComparisons[i])
                        {
                            case "<>":
                            case "<":
                            case "<=":
                            case "=":
                            case ">":
                            case ">=":
                                if (fieldContentTypes[filterFields[i]] == typeof(double))
                                {
                                    _ = sql.Append($"{filterLogics[i]} `{filterFields[i]}`{filterComparisons[i]}{filterComparesTo[i].Trim()} ".TrimStart());
                                }
                                else
                                { 
                                    _ = sql.Append($"{filterLogics[i]} `{filterFields[i]}`{filterComparisons[i]}'{filterComparesTo[i].Trim()}' ".TrimStart());
                                }
                                break;
                            case "IS NULL":
                            case "IS NOT NULL":
                                _ = sql.Append($"{filterLogics[i]} `{filterFields[i]}` {filterComparisons[i]} ".TrimStart());
                                break;
                            case "LIKE":
                            case "NOT LIKE":
                                _ = sql.Append($"{filterLogics[i]} `{filterFields[i]}` {filterComparisons[i]} '%{filterComparesTo[i].Trim()}%' ".TrimStart());
                                break;
                        }
                    }
                }
            }

            // Sort (ORDER BY SQL part).
            if (sortFields.Any(x => !string.IsNullOrEmpty(x)))
            {
                _ = sql.Append("ORDER BY ");

                string sortDir;

                for (int i = 0; i < sortFields.Count; i++)
                {
                    if (!string.IsNullOrEmpty(sortFields[i]))
                    {
                        sortDir = sortDirections[i] ? "ASC" : "DESC";
                        _ = sql.Append($"`{sortFields[i]}` {sortDir},");
                    }
                }

                // Remove the last comma.
                _ = sql.Remove(sql.Length - 1, 1);
            }

            return sql.ToString().Trim();
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
