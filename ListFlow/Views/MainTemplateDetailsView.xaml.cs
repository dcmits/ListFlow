using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using ListFlow.Models;


namespace ListFlow.Views
{
    /// <summary>
    /// Interaction logic for MainTemplateDetailsView.xaml
    /// </summary>
    public partial class MainTemplateDetailsView : Window
    {
        #region Properties

        public MainTemplate SelectedMainTemplate { get; set; }
        public List<string> SubTemplateTags { get; set; }

        #endregion

        #region Command Routing

        public static readonly RoutedCommand CloseWindowCommand = new RoutedCommand();

        #endregion

        #region Constructors

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
