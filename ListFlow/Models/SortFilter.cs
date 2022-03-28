using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListFlow.Models
{
    public class SortFilter : INotifyPropertyChanged
    {
        #region Fields

        //private List<string> filterComparisons;
        //private List<string> filterLogics;
        //private List<string> filterFields;
        //private List<string> filterComparesTo;

        private ObservableCollection<string> sortFields;
        private ObservableCollection<bool> sortDirections;

        #endregion

        #region Properties

        public List<string> FilterComparisons { get; set; }
        public List<string> FilterLogics { get; set; }
        public List<string> FilterFields { get; set; }
        public List<string> FilterComparesTo { get; set; }

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

        public Dictionary<string, string> Filters { get; set; }
        public Dictionary<string, string> Comparisons { get; set; }

        #endregion

        #region Constructors

        public SortFilter()
        {
            FilterComparisons = new List<string>();
            FilterLogics = new List<string>();
            FilterFields = new List<string>();
            FilterComparesTo = new List<string>();

            SortFields = new ObservableCollection<string>();
            SortDirections = new ObservableCollection<bool>();

            for (int i = 0; i < 7; i++)
            {
                FilterComparisons.Add(string.Empty);
                FilterLogics.Add(string.Empty);
                FilterFields.Add(string.Empty);
                FilterComparesTo.Add(string.Empty);

                SortFields.Add(string.Empty);
                SortDirections.Add(true);
            }

            FillLists();
        }

        #endregion

        #region Methods

        public void ResetSort()
        {
            for (int i = 0; i < 7; i++)
            {
                SortFields[i] = string.Empty;
                SortDirections[i] = true;
            }
        }

            private void FillLists()
        {
            // Fill Sort criteria list.
            Filters = new Dictionary<string, string>
            {
                { "AND", Properties.Resources.Sort_Filter_And },
                { "OR", Properties.Resources.Sort_Filter_Or }
            };

            // Fill Comparisons criteria list.
            Comparisons = new Dictionary<string, string>
            {
                {"=", Properties.Resources.Sort_Comparison_Eq },
                {"<>",  Properties.Resources.Sort_Comparison_Neq },
                {"<", Properties.Resources.Sort_Comparison_Lt },
                {">", Properties.Resources.Sort_Comparison_Gt },
                {"<=", Properties.Resources.Sort_Comparison_Lte },
                {">=", Properties.Resources.Sort_Comparison_Gte },
                {"IS null", Properties.Resources.Sort_Comparison_Blk },
                {"IS NOT null", Properties.Resources.Sort_Comparison_Nblk },
                {"LIKE", Properties.Resources.Sort_Comparison_Contains },
                {"NOT LIKE", Properties.Resources.Sort_Comparison_NotContains }
            };
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
