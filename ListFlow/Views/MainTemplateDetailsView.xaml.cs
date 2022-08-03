using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using ListFlow.Models;


namespace ListFlow.Views
{
    /// <summary>
    /// Information on the main template.
    /// </summary>
    public partial class MainTemplateDetailsView : Window
    {
        #region Properties

        // Selected main template.
        public MainTemplate SelectedMainTemplate { get; set; }
        // List of sub-templates of the selected main template.
        public List<string> SubTemplateTags { get; set; }

        #endregion

        #region Command Routing

        public static readonly RoutedCommand CloseWindowCommand = new RoutedCommand();

        #endregion

        #region Constructors

        /// <summary>
        /// Information on the main template.
        /// </summary>
        /// <param name="selectedMainTemplate">Selected main template.</param>
        public MainTemplateDetailsView(MainTemplate selectedMainTemplate)
        {
            InitializeComponent();

            SelectedMainTemplate = selectedMainTemplate;
            SubTemplateTags = selectedMainTemplate.GetSubTemplateTags(selectedMainTemplate.FullPath);

            _ = CommandBindings.Add(new CommandBinding(CloseWindowCommand, CloseWindowCommand_Executed));

            DataContext = this;
        }

        #endregion

        #region Commands Binding

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

        #endregion
    }
}
