using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Packaging;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Linq;
using System.Windows.Threading;
using Microsoft.SqlServer.Management.SqlParser.Parser;

namespace ListFlow.Models
{
    /// <summary>
    /// Main template.
    /// </summary>
    public class MainTemplate : INotifyPropertyChanged
    {
        #region Fields

        // Word Application.
        private Application wApp;

        // Word Document.
        private Document wDoc;

        // Interop missing value.
        private readonly object oMissing = Missing.Value;
        // Interop true value.
        private readonly object oTrue = true;
        // Interop flase value.
        private readonly object oFalse = false;

        // Word visible.
        private readonly bool wAppVisible = false;

        // Selected Excel Data.
        private ExcelData excelData;

        // Event Details.
        private EventDetails eventDetails;

        // List of sub-templates for this main template.
        private List<SubTemplate> subTemplates;

        // Radial Progress Bar control.
        private Controls.RadialProgressBar rpb;

        // Progress information control.
        private System.Windows.Controls.Label lbl;

        // Selected sub-template.
        private SubTemplate selectedSubTemplate;

        // True if user has change parameters values.
        private bool isParametersValueChanged;
        // Template title (defined in Title file property).
        private string title;
        // Template description (defined in Comments file property).
        private string comment;
        // Events fields usage (option defined in the Word Document Category property).
        private EventDetails.Usage useEventDetailFields;
        // Rename the columns in the Excel file. 
        private bool renameColumns;
        // Keep the formated Excel file after building teh final document.
        private bool keepFormatedExcelFile;
        // Column name to split even if the name does not contain a / separator.
        private string columnForceToSplit;

        #endregion

        #region Properties

        // Organ Name.
        public string Organ { get; set; }
        // Word main template file name.
        public string Name { get; set; }
        // Folder containing the main template and all sub-templates for this Organ.
        public string OrganFolder { get; set; }
        // Main template Title (defined in Title Word Document property).
        public string Title
        {
            get => title;
            set
            {
                if (title != value)
                {
                    title = value;
                    IsParametersValueChanged = true;
                    OnPropertyChanged(nameof(Title));
                }
                else
                {
                    IsParametersValueChanged = false;
                }
            }
        }

        // Main template Comment (defined in Comments Word Document Property).
        public string Comment
        {
            get => comment;
            set
            {
                if (comment != value)
                {
                    comment = value;
                    IsParametersValueChanged = true;
                    OnPropertyChanged(nameof(Comment));
                }
                else
                {
                    IsParametersValueChanged = false;
                }
            }
        }
        // Usage of the Event detail fields (defined in the Category Word Document Property).
        public EventDetails.Usage UseEventDetailFields
        {
            get => useEventDetailFields;
            set
            {
                if (useEventDetailFields != value)
                {
                    useEventDetailFields = value;
                    IsParametersValueChanged = true;
                    OnPropertyChanged(nameof(UseEventDetailFields));
                }
                else
                {
                    IsParametersValueChanged = false;
                }
            }
        }
        // Main template full file path.
        public string FullPath
        {
            get
            {
                return !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(OrganFolder) ? Path.Combine(OrganFolder, Name) : string.Empty;
            }
        }
        // Excel source data used for the mailmerge.
        public ExcelData ExcelData
        {
            get => excelData;
            set
            {
                if (excelData != value)
                {
                    excelData = value;
                    OnPropertyChanged(nameof(ExcelData));
                }
            }
        }

        // Detail of the event/conference.
        public EventDetails EventDetails
        {
            get => eventDetails;
            set
            {
                if (eventDetails != value)
                {
                    eventDetails = value;
                    OnPropertyChanged(nameof(EventDetails));
                }
            }
        }

        // List of all sub-templates used in this main template.
        public List<SubTemplate> SubTemplates
        {
            get => subTemplates;
            set
            {
                if (subTemplates != value)
                {
                    subTemplates = value;
                    OnPropertyChanged(nameof(SubTemplates));
                }
            }
        }

        // Selected sub-template.
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

        // True if user has change value in parameters.
        public bool IsParametersValueChanged
        {
            get => isParametersValueChanged;
            set
            {
                if (isParametersValueChanged != value)
                {
                    isParametersValueChanged = value;
                    OnPropertyChanged(nameof(IsParametersValueChanged));
                }
            }
        }

        // Final document creation steps (for debugging purpose).
        public FinalDocCreationSteps DocCreationSteps { get; private set; }

        // Column name to split even if the name does not contain a / separator.
        public string ColumnForceToSplit
        {
            get => columnForceToSplit;
            set
            {
                if (columnForceToSplit != value)
                {
                    columnForceToSplit = value;
                    IsParametersValueChanged = true;
                    OnPropertyChanged(nameof(ColumnForceToSplit));
                }
                else
                {
                    IsParametersValueChanged = false;
                }
            }
        }

        // Rename the columns in the Excel file. 
        public bool RenameColumns
        {
            get => renameColumns;
            set
            {
                if (renameColumns != value)
                {
                    renameColumns = value;
                    IsParametersValueChanged = true;
                    OnPropertyChanged(nameof(RenameColumns));
                }
                else
                {
                    IsParametersValueChanged = false;
                }
            }
        }

        // Keep the formated Excel file after building teh final document.
        public bool KeepFormatedExcelFile
        {
            get => keepFormatedExcelFile;
            set
            {
                if (keepFormatedExcelFile != value)
                {
                    keepFormatedExcelFile = value;
                    OnPropertyChanged(nameof(KeepFormatedExcelFile));
                }
            }
        }

        #endregion

        #region Constants

        // File filter for template.
        private const string TemplateFileFilter = "*.docx";
        // Main template file name.
        private const string MainTemplateFileName = "Main.docx";
        // Main template file property Category content if the EventDetail fields are required.
        private const string MandatoryOptionName = "Mandatory";
        // Sub-template tag defined in main template as placeholder.
        private const string TagSearchCriteria = "#*.docx#";
        // RenameColomns option defined in Main template file Categories.
        public const string RenameColumnsOption = "RenameColumns";
        // ColumnForceToSplit option defined in Main template file Categories.
        public const string ColumnForceToSplitOption = "ColumnForceToSplit=";

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="rootPath">Root path of the templates.</param>
        /// <param name="wApp">Word Application instance.</param>
        public MainTemplate(string rootPath, Application wApp)
        {
            this.wApp = wApp;

            Organ = Path.GetFileName(rootPath);
            OrganFolder = rootPath;

            IEnumerable<string> fileList = Directory.EnumerateFiles(rootPath, TemplateFileFilter);

            if (wApp != null)
            {
                Title = string.Empty;
                ExcelData = new ExcelData();
                subTemplates = new List<SubTemplate>();
                RenameColumns = false;
                KeepFormatedExcelFile = false;

                foreach (string wDocFile in fileList)
                {
                    // Search Main.docx (main template) file.
                    if (string.Compare(Path.GetFileName(wDocFile), MainTemplateFileName) == 0)
                    {
                        // Get the file properties of the main template.
                        PackageProperties fileProperties = GetFileProperties(wDocFile, out Exception ex);
                        if (fileProperties != null)
                        {
                            Title = string.IsNullOrEmpty(fileProperties.Title) ? Path.GetFileName(Path.GetDirectoryName(wDocFile)) : fileProperties.Title;
                            Comment = fileProperties.Description ?? string.Empty;

                            if (fileProperties.Category != null)
                            {
                                if (string.IsNullOrEmpty(fileProperties.Category))
                                {
                                    UseEventDetailFields = EventDetails.Usage.Optional;
                                }
                                else if (fileProperties.Category.Contains(Enum.GetName(typeof(EventDetails.Usage), EventDetails.Usage.Mandatory)))
                                {
                                    UseEventDetailFields = EventDetails.Usage.Mandatory;
                                }
                                else if (fileProperties.Category.Contains(Enum.GetName(typeof(EventDetails.Usage), EventDetails.Usage.Optional)))
                                {
                                    UseEventDetailFields = EventDetails.Usage.Optional;
                                }
                                else if (fileProperties.Category.Contains(Enum.GetName(typeof(EventDetails.Usage), EventDetails.Usage.Hidden)))
                                {
                                    UseEventDetailFields = EventDetails.Usage.Hidden;
                                }

                                if (fileProperties.Category.Contains(RenameColumnsOption))
                                {
                                    RenameColumns = true;

                                    if (fileProperties.Category.Contains(ColumnForceToSplitOption))
                                    {
                                        ColumnForceToSplit = Helpers.ToolBox.ExtractBetweenTwoStrings(fileProperties.Category, "=[", "]", false, false);
                                    }
                                }

                                wAppVisible = fileProperties.Category.Contains("WordAppVisible");
                            }
                            else
                            {
                                UseEventDetailFields = EventDetails.Usage.Optional;
                            }

                            eventDetails = new EventDetails(UseEventDetailFields);
                        }
                        else
                        {
                            _ = new Helpers.CustomException(string.Format(Properties.Resources.Exception_ReadMainTemplateFileProperties, ex.Message), Properties.Resources.Exception_ReadFileProperties_Title);

                            // Ignore this Main template.
                            Title = string.Empty;
                        }

                        Name = Path.GetFileName(wDocFile);
                    }
                    else
                    {
                        // Add only docx file.
                        if (Path.GetExtension(wDocFile) == ".docx" && !Path.GetFileName(wDocFile).StartsWith("~$"))
                        {
                            subTemplates.Add(new SubTemplate(wDocFile));
                        }
                    }
                }
            }
            else
            {
                _ = new Helpers.CustomException(Properties.Resources.Exception_LaunchWord, Properties.Resources.Exception_ReadFileProperties_Title);                
            }
        }

        /// <summary>
        /// Get the list of sub-templates used in the main template.
        /// </summary>
        public List<string> GetSubTemplates(bool logDisabled)
        {
            List<string> log = new List<string>();

            foreach (SubTemplate subTemplate in subTemplates)
            {
                // Get the file properties.
                PackageProperties fileProperties = GetFileProperties(subTemplate.FilePath, out Exception ex);
                if (fileProperties != null)
                {
                    if (!string.IsNullOrEmpty(fileProperties.Description))
                    {
                        subTemplate.Query = fileProperties.Description ?? string.Empty;

                        if (!string.IsNullOrEmpty(subTemplate.Query))
                        {
                            subTemplate.SheetName = SubTemplate.GetSheetName(subTemplate.Query);

                            if (string.IsNullOrEmpty(subTemplate.SheetName))
                            {
                                subTemplate.Disabled = true;
                                if (logDisabled)
                                {
                                    log.Add(string.Format(Properties.Resources.Exception_SubTemplateNoSheetNameInQuery, subTemplate.FileName, subTemplate.Query));
                                }
                            }
                        }
                        else
                        {
                            subTemplate.Disabled = true;
                            if (logDisabled)
                            {
                                log.Add(string.Format(Properties.Resources.Exception_SubTemplateMissingQuery, subTemplate.FileName, Properties.Resources.FileProperty_Description));
                            }
                        }
                    }
                    else
                    {
                        subTemplate.Disabled = true;
                        if (logDisabled)
                        {
                            log.Add(string.Format(Properties.Resources.Exception_SubTemplateMissingQuery, subTemplate.FileName, Properties.Resources.FileProperty_Description));
                        }
                    }
                }
                else
                {
                    subTemplate.Disabled = true;
                    if (logDisabled)
                    {
                        log.Add(string.Format(Properties.Resources.Exception_SubTemplatePropertiesReading, subTemplate.FileName, ex.Message));
                    }
                }
            }

            CleanSubTemplateList(FullPath);

            return log;
        }

        /// <summary>
        /// // Get the list of sub-templates tags defined in the main template.
        /// </summary>
        /// <param name="file">Main template file name.</param>
        /// <returns>List of sub-template tags found.</returns>
        public List<string> GetSubTemplateTags(string file)
        {
            List<string> subTemplateTags = null;

            try
            {
                wDoc = wApp.Documents.OpenNoRepairDialog(file, oFalse, oTrue, oFalse, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing);

                // Get the sub-templates tags defined in the main template.
                if (wDoc != null)
                {
                    subTemplateTags = GetSubTemplateTags(wDoc, TagSearchCriteria);

                    wDoc.Close(oFalse, oMissing, oMissing);

                    _ = Marshal.ReleaseComObject(wDoc);
                }
            }
            catch (Exception ex)
            {
                wDoc = null;

                _ = new Helpers.CustomException($"{string.Format(Properties.Resources.Exception_CreateSubTemplateTagsList, file)}{Environment.NewLine}{Environment.NewLine}{Properties.Resources.Exception_ErrorDetails}{Environment.NewLine}{ex.Message}", Properties.Resources.Exception_CreateSubTemplateTagsList_Title);
            }

            return subTemplateTags;
        }

        /// <summary>
        /// Removes from the list of sub-templates those for which no tag exists in the main template.
        /// </summary>
        /// <param name="file">Main template file name.</param>
        private void CleanSubTemplateList(string file)
        {
            List<string> subTemplateTags = GetSubTemplateTags(file);

            foreach (SubTemplate subTemplate in subTemplates)
            {
                if (!subTemplate.Disabled)
                {
                    subTemplate.Disabled = !subTemplateTags.Contains($"#{subTemplate.FileName}#".ToUpper());
                }
            }

            _ = subTemplates.RemoveAll(x => x.Disabled);
        }

        #endregion

        #region Methods

        // tag:#Merge
        /// <summary>
        /// Create the final document by merging teh Excel file with all defined sub-templates.
        /// </summary>
        /// <param name="radialProgressBar">RadialProgressBar control used to track the progress of the final document generation.</param>
        /// <param name="subTemplateLog">List of all the sub-template that will be ignored.</param>
        /// <param name="userInfo">Label control used to inform the user of the current step.</param>
        public void Merge(Controls.RadialProgressBar radialProgressBar, List<string> subTemplateLog, System.Windows.Controls.Label userInfo)
        {
            DateTime start = DateTime.Now;
            bool errorRaised = false;
            bool warningRaided = false;

            rpb = radialProgressBar;
            lbl = userInfo;

            // Logging of the steps.
            DocCreationSteps = new FinalDocCreationSteps();

            // Logging the merge parameters.
            DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Information, FinalDocCreationSteps.EntryCategory.TemplateTitle, Title);
            DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Information, FinalDocCreationSteps.EntryCategory.TemplatePath, FullPath);
            DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Information, FinalDocCreationSteps.EntryCategory.TemplateComment, Comment);
            DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Information, FinalDocCreationSteps.EntryCategory.OptionalFieldsRequired, Enum.GetName(typeof(EventDetails.Usage), UseEventDetailFields));
            DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Information, FinalDocCreationSteps.EntryCategory.EventTitle, EventDetails.Title);
            DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Information, FinalDocCreationSteps.EntryCategory.EventLocation, EventDetails.Location);
            DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Information, FinalDocCreationSteps.EntryCategory.EventDate, EventDetails.Date);
            DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Information, FinalDocCreationSteps.EntryCategory.ExcelFile, ExcelData.FilePath);
            DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Information, FinalDocCreationSteps.EntryCategory.FormatedExcelFile, ExcelData.FormatedFilePath);
            DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Information, FinalDocCreationSteps.EntryCategory.KeepFormatedExcelFile, KeepFormatedExcelFile.ToString());
            DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Information, FinalDocCreationSteps.EntryCategory.SubTemplatesCount, SubTemplates.Count.ToString());
            DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Information, FinalDocCreationSteps.EntryCategory.CreationDateTime, $"{DateTime.Now.ToLongDateString()} @ {DateTime.Now.ToShortTimeString()}");

            // Logging ignored sub-templates.
            if (subTemplateLog.Count > 0)
            {
                DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Information, FinalDocCreationSteps.EntryCategory.DisabledSubTemplateLoggingTitle, Properties.Resources.FinalDocumentCreation_DisabledSubTemplateTitle);

                foreach (string item in subTemplateLog)
                {
                    DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Error, FinalDocCreationSteps.EntryCategory.DisabledSubTemplateItem, item);
                }
            }

            DocCreationSteps.Add(string.Format(Properties.Resources.FinalDocumentCreation_MainTemplateProcessing, Title));
            rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());

            DocCreationSteps.Add(string.Format(Properties.Resources.FinalDocumentCreation_OpenMainTemplate, FullPath));
            rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());

            // Open the main template.
            if (File.Exists(FullPath))
            {
                try
                {
                    // Hide Word during merge process.
                    wApp.Visible = wAppVisible;

                    // Create a new Word document base on the main template.
                    wDoc = wApp.Documents.Add(FullPath, oMissing, oMissing, oMissing);
                    if (wDoc != null)
                    {
                        int totalMergedRecord = 0;

                        DocCreationSteps.Add(Properties.Resources.FinalDocumentCreation_MainTemplateOpened);
                        rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());

                        // Loop over the Sub-templates list.
                        foreach (SubTemplate subTemplate in subTemplates)
                        {
                            // Informs the user about the sub-template being processed.
                            lbl?.Dispatcher.Invoke(new Action(() => { userInfo.Content = string.Format(Properties.Resources.FinalDocumentCreation_SubTemplateStartProcessing, subTemplate.FileName); }));

                            DocCreationSteps.Add(FinalDocCreationSteps.EntryType.StartProcessing, string.Format(Properties.Resources.FinalDocumentCreation_SubTemplateStartProcessing, subTemplate.FileName));
                            rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());

                            DocCreationSteps.Add(string.Format(Properties.Resources.FinalDocumentCreation_OpenSubTemplate, subTemplate.FileName));
                            rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());

                            try
                            {
                                // Check if the query are defined in this sub-template.
                                if (!string.IsNullOrEmpty(subTemplate.Query))
                                {
                                    // Check if this sub-template file exist.
                                    if (File.Exists(subTemplate.FilePath))
                                    {
                                        // Open this sub-template in Word (read-only).
                                        Document wSubTemplate = wApp.Documents.OpenNoRepairDialog(subTemplate.FilePath, oTrue, oFalse, oFalse, oMissing, oMissing, oMissing,
                                                                                        oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing);

                                        // Check if this sub-template are opened in ProtectedView in Word.
                                        bool protectedView = false;
                                        if (wApp.ProtectedViewWindows.Count > 0)
                                        {
                                            for (int i = 0; i < wApp.ProtectedViewWindows.Count - 1; i++)
                                            {
                                                if (wSubTemplate.Name.CompareTo($"{wApp.ProtectedViewWindows[i].Document.Name}") == 0)
                                                {
                                                    protectedView = true;

                                                    break;
                                                }
                                            }
                                        }

                                        // Checks if the sub-template was opened succesfully in Word and not opened in Word in ProtectedView (file comes from Internet or mail attachment).
                                        if (wSubTemplate != null & !protectedView)
                                        {
                                            DocCreationSteps.Add(Properties.Resources.FinalDocumentCreation_SubTemplateOpened);
                                            rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());

                                            DocCreationSteps.Add(Properties.Resources.FinalDocumentCreation_GetSubTemplateMergeParameters);
                                            rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());

                                            // Get the mergefields list and the sheet name defined in this sub-template.
                                            subTemplate.GetParameters(wSubTemplate);

                                            string queryPart1 = string.Empty;
                                            string queryPart2 = string.Empty;
                                            bool queryToLong = false;

                                            // Split the query in two parts if more than 255 caracters (max 2 x 255 chars).
                                            if (subTemplate.Query.Length > 255)
                                            {
                                                if (subTemplate.Query.Length < 512)
                                                {
                                                    queryPart1 = subTemplate.Query.Substring(0, 255);
                                                    queryPart2 = subTemplate.Query.Substring(255);
                                                }
                                                else
                                                {
                                                    // Ignore thi sub-template if the query contains more than 512 caracters.
                                                    DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Error, string.Format(Properties.Resources.Exception_QueryToLong, subTemplate.Query));
                                                    rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());

                                                    queryToLong = true;
                                                    errorRaised = true;
                                                }
                                            }
                                            else
                                            {
                                                queryPart1 = subTemplate.Query;
                                                queryPart2 = string.Empty;
                                            }

                                            if (!queryToLong)
                                            {
                                                //TODO: Add SQL Parser.
                                                // Check the query syntax.
                                                ParseResult sqlParseResult = Parser.Parse(subTemplate.Query);
                                                if (sqlParseResult.Errors.Count() == 0)
                                                {
                                                    // Check if the Excel source file doesn't exist.
                                                    if (File.Exists(ExcelData.FormatedFilePath))
                                                    {
                                                        DocCreationSteps.Add(Properties.Resources.FinalDocumentCreation_SearchSheet);
                                                        rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());

                                                        // Check if the sheet name defined in the query exist in the Excel source file.
                                                        bool sheetExist = subTemplate.Query.Contains(ExcelData.SheetName);

                                                        // Ignore this sub-template if the sheet name defined in the query doesn't exist in the Excel source file.
                                                        if (sheetExist)
                                                        {
                                                            DocCreationSteps.Add(Properties.Resources.FinalDocumentCreation_CheckMergeFields);
                                                            rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());

                                                            // Check that the names of the merge fields match the fields in the source file.
                                                            List<string> notMatchingMergeFields = subTemplate.CheckMergeFields(ExcelData.ColumnFieldNames);

                                                            // Ignore this sub-template if one or more merge field doesn't exist in the Excel source file.
                                                            if (notMatchingMergeFields.Count == 0)
                                                            {
                                                                // Check if the fields defined in the query exist in the Excel source file.
                                                                List<string> notMatchingQueryFields = subTemplate.CheckQueryFields(ExcelData.ColumnFieldNames);

                                                                // Ignore this sub-template if one or more query field doesn't exist in the Excel source file.
                                                                if (notMatchingQueryFields.Count == 0)
                                                                {
                                                                    // Set the merge type to Catalog.
                                                                    wSubTemplate.MailMerge.MainDocumentType = WdMailMergeMainDocType.wdCatalog;

                                                                    DocCreationSteps.Add(string.Format(Properties.Resources.FinalDocumentCreation_OpenDataSource, ExcelData.FormatedFilePath, queryPart1, queryPart2));
                                                                    rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());

                                                                    wSubTemplate.MailMerge.OpenDataSource(ExcelData.FormatedFilePath, oMissing, oFalse, oTrue, oMissing, oFalse, oMissing, oMissing,
                                                                                                                oMissing, oMissing, oMissing, oMissing, queryPart1, queryPart2, oFalse, oMissing);

                                                                    // Counts the number of records matching the criteria (query). 
                                                                    bool noRecord = false;
                                                                    try
                                                                    {
                                                                        wSubTemplate.MailMerge.DataSource.ActiveRecord = WdMailMergeActiveRecord.wdLastRecord;
                                                                        _ = int.TryParse(wSubTemplate.MailMerge.DataSource.ActiveRecord.ToString(), out int lastRecordID);
                                                                        wSubTemplate.MailMerge.DataSource.ActiveRecord = WdMailMergeActiveRecord.wdFirstRecord;

                                                                        totalMergedRecord += lastRecordID;

                                                                        DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Result, string.Format(Properties.Resources.FinalDocumentCreation_NumberOfRecords, lastRecordID));
                                                                        rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());
                                                                    }
                                                                    catch (COMException ex)
                                                                    {
                                                                        DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Warning, Properties.Resources.Exception_NoMatchingData);
                                                                        rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());

                                                                        // No matching record.
                                                                        noRecord = ex.HResult == -2146822435;
                                                                        warningRaided = true;
                                                                    }
                                                                    catch (Exception ex)
                                                                    {
                                                                        DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Error, string.Format(Properties.Resources.Exception_CountRecords, ex.Message));
                                                                        rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());

                                                                        noRecord = true;
                                                                        errorRaised = true;
                                                                    }

                                                                    // Ignore thi sub-template if no matching record or an error occured.
                                                                    bool mergeError = false;
                                                                    if (!noRecord)
                                                                    {
                                                                        // Set the mail merge destination to a new document.
                                                                        wSubTemplate.MailMerge.Destination = WdMailMergeDestination.wdSendToNewDocument;

                                                                        try
                                                                        {
                                                                            DocCreationSteps.Add(Properties.Resources.FinalDocumentCreation_StartMergeSubTemplate);
                                                                            rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());

                                                                            // Performs the sub-template mailmerge.
                                                                            wSubTemplate.MailMerge.Execute(oFalse);
                                                                        }
                                                                        catch (COMException ex)
                                                                        {
                                                                            DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Warning, string.Format(Properties.Resources.Exception_NoMatchingData, subTemplate.FileName));
                                                                            rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());

                                                                            // No matching record.
                                                                            noRecord = ex.HResult == -2146822657;
                                                                            warningRaided = true;
                                                                        }
                                                                        catch (Exception ex)
                                                                        {
                                                                            mergeError = true;

                                                                            DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Error, string.Format(Properties.Resources.Exception_MergeSubTemplate, ex.Message));
                                                                            rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());
                                                                            errorRaised = true;
                                                                        }
                                                                    }

                                                                    if (!noRecord && !mergeError)
                                                                    {
                                                                        DocCreationSteps.Add(Properties.Resources.FinalDocumentCreation_MergeSubTemplateSuccessfully);
                                                                        rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());


                                                                        // Merge successfully.
                                                                        Document wMailMergeResult = wApp.ActiveDocument;

                                                                        DocCreationSteps.Add(Properties.Resources.FinalDocumentCreation_CopyMergeDataToMain);
                                                                        rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());

                                                                        // Copy to the Office Clipboard the result of the merge.
                                                                        wMailMergeResult.StoryRanges[WdStoryType.wdMainTextStory].Copy();

                                                                        DocCreationSteps.Add(string.Format(Properties.Resources.FinalDocumentCreation_SearchTagInMain, subTemplate.FileName));
                                                                        rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());

                                                                        // Find the tag of this sub-template in the main template.
                                                                        Range wRange = wDoc.Content;
                                                                        wRange.Find.ClearFormatting();
                                                                        wRange.Find.Text = $"#{subTemplate.FileName}#";
                                                                        wRange.Find.Forward = true;
                                                                        wRange.Find.Wrap = WdFindWrap.wdFindStop;
                                                                        if (wRange.Find.Execute())
                                                                        {
                                                                            DocCreationSteps.Add(Properties.Resources.FinalDocumentCreation_PasteMergeDataToMain);
                                                                            rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());

                                                                            // Paste the merge result to this location with keeping the original formatting (from sub-template).
                                                                            wRange.PasteAndFormat(WdRecoveryType.wdFormatOriginalFormatting);
                                                                        }
                                                                        else
                                                                        {
                                                                            DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Error, string.Format(Properties.Resources.Exception_TagNotExist, subTemplate.FileName));
                                                                            rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());
                                                                        }

                                                                        _ = Marshal.ReleaseComObject(wRange);

                                                                        DocCreationSteps.Add(Properties.Resources.FinalDocumentCreation_CloseMergeResultSubTemplate);
                                                                        rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());

                                                                        // Close the merge result document without saving, if Word if hidden.
                                                                        if (!wAppVisible)
                                                                        {
                                                                            wMailMergeResult.Close(oFalse, oMissing, oMissing);
                                                                        }

                                                                        _ = Marshal.ReleaseComObject(wMailMergeResult);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    // Liste the query fields that are not available in the data source Excel file, for debuging purpose.
                                                                    foreach (string item in notMatchingQueryFields)
                                                                    {
                                                                        DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Error, string.Format(Properties.Resources.Exception_QueryFieldNotInDataSource, item));
                                                                        rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());
                                                                    }

                                                                    DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Error, Properties.Resources.Exception_SubTemplateIgnored);
                                                                    rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());
                                                                    errorRaised = true;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                // Liste the merge fields that are not available in the data source Excel file, for debuging purpose.
                                                                foreach (string item in notMatchingMergeFields)
                                                                {
                                                                    DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Error, string.Format(Properties.Resources.Exception_MergeFieldNotInDataSource, item));
                                                                    rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());
                                                                }

                                                                DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Error, Properties.Resources.Exception_SubTemplateIgnored);
                                                                rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());
                                                                errorRaised = true;
                                                            }

                                                            DocCreationSteps.Add(Properties.Resources.FinalDocumentCreation_CloseSubTemplate);
                                                            rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());
                                                        }
                                                        else
                                                        {
                                                            DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Error, string.Format(Properties.Resources.Exception_SheetNotExist, subTemplate.Query));
                                                            rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());
                                                        }
                                                    }
                                                    else
                                                    {
                                                        // Ignore this sub-template because the Excel source file doesn't exist.
                                                        DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Error, string.Format(Properties.Resources.Exception_DataSourceNotExist, ExcelData.FormatedFilePath));
                                                        rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());
                                                        errorRaised = true;
                                                    }
                                                }
                                                else
                                                {
                                                    // Ignore this sub-template because a syntax error is present in the SQL code.
                                                    if (sqlParseResult.Errors.Count() == 1)
                                                    {
                                                        DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Error, Properties.Resources.Exception_SqlSyntaxIssue);
                                                    }
                                                    else
                                                    {
                                                        DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Error, string.Format(Properties.Resources.Exception_SqlSyntaxIssues,sqlParseResult.Errors.Count()));
                                                    }

                                                    DocCreationSteps.Add(FinalDocCreationSteps.EntryType.SqlSyntax, string.Format(Properties.Resources.Exception_SqlSyntaxIssuesQuery, subTemplate.Query));
                                                    foreach (Error item in sqlParseResult.Errors)
                                                    {
                                                        DocCreationSteps.Add(FinalDocCreationSteps.EntryType.SqlSyntax, string.Format(Properties.Resources.Exception_SqlSyntaxIssuesErrorText, item.Message));

                                                        List<string> queryErrorLocation = ExtractQuerySyntaxErrorLocation(subTemplate.Query, item.Start.Offset, item.End.Offset);
                                                        if (queryErrorLocation.Count == 3)
                                                        {
                                                            DocCreationSteps.Add(FinalDocCreationSteps.EntryType.SqlSyntax, string.Format(Properties.Resources.Exception_SqlSyntaxIssuesErrorLocation, queryErrorLocation[0], queryErrorLocation[1], queryErrorLocation[2]));
                                                        }
                                                    }
                                                    rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());
                                                    errorRaised = true;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (protectedView)
                                            {
                                                // Ignore this sub-template because the sub-template was opened in ProtectedView in Word. 
                                                DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Error, string.Format(Properties.Resources.Exception_SubTemplateInProtectedView, subTemplate.FilePath));
                                                rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());
                                            }
                                            else
                                            {
                                                // Ignore this sub-template because the sub-template object doesn't exist. 
                                                DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Error, string.Format(Properties.Resources.Exception_SubTemplateNotExist, subTemplate.FilePath));
                                                rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());
                                            }
                                            errorRaised = true;
                                        }

                                        // Close the sub-template without saving.
                                        wSubTemplate.Close(oFalse, oMissing, oMissing);
                                        _ = Marshal.ReleaseComObject(wSubTemplate);
                                    }
                                    else
                                    {
                                        // Ignore this sub-template because the sub-template file doesn't exist. 
                                        DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Error, string.Format(Properties.Resources.Exception_SubTemplateNotExist, subTemplate.FilePath));
                                        rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());
                                        errorRaised = true;
                                    }
                                }
                                else
                                {
                                    // Ignore this sub-template because no query was defined.
                                    DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Error, string.Format(Properties.Resources.Exception_SubTemplateNoQueryDefined, subTemplate.FilePath));
                                    rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());
                                    errorRaised = true;
                                }
                            }
                            catch (Exception ex)
                            {
                                DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Error, string.Format(Properties.Resources.Exception_SubTemplateGlobalException, ex.Message));
                                rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());
                            }

                            DocCreationSteps.Add(FinalDocCreationSteps.EntryType.EndProcessing, string.Format(Properties.Resources.FinalDocumentCreation_SubTemplateEndProcessing, subTemplate.FileName));
                            rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());
                        }

                        // Clean user information.
                        lbl?.Dispatcher.Invoke(new Action(() => { userInfo.Content = string.Empty; }));

                        // Update the Events Fields.
                        if (!string.IsNullOrEmpty(eventDetails.Title) || !string.IsNullOrEmpty(eventDetails.Date) || !string.IsNullOrEmpty(eventDetails.Location))
                        {
                            DocCreationSteps.Add(Properties.Resources.FinalDocumentCreation_UpdateEventDetails);
                            rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());

                            // Find all Word FormField and replace content with the content of the Event details.
                            foreach (FormField wFormField in wDoc.FormFields)
                            {
                                if (wFormField.Name == "EventTitle")
                                {
                                    if (!string.IsNullOrEmpty(eventDetails.Title))
                                    {
                                        wFormField.Range.Text = eventDetails.Title;

                                        DocCreationSteps.Add(string.Format(Properties.Resources.FinalDocumentCreation_UpdateEventField, "EventTitle"));
                                        rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());
                                    }
                                }
                                else if (wFormField.Name == "EventLocation")
                                {
                                    if (!string.IsNullOrEmpty(eventDetails.Location))
                                    {
                                        wFormField.Result = eventDetails.Location;
                                        DocCreationSteps.Add(string.Format(Properties.Resources.FinalDocumentCreation_UpdateEventField, "EventLocation"));
                                        rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());
                                    }
                                }
                                else if (wFormField.Name == "EventDate")
                                {
                                    if (!string.IsNullOrEmpty(eventDetails.Date))
                                    {
                                        wFormField.Result = eventDetails.Date;
                                        DocCreationSteps.Add(string.Format(Properties.Resources.FinalDocumentCreation_UpdateEventField, "EventDate"));
                                        rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());
                                    }
                                }
                            }
                        }

                        // Update the Participants Count FormField.
                        DocCreationSteps.Add(Properties.Resources.FinalDocumentCreation_UpdateParticipantsCount);
                        rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());

                        foreach (FormField wFormField in wDoc.FormFields)
                        {
                            if (wFormField.Name == "Participants")
                            {
                                wFormField.Result = totalMergedRecord.ToString();
                                DocCreationSteps.Add(string.Format(Properties.Resources.FinalDocumentCreation_UpdateParticipantsCountField, "Participants"));
                                rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar());
                            }
                        }

                        // Make Word visible when merge is finished.
                        if (!wApp.Visible)
                        {
                            wApp.Visible = true;
                            wApp.Activate();
                        }

                        DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Result, string.Format(Properties.Resources.FinalDocumentCreation_DocumentName, wDoc.Name));
                        DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Result, string.Format(Properties.Resources.FlowDoc_TotalMergedRecord, totalMergedRecord));
                        DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Result, Properties.Resources.FinalDocumentCreation_Successfully);
                        rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar(true));
                    }
                    else
                    {
                        DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Error, string.Format(Properties.Resources.Exception_OpenMainTemplate, FullPath));
                        rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar(true, true));
                        errorRaised = true;
                    }
                }
                catch (Exception ex)
                {
                    DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Error, string.Format(Properties.Resources.Exception_WordOpenMainTemplate, FullPath, Title, ex.Message));
                    rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar(true, true));
                    errorRaised = true;
                }
                _ = Marshal.ReleaseComObject(wDoc);
            }
            else
            {
                DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Error, string.Format(Properties.Resources.Exception_MainTemplateNotExist, FullPath, Title));
                rpb?.Dispatcher.Invoke(() => UpdateRadialProgressBar(true, true));
                errorRaised = true;
            }

            // Show a message when merge is completed with or without error.
            if (errorRaised)
            {
                lbl?.Dispatcher.Invoke(new Action(() => { userInfo.FontSize = 14; }));
                lbl?.Dispatcher.Invoke(new Action(() => { userInfo.Foreground = Brushes.Red; }));
                lbl?.Dispatcher.Invoke(new Action(() => { userInfo.Content = string.Format(Properties.Resources.FinalDocumentCreation_Unsuccessfully_UserInfo, Properties.Resources.Button_ProcessReport.Replace("_", "")); }));
            }
            else if (warningRaided)
            {
                lbl?.Dispatcher.Invoke(new Action(() => { userInfo.FontSize = 14; }));
                lbl?.Dispatcher.Invoke(new Action(() => { userInfo.Foreground = Brushes.Orange; }));
                lbl?.Dispatcher.Invoke(new Action(() => { userInfo.Content = string.Format(Properties.Resources.FinalDocumentCreation_Warning_UserInfo, Properties.Resources.Button_ProcessReport.Replace("_", "")); }));
            }
            else
            {
                lbl?.Dispatcher.Invoke(new Action(() => { userInfo.FontSize = 14; }));
                lbl?.Dispatcher.Invoke(new Action(() => { userInfo.Content = string.Format(Properties.Resources.FinalDocumentCreation_Successfully_UserInfo, Properties.Resources.Button_ProcessReport.Replace("_", "")); }));
            }

            // Logging the time spend for making the final document ready.
            TimeSpan ts = DateTime.Now - start;
            DocCreationSteps.Add(FinalDocCreationSteps.EntryType.Warning, FinalDocCreationSteps.EntryCategory.ProcessDuration, string.Format(Properties.Resources.FlowDoc_ProcessDuration, ts.TotalSeconds));
        }

        /// <summary>
        /// Update the RadialProgressBar control.
        /// </summary>
        /// <param name="max">True: set the maxium value.</param>
        /// <param name="error">True: change the color.</param>
        private void UpdateRadialProgressBar(bool max = false, bool error = false)
        {
            if (rpb.Value == rpb.Maximum)
            {
                rpb.Value = 1;
            }
            else
            {
                rpb.Value++;
            }

            if (max)
            {
                rpb.Value = rpb.Maximum;
            }

            if (error)
            {
                rpb.Foreground = System.Windows.Application.Current.FindResource("ListViewHeaderTemplateNoSorting") as LinearGradientBrush;
            }
        }

        /// <summary>
        /// Get the properties of the file.
        /// </summary>
        /// <param name="file">File.</param>
        /// <returns>All the properties or null (if file not found or unavailable.</returns>
        private PackageProperties GetFileProperties(string file, out Exception exception)
        {

            if (!IsFileInUse(file, out Exception fileInUseException))
            {
                PackageProperties properties;

                try
                {
                    Package docx = Package.Open(file, FileMode.Open, FileAccess.Read);

                    properties = docx.PackageProperties;

                    docx.Close();

                    exception = null;
                    return properties;
                }
                catch (Exception ex)
                {
                    exception = new Exception($"{string.Format(Properties.Resources.Exception_ReadFileProperties_Details, file)}{Environment.NewLine}{Environment.NewLine}{Properties.Resources.Exception_ErrorDetails}{Environment.NewLine}{ex.Message}");
                }
            }
            else
            {
                exception = fileInUseException;
            }

            return null;
        }

        /// <summary>
        /// Check if the file is in use (read/write) by another process.
        /// </summary>
        /// <param name="file">File to be checked.</param>
        /// <returns>True if the file is in use, false if the fiel is not in use.</returns>
        private bool IsFileInUse(string file, out Exception exception)
        {
            if (File.Exists(file))
            {
                try
                {
                    using (FileStream fs = new FileStream(file, FileMode.Open))
                    {
                        bool canRead = fs.CanRead;
                        bool canWrite = fs.CanWrite;
                    }

                    exception = null;
                    return false;
                }
                catch (Exception ex)
                {
                    exception = new Exception(ex.Message);
                    return true;
                }
            }
            else
            {
                exception = new Exception(string.Format(Properties.Resources.Exception_FileInUseNotExist, file));
                return false;
            }
        }

        /// <summary>
        /// Searches for tags present in the main model that map to the sub-models.
        /// </summary>
        /// <param name="document">Document to be analyzed.</param>
        /// <returns>List of tags or null</returns>
        private List<string> GetSubTemplateTags(Document document, string searchCriteria)
        {
            List<string> tags = new List<string>();

            Range wRange = document.Content;
            wRange.Find.ClearFormatting();
            wRange.Find.MatchWholeWord = true;
            wRange.Find.MatchWildcards = true;
            wRange.Find.Text = searchCriteria;
            wRange.Find.Forward = true;
            wRange.Find.Wrap = WdFindWrap.wdFindStop;
            while (wRange.Find.Execute())
            {
                tags.Add(wRange.Text.ToUpper());
            }

            _ = Marshal.ReleaseComObject(wRange);

            return tags;
        }

        /// <summary>
        /// Save the main template parameters in file properties fields.
        /// </summary>
        public void SaveParameters()
        {
            IsParametersValueChanged = !SetFileProperties(FullPath, Title, Comment, UseEventDetailFields, RenameColumns, ColumnForceToSplit);
        }

        /// <summary>
        /// Set the file properties fields.
        /// </summary>
        /// <param name="file">File to be processed.</param>
        /// <param name="title">Title propery content.</param>
        /// <param name="comments">Comments propterty content.</param>
        /// <param name="mandatory">Category property content.</param>
        /// <returns>True if update done without error.</returns>
        private bool SetFileProperties(string file, string title, string comments, EventDetails.Usage useEventDetailFields, bool renameColumns, string columnForceToSplit)
        {
            try
            {
                Package docx = Package.Open(file, FileMode.Open, FileAccess.ReadWrite);

                docx.PackageProperties.Title = title;
                docx.PackageProperties.Description = comments;
                string category = Enum.GetName(typeof(EventDetails.Usage), useEventDetailFields);
                if (renameColumns)
                {
                    category = $"{category};{RenameColumnsOption}";
                    if (!string.IsNullOrEmpty(columnForceToSplit) && string.Compare(columnForceToSplit, Properties.Resources.None) != 0)
                    {
                        category = $"{category};{ColumnForceToSplitOption}[{columnForceToSplit}]";
                    }
                }
                docx.PackageProperties.Category = category;

                docx.Close();

                return true;
            }
            catch (Exception ex)
            {
                _ = new Helpers.CustomException($"{string.Format(Properties.Resources.Exception_WriteFileProperties, file)}{Environment.NewLine}{Environment.NewLine}{Properties.Resources.Exception_ErrorDetails}{Environment.NewLine}{ex.Message}", Properties.Resources.Exception_WriteFileProperties_Title);

                return false;
            }
        }

        private List<string> ExtractQuerySyntaxErrorLocation(string query, int errorStartPos, int errorEndPos)
        {
            List<string> queryParts = new List<string>();
            // Number of characters extracted before and after the syntax error location.
            const int BeforeAfterPos = 10;

            // Before syntax error location.
            if (errorStartPos >= BeforeAfterPos)
            {
                queryParts.Add(query.Substring(errorStartPos - BeforeAfterPos, BeforeAfterPos));
            }
            else
            {
                queryParts.Add(query.Substring(0, errorStartPos));
            }
            // Syntax error.
            queryParts.Add(query.Substring(errorStartPos, errorEndPos - errorStartPos));
            // After syntax error location.
            if (errorEndPos + BeforeAfterPos <= query.Length)
            {
                queryParts.Add(query.Substring(errorEndPos, BeforeAfterPos));
            }
            else
            {
                queryParts.Add(query.Substring(errorEndPos, query.Length - errorEndPos));
            }

            return queryParts;
        }

        public override string ToString()
        {
            return base.ToString();
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
