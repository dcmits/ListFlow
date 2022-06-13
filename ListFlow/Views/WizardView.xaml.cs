using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ListFlow.ViewModels;
using System.Threading.Tasks;
using System.Collections.Generic;
using ListFlow.Helpers;

namespace ListFlow.Views
{
    /// <summary>
    /// Main Screen.
    /// </summary>
    public partial class WizardView : Window
    {
        #region Fields

        private readonly MainTemplatesViewModel mainTemplatesViewModel;
        // Logging of disabled sub-models due to an error or otherwise when creating the list of sub-models. 
        private List<string> subTemplateLog;

        #endregion 

        #region Properties

        #endregion

        #region Command Routing

        public static readonly RoutedCommand SetupCommand = new RoutedCommand();
        public static readonly RoutedCommand NextStepCommand = new RoutedCommand();
        public static readonly RoutedCommand PrevStepCommand = new RoutedCommand();
        public static readonly RoutedCommand CancelCommand = new RoutedCommand();
        public static readonly RoutedCommand SelectFileCommand = new RoutedCommand();
        public static readonly RoutedCommand CloseWindowCommand = new RoutedCommand();
        public static readonly RoutedCommand ResetCommand = new RoutedCommand();
        public static readonly RoutedCommand ProcessReportCommand = new RoutedCommand();
        public static readonly RoutedCommand SortFilterCommand = new RoutedCommand();
        public static readonly RoutedCommand HelpCommand = new RoutedCommand();

        #endregion

        #region Constructors

        public WizardView()
        {
            InitializeComponent();

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                mainTemplatesViewModel = new MainTemplatesViewModel();
                DataContext = mainTemplatesViewModel;

                elpStep1.IsEnabled = true;
                elpStep1.Fill = (Brush)FindResource("CurrentStep");

                // View only the first step.
                gbxStep3.Visibility = Visibility.Collapsed;
                gbxStep2.Visibility = Visibility.Collapsed;
                gbxStep1.Visibility = Visibility.Visible;


                mainTemplatesViewModel.IsNotLastStep = true;
                mainTemplatesViewModel.IsMergeNotInProgress = true;

                // Command Bindings.
                _ = CommandBindings.Add(new CommandBinding(NextStepCommand, NextStepCommand_Executed, NextStepCommand_CanExecuted));
                _ = CommandBindings.Add(new CommandBinding(PrevStepCommand, PrevStepCommand_Executed, PrevStepCommand_CanExecuted));
                _ = CommandBindings.Add(new CommandBinding(CancelCommand, CancelCommand_Executed));
                _ = CommandBindings.Add(new CommandBinding(SelectFileCommand, SelectFileCommand_Executed, SelectFileCommand_CanExecuted));
                _ = CommandBindings.Add(new CommandBinding(CloseWindowCommand, CloseWindowCommand_Executed));
                _ = CommandBindings.Add(new CommandBinding(ResetCommand, ResetCommand_Executed, ResetCommand_CanExecuted));
                _ = CommandBindings.Add(new CommandBinding(ProcessReportCommand, ProcessReportCommand_Executed, ProcessReportCommand_CanExecuted));
                _ = CommandBindings.Add(new CommandBinding(SortFilterCommand, TemplateParametersCommand_Executed, TemplateParametersCommand_CanExecuted));
                _ = CommandBindings.Add(new CommandBinding(HelpCommand, HelpCommand_Executed, HelpCommand_CanExecuted));

                Mouse.OverrideCursor = null;
            }
            catch (Exception ex)
            {
                Mouse.OverrideCursor = null;

                _ = new CustomException(ex.Message, Properties.Resources.Exception_MessageBox_TitleText);
                //_ = Controls.MessageBoxUC.Show(null, Properties.Resources.Exception_MessageBox_TitleText, ex.Message, Controls.MessageBoxUC.MessageType.Error);

                Close();
            }
        }

        private void WizardView_Loaded(object sender, RoutedEventArgs e)
        {
            // Ajust Window Height to the content.
            Height = grdMain.ActualHeight + 18d;

            Mouse.OverrideCursor = null;
        }

        #endregion

        #region Commands Binding

        /// <summary>
        /// Select the Excel File.
        /// </summary>
        private void SelectFileCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;

            OpenFileDialog fileDialog = new OpenFileDialog
            {
                Filter = Properties.Resources.ExcelFilesFilter,
                FilterIndex = 4,
                Title = Properties.Resources.Data_StepTitle,
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false
            };

            // Select the previous InitialDirectory, if exist.
            fileDialog.InitialDirectory = !string.IsNullOrEmpty(mainTemplatesViewModel.SelectedMainTemplate.ExcelData.LastInitialDirectory)
                ? Directory.Exists(mainTemplatesViewModel.SelectedMainTemplate.ExcelData.LastInitialDirectory) ? mainTemplatesViewModel.SelectedMainTemplate.ExcelData.LastInitialDirectory : Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                : Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            Mouse.OverrideCursor = null;

            bool? result = fileDialog.ShowDialog(this);

            Mouse.OverrideCursor = Cursors.Wait;

            if (result.HasValue && result.Value)
            {
                mainTemplatesViewModel.SelectedMainTemplate.ExcelData.FilePath = fileDialog.FileName;
                // Save the used InitialDirectory.
                mainTemplatesViewModel.SelectedMainTemplate.ExcelData.LastInitialDirectory = Path.GetDirectoryName(fileDialog.FileName);
            }

            Mouse.OverrideCursor = null;
        }

        private void SelectFileCommand_CanExecuted(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private static readonly Action EmptyDelegate = delegate () { };

        private async void NextStepCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            switch (mainTemplatesViewModel.CurrentStep)
            {
                case 1:
                    // Select the Word Template.
                    Mouse.OverrideCursor = Cursors.Wait;

                    gbxStep1.Visibility = Visibility.Collapsed;
                    imgDoneStep1.IsEnabled = true;
                    elpStep1.Fill = (GradientBrush)FindResource("FinishedStep");

                    gbxStep2.Visibility = Visibility.Visible;
                    elpStep2.IsEnabled = true;
                    elpStep2.Fill = (GradientBrush)FindResource("CurrentStep");

                    Mouse.OverrideCursor = null;

                    mainTemplatesViewModel.CurrentStep++;
                    mainTemplatesViewModel.UpdateStep();
                    break;
                case 2:
                    // Select the Excel File.
                    Mouse.OverrideCursor = Cursors.Wait;

                    imgDoneStep2.IsEnabled = true;
                    elpStep2.Fill = (GradientBrush)FindResource("FinishedStep");

                    if (mainTemplatesViewModel.SelectedMainTemplate.RenameColumns)
                    {
                        lblFormatFile.Visibility = Visibility.Visible;
                        // Refresh the control containing the message to the user.
                        lblFormatFile.Refresh();
                    }

                    // Load the data.
                    if (!mainTemplatesViewModel.SelectedMainTemplate.ExcelData.Connect(mainTemplatesViewModel.SelectedMainTemplate.RenameColumns, mainTemplatesViewModel.SelectedMainTemplate.ColumnForceToSplit))
                    {
                        // Show the list of duplicate column names in the Excel file if exist.
                        if (mainTemplatesViewModel.SelectedMainTemplate.ExcelData.DuplicateColumnNames.Count > 0)
                        {
                            DuplicateColumnView dialog = new DuplicateColumnView(mainTemplatesViewModel.SelectedMainTemplate.ExcelData.DuplicateColumnNames)
                            {
                                Left = Left + 100,
                                Top = Top + 100
                            };

                            Mouse.OverrideCursor = null;

                            _ = dialog.ShowDialog();
                        }

                        // Closes the application if no connection can be established with the data source or duplicate column names detected in the Excel file.
                        Close();
                    }

                    subTemplateLog = mainTemplatesViewModel.SelectedMainTemplate.GetSubTemplates(true);

                    mainTemplatesViewModel.NextStepButtonText = Properties.Resources.Button_LastStep;

                    gbxStep2.Visibility = Visibility.Collapsed;

                    gbxStep3.Visibility = Visibility.Visible;
                    elpStep3.IsEnabled = true;
                    elpStep3.Fill = (GradientBrush)FindResource("CurrentStep");

                    Mouse.OverrideCursor = null;

                    mainTemplatesViewModel.CurrentStep++;
                    mainTemplatesViewModel.UpdateStep();

                    break;
                case 3:
                    // Generate the Final Document.
                    Mouse.OverrideCursor = Cursors.Wait;

                    mainTemplatesViewModel.IsMergeNotInProgress = false;
                    mainTemplatesViewModel.IsNotLastStep = false;

                    rpbSteps.Visibility = Visibility.Visible;
                    rpbSteps.Maximum = 360 / rpbSteps.ShapeModeStep;
                    lblStep.Visibility = Visibility.Hidden;

                    await Task.Run(() =>
                    {
                        mainTemplatesViewModel.SelectedMainTemplate.Merge(rpbSteps, subTemplateLog, lblUserInfo);
                    });

                    rpbSteps.Visibility = Visibility.Hidden;

                    imgDoneStep3.IsEnabled = true;
                    elpStep3.Fill = (GradientBrush)FindResource("FinishedStep");

                    mainTemplatesViewModel.NextStepButtonText = Properties.Resources.Button_Quit;
                    mainTemplatesViewModel.IsNotLastStep = true;

                    Mouse.OverrideCursor = null;

                    mainTemplatesViewModel.CurrentStep++;

                    break;
                case 4:
                    // Close.
                    Close();
                    break;
            }
        }

        private void NextStepCommand_CanExecuted(object sender, CanExecuteRoutedEventArgs e)
        {
            switch (mainTemplatesViewModel.CurrentStep)
            {
                case 1:
                    e.CanExecute = mainTemplatesViewModel.SelectedMainTemplate != null
                                        && (mainTemplatesViewModel.SelectedMainTemplate.UseEventDetailFields != Models.EventDetails.Usage.Mandatory
                                        || mainTemplatesViewModel.SelectedMainTemplate.EventDetails.OptionalFieldsFilledOut);
                    break;
                case 2:
                    e.CanExecute = mainTemplatesViewModel.SelectedMainTemplate.ExcelData != null && !string.IsNullOrEmpty(mainTemplatesViewModel.SelectedMainTemplate.ExcelData.FilePath);
                    break;
                default:
                    e.CanExecute = true;
                    break;
            }
        }

        private void PrevStepCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            switch (mainTemplatesViewModel.CurrentStep)
            {
                case 2:
                    gbxStep2.Visibility = Visibility.Collapsed;
                    elpStep2.IsEnabled = false;
                    elpStep2.Fill = (Brush)FindResource("DisabledStep");

                    gbxStep1.Visibility = Visibility.Visible;
                    imgDoneStep1.IsEnabled = false;
                    elpStep1.Fill = (GradientBrush)FindResource("CurrentStep");

                    break;
                case 3:
                    gbxStep3.Visibility = Visibility.Collapsed;
                    elpStep3.IsEnabled = false;
                    elpStep3.Fill = (Brush)FindResource("DisabledStep");

                    gbxStep2.Visibility = Visibility.Visible;
                    imgDoneStep2.IsEnabled = false;
                    elpStep2.Fill = (GradientBrush)FindResource("CurrentStep");
                    lblFormatFile.Visibility = Visibility.Hidden;

                    mainTemplatesViewModel.NextStepButtonText = Properties.Resources.Button_NextStep;

                    break;
                default:
                    break;
            }

            mainTemplatesViewModel.CurrentStep--;
            mainTemplatesViewModel.UpdateStep();
        }

        private void PrevStepCommand_CanExecuted(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mainTemplatesViewModel.CurrentStep > 1;
        }

        private void CancelCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void CloseWindowCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private void ResetCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mainTemplatesViewModel.SelectedMainTemplate.EventDetails.Reset();
        }

        private void ResetCommand_CanExecuted(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mainTemplatesViewModel.SelectedMainTemplate != null && mainTemplatesViewModel.SelectedMainTemplate.EventDetails != null
                ? !string.IsNullOrEmpty(mainTemplatesViewModel.SelectedMainTemplate.EventDetails.Date) ||
                    !string.IsNullOrEmpty(mainTemplatesViewModel.SelectedMainTemplate.EventDetails.Location) ||
                    !string.IsNullOrEmpty(mainTemplatesViewModel.SelectedMainTemplate.EventDetails.Title)
                : false;
        }

        private void ProcessReportCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            WizardProcessReportView dialog = new WizardProcessReportView(mainTemplatesViewModel.SelectedMainTemplate.DocCreationSteps, 
                                                                            mainTemplatesViewModel.SelectedMainTemplate.UseEventDetailFields != Models.EventDetails.Usage.Hidden,
                                                                            mainTemplatesViewModel.SelectedMainTemplate.RenameColumns)
            {
                Left = Left + 30,
                Top = Top + 50
            };

            _ = dialog.ShowDialog();
        }

        private void ProcessReportCommand_CanExecuted(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = rpbSteps.Value == rpbSteps.Maximum;
        }

        private void TemplateParametersCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TemplateParametersView dialog = new TemplateParametersView(mainTemplatesViewModel.SelectedMainTemplate)
            {
                Left = Left + 50,
                Top = Top + 50
            };

            _ = dialog.ShowDialog();
        }
        private void TemplateParametersCommand_CanExecuted(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mainTemplatesViewModel.CurrentStep == 3;
            //e.CanExecute = mainTemplatesViewModel.CurrentStep == 1 && mainTemplatesViewModel.MainTemplates.Count > 0;
        }

        private void HelpCommand_CanExecuted(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void HelpCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            HelpAndUpdateView dialog = new HelpAndUpdateView()
            {
                Left = Left + 50,
                Top = Top + 50
            };

            _ = dialog.ShowDialog();
        }

        #endregion

        #region Events

        /// <summary>
        /// View the selected main template details.
        /// </summary>
        private void Label_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainTemplateDetailsView mainTemplateDetails = new MainTemplateDetailsView(mainTemplatesViewModel.SelectedMainTemplate)
            {
                Top = Top + 307,
                Left = Left + 80
            };

            _ = mainTemplateDetails.ShowDialog();
        }

        #endregion

        #region Methods

        #endregion
    }
}
