using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.IO;
using Microsoft.Office.Interop.Word;
using System.Runtime.InteropServices;
using ListFlow.Models;
using ListFlow.Helpers;
using System.Diagnostics;

namespace ListFlow.ViewModels
{
    /// <summary>
    /// Main template View model (logic).
    /// </summary>
    public class MainTemplatesViewModel : INotifyPropertyChanged, IFilesDropped
    {
        #region Fields

        // Current Word Application instance.
        private Application wApp;
        // Interop missing value.
        private object oMissing = Missing.Value;
        // Interop true value.
        private object oTrue = true;
        // Interop false value.
        private object oFalse = false;
        // Current step in the Wizard.
        private int currentStep;
        // Current step formated text.
        private string stepFormated;
        // Dynamic information text for the user.
        private string userInfo;
        // Next step button text.
        private string nextStepButtonText;
        // True if the final merge is not in progress.
        private bool isMergeNotInProgress;
        // True if the current step is not the last one.
        private bool isNotLastStep;
        // Selected Word Template.
        private MainTemplate selectedMainTemplate;
        // List of existing Word Templates (content of TemplatesRootFolderName folder).
        private List<MainTemplate> mainTemplates;
        // List of possible data filters.
        private Dictionary<string, string> filters;
        // List of possible data sort comparisons.
        private Dictionary<string, string> comparisons;

        #endregion

        #region Constants

        // Default root folder containing the Word Templates for each clients.
        private const string TemplatesRootFolderName = "Templates";
        // Max number of steps.
        private const int MaxSteps = 3;

        #endregion

        #region Properties

        public string NextStepButtonText
        {
            get => nextStepButtonText;
            set
            {
                if (nextStepButtonText != value)
                {
                    nextStepButtonText = value;
                    OnPropertyChanged(nameof(NextStepButtonText));
                }
            }
        }

        public MainTemplate SelectedMainTemplate
        {
            get => selectedMainTemplate;
            set
            {
                if (selectedMainTemplate != value)
                {
                    selectedMainTemplate = value;
                    OnPropertyChanged(nameof(SelectedMainTemplate));
                };
            }
        }

        public List<MainTemplate> MainTemplates
        {
            get => mainTemplates;
            set
            {
                if (mainTemplates != value)
                {
                    mainTemplates = value;
                    OnPropertyChanged(nameof(MainTemplates));
                }
            }
        }

        public string StepFormated
        {
            get => stepFormated;
            set
            {
                if (stepFormated != value)
                {
                    stepFormated = value;
                    OnPropertyChanged(nameof(StepFormated));
                }
            }
        }

        public int CurrentStep
        {
            get => currentStep;
            set
            {
                if (currentStep != value)
                {
                    currentStep = value;
                    OnPropertyChanged(nameof(CurrentStep));
                }
            }
        }

        public string UserInfo
        {
            get => userInfo;
            set
            {
                if (userInfo != value)
                {
                    userInfo = value;
                    OnPropertyChanged(nameof(userInfo));
                }
            }
        }

        public Dictionary<string, string> Filters
        {
            get => filters;
            set
            {
                if (filters != value)
                {
                    filters = value;
                    OnPropertyChanged(nameof(Filters));
                }
            }
        }

        public Dictionary<string, string> Comparisons
        {
            get => comparisons; 
            set
            {
                if (comparisons != value)
                {
                    comparisons = value;
                    OnPropertyChanged(nameof(Comparisons));
                }
            }
        }

        public bool IsMergeNotInProgress
        {
            get => isMergeNotInProgress; 
            set
            {
                if (isMergeNotInProgress != value)
                {
                    isMergeNotInProgress = value;
                    OnPropertyChanged(nameof(IsMergeNotInProgress));
                }
            }
        }

        public bool IsNotLastStep
        {
            get => isNotLastStep;
            set
            {
                if (isNotLastStep != value)
                {
                    isNotLastStep = value;
                    OnPropertyChanged(nameof(IsNotLastStep));
                }
            }
        }

        #endregion

        #region Constructors

        public MainTemplatesViewModel()
        {
            // Start a Word instance (hidden).
            if (StartWord(false))
            {
                // Get all main template present in Templates application root sub-folder.
                if (GetMainTemplates(AppDomain.CurrentDomain.BaseDirectory))
                {
                    if (MainTemplates.Count > 0)
                    {
                        SelectedMainTemplate = MainTemplates.First();

                        CurrentStep = 1;
                        NextStepButtonText = Properties.Resources.Button_NextStep;

                        UpdateStep();
                    }
                    else
                    {
                        _ = new CustomException(string.Format(Properties.Resources.Exception_NoItemTemplatesSubFolder, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TemplatesRootFolderName)));
                    }
                }
                else
                {
                    _ = new CustomException(string.Format(Properties.Resources.Exception_NoTemplatesSubFolder, TemplatesRootFolderName, AppDomain.CurrentDomain.BaseDirectory));
                }
            }
            else
            {
                _ = new CustomException(Properties.Resources.Exception_LaunchWord, Properties.Resources.Exception_LaunchWord_Title);
            }
        }

        ~MainTemplatesViewModel()
        {
            CloseWord();
            CloseHiddenWordInstancies();

            if (selectedMainTemplate != null && selectedMainTemplate.ExcelData != null && selectedMainTemplate.ExcelData.FilePath != null)
            {
                selectedMainTemplate.ExcelData.CloseHiddenExcelInstancies();
                if (selectedMainTemplate.RenameColumns && !selectedMainTemplate.KeepFormatedExcelFile)
                {
                    selectedMainTemplate.ExcelData.Disconnect();
                }
            };


        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the list of main templates existing in the subfolder defined by the constant TemplatesRootFolderName.
        /// </summary>
        /// <param name="rootPath">Root folder to be analyzed.</param>
        /// <returns>True if at least one main template exists in the folder.</returns>
        private bool GetMainTemplates(string rootPath)
        {
            if (Directory.Exists(Path.Combine(rootPath, TemplatesRootFolderName)))
            {
                // List of main templates.
                MainTemplates = new List<MainTemplate>();

                IEnumerable<string> directories = Directory.EnumerateDirectories(Path.Combine(rootPath, TemplatesRootFolderName));

                // Browse all the subfolders in the Templates folder to create the list of templates.
                foreach (string item in directories)
                {
                    // Add item to the list.
                    MainTemplate mainTemplate = new MainTemplate(item, wApp);

                    // Ignore sub-folders that are empty or that do not contain a main template or that do not contain sub-templates.
                    if (!string.IsNullOrEmpty(mainTemplate.Title) && mainTemplate.SubTemplates.Count > 0)
                    {
                        MainTemplates.Add(mainTemplate);
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Formats the text of the current step indicator.
        /// </summary>
        public void UpdateStep()
        {
            StepFormated = string.Format(Properties.Resources.Import_Step_Steps, currentStep, MaxSteps);
        }

        /// <summary>
        /// Start a Word instance or connect to a existing instance.
        /// </summary>
        /// <param name="visible">True: Word will be visible, False: Word will be hidden during the converting.</param>
        private bool StartWord(bool visible)
        {
            CloseHiddenWordInstancies();

            try
            {
                if (wApp == null)
                {
                    wApp = (Application)Activator.CreateInstance(Type.GetTypeFromProgID("Word.Application"));
                    wApp.Visible = visible;
                }

                return true;
            }
            catch (COMException)
            {
                return false;
            }
            catch (Exception ex)
            {
                _ = new CustomException(ex.Message, Properties.Resources.Exception_LaunchWord);

                return false;
            }
        }

        /// <summary>
        /// Close all hidden Word instancies.
        /// </summary>
        private void CloseHiddenWordInstancies()
        {
            try
            {
                List<Process> processes = Process.GetProcesses().ToList().Where(x => x.ProcessName == "WINWORD" && string.IsNullOrEmpty(x.MainWindowTitle)).ToList();
                foreach (Process p in processes)
                {
                    p.Kill();
                }
            }
            catch (Exception ex)
            {
                _ = new CustomException(ex.Message, Properties.Resources.Exception_CloseWord_Title);
            }
        }

        /// <summary>
        /// Close current hidden Word instance.
        /// </summary>
        private void CloseWord()
        {
            try
            {
                if (wApp != null && !wApp.Visible)
                {
                    foreach (Document item in wApp.Documents)
                    {
                        item.Close(oFalse, oMissing, oMissing);
                    }
                    wApp.Quit(oFalse, oMissing, oMissing);
                    _ = Marshal.ReleaseComObject(wApp);
                    wApp = null;
                }
            }
            catch (COMException ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves the name of the Excel file dropped in the control (first name in the list).
        /// </summary>
        /// <param name="files">List of files dropped in the control.</param>
        public void OnFilesDropped(string[] files)
        {
            SelectedMainTemplate.ExcelData.FilePath = files[0];
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
