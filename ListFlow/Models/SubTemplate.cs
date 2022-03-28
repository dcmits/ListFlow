using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Reflection;
using Microsoft.Office.Interop.Word;
using System.Linq;

namespace ListFlow.Models
{
    /// <summary>
    /// Sub-template.
    /// </summary>
    public class SubTemplate : INotifyPropertyChanged
    {
        #region Fields

        // Interop missing value.
        private object oMissing = Missing.Value;
        // Interop true value.
        private object oTrue = true;
        // Interop false value.
        private object oFalse = false;

        // Query (saved in Comments Property from docx template file).
        private string query;
        // True if user change the query content.
        private bool isQueryValueChanged;

        #endregion

        #region Properties

        // Template file name.
        public string FileName { get; set; }
        // Query stored in the Comments property of the template.
        public string Query
        {
            get => query;
            set
            {
                if (query != value)
                {
                    query = value;
                    IsQueryValueChanged = true;
                    OnPropertyChanged(nameof(Query));
                }
                else
                {
                    IsQueryValueChanged = false;
                }
            }
        }
        // True if user change the query content.
        public bool IsQueryValueChanged
        { 
            get => isQueryValueChanged;
            set
            {
                if (isQueryValueChanged != value)
                {
                    isQueryValueChanged = value;
                    OnPropertyChanged(nameof(IsQueryValueChanged));
                }
            }
        }
        // Template file path.
        public string FilePath { get; set; }
        // List of merge fields in the template.
        public List<string> MergeFields { get; set; }
        // Not used template.
        public bool Disabled { get; set; }
        // Excel sheet name used in the query.
        public string SheetName { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="file">Word file.</param>
        public SubTemplate(string file)
        {
            FileName = Path.GetFileName(file);
            FilePath = file;

            IsQueryValueChanged = false;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get the merge fields list and sheet name defined in query of the sub-template.
        /// </summary>
        /// <param name="wDoc">Word document.</param>
        public void GetParameters(Document wDoc)
        {
            if (wDoc != null)
            {
                MergeFields = GetMergeFields(wDoc);
                SheetName = GetSheetName(Query);
            }
        }

        /// <summary>
        /// Extract the Excel Sheetname defined in the query.
        /// </summary>
        /// <param name="query">Query string.</param>
        /// <returns>Excel sheet name or empty string if no Excel sheet defined in the query string.</returns>
        public static string GetSheetName(string query)
        {
            string sheetName = string.Empty;

            if (query.Contains("FROM"))
            {
                sheetName = Helpers.ToolBox.ExtractBetweenTwoStrings(query.Substring(query.IndexOf("FROM")), "`", "`", false, false);
                if (string.IsNullOrEmpty(sheetName))
                {
                    sheetName = Helpers.ToolBox.ExtractBetweenTwoStrings(query.Substring(query.IndexOf("FROM")), "[", "]", false, false);
                }
            }

            return sheetName;
        }


        /// <summary>
        /// Save the query in file property Comments.
        /// </summary>
        public void SaveQuery()
        {
            IsQueryValueChanged = !SetFileCommentsProperty(FilePath, Query);
        }

        /// <summary>
        /// Set file property Comments.
        /// </summary>
        /// <param name="file">File.</param>
        /// <param name="comments">Comments to be saved.</param>
        /// <returns>True if successfully saved.</returns>
        private bool SetFileCommentsProperty(string file, string comments)
        {
            try
            {
                Package docx = Package.Open(file, FileMode.Open, FileAccess.ReadWrite);

                docx.PackageProperties.Description = comments;
                
                docx.Close();

                return true;
            }
            catch (Exception ex)
            {
                _ = new Helpers.CustomException($"{string.Format(Properties.Resources.Exception_WriteFileProperty, Properties.Resources.FileProperty_Description, file)}{Environment.NewLine}{Environment.NewLine}{Properties.Resources.Exception_ErrorDetails}{Environment.NewLine}{ex.Message}", Properties.Resources.Exception_WriteFileProperties_Title);
                return false;
            }
        }

        /// <summary>
        /// Get the list of merge fields present in the docx file.
        /// </summary>
        /// <param name="document">Word document to parse.</param>
        /// <returns>List of all merge fields.</returns>
        private List<string> GetMergeFields(Document document)
        {
            List<string> mergeFields = new List<string>();
            // Merge field entire code.
            string fieldCodeText;
            // Merge field code without MERGEFIELD.
            string fieldCodeValue;
            // Merge field name.
            string mergeFieldName;
            // Position of first ' ' (space) in fieldCodeValue.
            int pos;

            foreach (Field field in document.Fields)
            {
                if (field.Type == WdFieldType.wdFieldMergeField)
                {
                    fieldCodeText = field.Code.Text.Trim();

                    if (fieldCodeText.StartsWith("MERGEFIELD", true, new CultureInfo("en-US")))
                    {
                        fieldCodeValue = fieldCodeText.Remove(0, "MERGEFIELD".Length).Trim();

                        if (fieldCodeValue.Length > 0)
                        {
                            pos = fieldCodeValue.IndexOf(' ');
                            mergeFieldName = pos > 0 ? fieldCodeValue.Substring(0, pos) : fieldCodeValue;
                            if (mergeFieldName.Length > 0 && !mergeFields.Contains(mergeFieldName))
                            {
                                mergeFields.Add(mergeFieldName);
                            }
                        }
                    }
                }
            }

            return mergeFields;
        }

        /// <summary>
        /// Check if the word merge fields are present in the Excel datasource fields (columns).
        /// </summary>
        /// <param name="sourceFields">List of the fields (columns) present in Excel datasource file.</param>
        /// <returns>List of non-matching word merge fields.</returns>
        public List<string> CheckMergeFields(Dictionary<string, string> sourceFields)
        {
            List<string> nonMatchingFields = new List<string>();

            foreach (string mergeField in MergeFields)
            {
                if (!sourceFields.Values.Contains(mergeField, StringComparer.InvariantCultureIgnoreCase))
                {
                    if (!nonMatchingFields.Contains(mergeField))
                    {
                        nonMatchingFields.Add(mergeField);
                    }
                }
            }

            return nonMatchingFields;
        }

        /// <summary>
        /// Check if the query fields are present in the Excel datasource fields (columns).
        /// </summary>
        /// <param name="sourceFields">List of the fields (columns) present in Excel datasource file.</param>
        /// <returns>List of non-matching fields.</returns>
        public List<string> CheckQueryFields(Dictionary<string, string> sourceFields)
        {
            char startSeparator = '[';
            char endSeparator = ']';

            // Find out wich kind of field separator is used in the query.
            if (query.IndexOf(startSeparator) == -1 && query.IndexOf(endSeparator) == -1)
            {
                startSeparator = '`';
                endSeparator = '`';

                if (query.IndexOf(startSeparator) == -1 && query.IndexOf(endSeparator) == -1)
                {
                    return null;
                }
            }

            // Build the list of fields used in the query.
            string[] rawFields;
            List<string> fields = new List<string>();

            if (query.IndexOf("where", StringComparison.InvariantCultureIgnoreCase) == -1)
            {
                // no Where clause in the query, search for fields after the From clause.
                rawFields = query.Substring(query.IndexOf("from", StringComparison.InvariantCultureIgnoreCase) + query.IndexOf(endSeparator.ToString(), StringComparison.InvariantCultureIgnoreCase)).Split(endSeparator);

            }
            else
            {
                // Where clause in the query, search for fields after the Where clause.
                rawFields = query.Substring(query.IndexOf("where", StringComparison.InvariantCultureIgnoreCase) + "where".Length).Split(endSeparator);
            }

            for (int i = 0; i < rawFields.Length; i++)
            {
                int index = rawFields[i].IndexOf(startSeparator);
                if (index > -1)
                {
                    if (!fields.Contains(rawFields[i].Substring(index + 1)))
                    {
                        fields.Add(rawFields[i].Substring(index + 1));
                    }
                }
            }

            // Build the list of fields not matching with the list of fields present in the Excel data source file.
            List<string> nonMatchingFields = new List<string>();
            foreach (string field in fields)
            {
                if (!sourceFields.Keys.Contains(field, StringComparer.InvariantCultureIgnoreCase))
                {
                    if (!nonMatchingFields.Contains(field))
                    {
                        nonMatchingFields.Add(field);
                    }
                }
            }

            return nonMatchingFields;
        }


        public override string ToString()
        {
            return $"[{nameof(SubTemplate)}: FileName={FileName}; FilePath={FilePath}; Query={Query}; IsQueryValueChanged={IsQueryValueChanged}; SheetName:{SheetName}; Disabled={Disabled}; MergeFields count:{(MergeFields != null ? MergeFields.Count() : 0)}";
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
