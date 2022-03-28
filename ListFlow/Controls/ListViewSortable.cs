using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace ListFlow.Controls
{
    /// <summary>
    /// ListView control with sort columns and sort direction indicator.
    /// </summary>
    public class ListViewSortable : ListView
    {
        #region Main control

        #region Fields

        private Point startPoint;
        private GridViewColumnHeader lastHeaderClicked = null;

        #endregion

        #region Properties

        public bool IsSelectionInMove { get; set; }

        #endregion

        #region Events

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            PreviewMouseDown += ListViewEx_PreviewMouseDown;
            PreviewMouseUp += ListViewEx_PreviewMouseUp;
        }

        private void ListViewEx_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                startPoint = e.GetPosition(null);
            }
        }

        private void ListViewEx_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsSelectionInMove)
            {
                IsSelectionInMove = false;
                if (IsEnabled == false)
                {
                    IsEnabled = true;
                }
            }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ListViewItemEx();
        }

        public void ListViewColumnHeaderClick(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;

            if (headerClicked == null)
            {
                return;
            }

            if (headerClicked.Role == GridViewColumnHeaderRole.Padding)
            {
                return;
            }

            if (headerClicked.Column.DisplayMemberBinding as Binding is null)
            {
                return;
            }

            string sortingColumn = (headerClicked.Column.DisplayMemberBinding as Binding).Path.Path;

            if (string.IsNullOrEmpty(sortingColumn))
            {
                return;
            }

            ListSortDirection direction = ApplySort(Items, sortingColumn);

            SetSortDirectionIndicator(this, headerClicked, lastHeaderClicked, direction);

            lastHeaderClicked = headerClicked;
        }

        #endregion

        #region Methods

        private static ListSortDirection ApplySort(ICollectionView view, string propertyName)
        {
            ListSortDirection direction = ListSortDirection.Ascending;
            if (view.SortDescriptions.Count > 0)
            {
                SortDescription currentSort = view.SortDescriptions[0];
                if (currentSort.PropertyName == propertyName)
                {
                    direction = currentSort.Direction == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                }
                view.SortDescriptions.Clear();
            }

            if (!string.IsNullOrEmpty(propertyName))
            {
                view.SortDescriptions.Add(new SortDescription(propertyName, direction));
            }

            return direction;
        }

        /// <summary>
        /// Displays the sort direction indicator next to the column heading using a DataTemplate.
        /// </summary>
        /// <param name="lstView">ListView control used.</param>
        /// <param name="headerClicked">Clicked column header.</param>
        /// <param name="previouslyColumnHeaderClicked">Previously clicked column header.</param>
        /// <param name="sortDirection">Sorting direction.</param>
        private void SetSortDirectionIndicator(ListView lstView, GridViewColumnHeader headerClicked, GridViewColumnHeader previouslyColumnHeaderClicked, ListSortDirection sortDirection)
        {
            if (previouslyColumnHeaderClicked != null)
            {
                // Set ColumnHeaderTemplate to NoSorting to the previous column.
                previouslyColumnHeaderClicked.Column.HeaderTemplate = lstView.TryFindResource("ListViewHeaderTemplateNoSorting") as DataTemplate;
            }

            switch (sortDirection)
            {
                case ListSortDirection.Ascending:
                    // Set ColumnHeaderTemplate to the clicked column.
                    headerClicked.Column.HeaderTemplate = lstView.TryFindResource("ListViewHeaderTemplateAscendingSorting") as DataTemplate;
                    break;
                case ListSortDirection.Descending:
                    // Set ColumnHeaderTemplate to the clicked column.
                    headerClicked.Column.HeaderTemplate = lstView.TryFindResource("ListViewHeaderTemplateDescendingSorting") as DataTemplate;
                    break;
                default:
                    // Set ColumnHeaderTemplate to the clicked column.
                    previouslyColumnHeaderClicked.Column.HeaderTemplate = lstView.TryFindResource("ListViewHeaderTemplateNoSorting") as DataTemplate;
                    break;
            }
        }

        #endregion

        #endregion

        #region Items

        class ListViewItemEx : ListViewItem
        {
            #region Fields

            private bool deferSelection = false;

            #endregion

            #region Events

            protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
            {
                if (e.ClickCount == 1 && IsSelected)
                {
                    // the user may start a drag by clicking into selected items
                    // delay destroying the selection to the Up event
                    deferSelection = true;
                }
                else
                {
                    base.OnMouseLeftButtonDown(e);
                }
            }

            protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
            {
                if (deferSelection)
                {
                    try
                    {
                        base.OnMouseLeftButtonDown(e);
                    }
                    finally
                    {
                        deferSelection = false;
                    }
                }
                base.OnMouseLeftButtonUp(e);
            }

            protected override void OnMouseLeave(MouseEventArgs e)
            {
                // abort deferred Down
                deferSelection = false;
                base.OnMouseLeave(e);
            }

            #endregion
        }

        #endregion
    }

}
