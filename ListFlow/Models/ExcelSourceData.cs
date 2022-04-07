using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.ComponentModel;
using System.Text;
using Microsoft.Office.Interop.Excel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Media;

namespace ListFlow.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class ExcelData : INotifyPropertyChanged
    {
        #region Fields

        private string filePath;
        private string formatedFilePath;
        private Dictionary<string, string> columnFieldNames;
        private string sheetName;

        // Excel Application.
        private Application xApp;

        // Interop missing value.
        private readonly object oMissing = Missing.Value;
        // Interop true value.
        private readonly object oTrue = true;
        // Interop false value.
        private readonly object oFalse = false;

        #endregion

        #region Properties

        public string LastInitialDirectory { get; set; }

        /// <summary>
        /// Excel data source file path.
        /// </summary>
        public string FilePath
        {
            get => filePath;
            set
            {
                if (filePath != value)
                {
                    filePath = value;
                    LastInitialDirectory = Path.GetDirectoryName(filePath);
                    OnPropertyChanged(nameof(FilePath));
                }
            }
        }

        /// <summary>
        /// Formated Excel data source file path.
        /// </summary>
        public string FormatedFilePath
        {
            get => formatedFilePath;
            set
            {
                if (formatedFilePath != value)
                {
                    formatedFilePath = value;
                    OnPropertyChanged(nameof(FormatedFilePath));
                }
            }
        }

        /// <summary>
        /// List of column names (original and optimized) present in the Excel table defined as data source with their Word Mail Merge syntax matches.
        /// </summary>
        public Dictionary<string, string> ColumnFieldNames
        {
            get => columnFieldNames;
            set
            {
                if (columnFieldNames != value)
                {
                    columnFieldNames = value;

                    OnPropertyChanged(nameof(ColumnFieldNames));
                }
            }
        }

        public string SheetName
        {
            get => sheetName;
            set
            {
                if (sheetName != value)
                {
                    sheetName = value;

                    OnPropertyChanged(nameof(SheetName));
                }
            }
        }
    
        #endregion

        #region Methods

        /// <summary>
        /// Connect to the data source (Excel Sheet), 
        /// Format the columns names of the first Sheet to optimize the size of the merge field names,
        /// Get all column names.
        /// </summary>
        /// <returns>True if no error.</returns>
        public bool Connect(bool renameColumns, string columnForceToSplit)
        {
            bool result;

            if (StartExcel(false))
            {
                SheetName = renameColumns ? FormatFileContent(columnForceToSplit) : CheckFile();

                if (string.IsNullOrEmpty(SheetName))
                {
                    _ = new Helpers.CustomException(string.Format(Properties.Resources.Exception_NoExcelSheet, filePath), Properties.Resources.Exception_ConnectSource_Title);

                    result = false;
                }
                else
                {
                    result = true;
                }

                CloseExcel();
            }
            else
            {
                _ = new Helpers.CustomException(Properties.Resources.Exception_LaunchExcel, Properties.Resources.Exception_ConnectSource_Title);

                result = false;
            }

            return result;
        }

        public void Disconnect()
        {
            try
            {
                if (string.Compare(Path.GetDirectoryName(filePath), Path.GetDirectoryName(formatedFilePath)) == 0)
                {
                    File.Delete(formatedFilePath);
                }
                else
                {
                    Directory.Delete(Path.GetDirectoryName(formatedFilePath), true);
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Start a Excel instance or connect to a existing instance.
        /// </summary>
        /// <param name="visible">True: Excel will be visible, False: Excel will be hidden during the converting.</param>
        private bool StartExcel(bool visible)
        {
            CloseHiddenExcelInstancies();

            try
            {
                if (xApp == null)
                {
                    xApp = (Application)Activator.CreateInstance(Type.GetTypeFromProgID("Excel.Application"));
                    xApp.Visible = visible;
                }

                return true;
            }
            catch (COMException)
            {
                return false;
            }
            catch (Exception ex)
            {
                _ = new Helpers.CustomException(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Close all hidden Excel instancies.
        /// </summary>
        public void CloseHiddenExcelInstancies()
        {
            try
            {
                List<Process> processes = Process.GetProcesses().ToList().Where(x => x.ProcessName == "EXCEL" && string.IsNullOrEmpty(x.MainWindowTitle)).ToList();
                foreach (Process p in processes)
                {
                    p.Kill();
                }
            }
            catch (Exception ex)
            {
                _ = new Helpers.CustomException(ex.Message);
            }
        }

        /// <summary>
        /// Copy the original Excel file to a temp folder,
        /// Open the Excel File,
        /// Split the name and the content, in two different column, the column who's content a '/' char in column name and in all data rows,
        /// Get all column names (original, optimized and merge field formated name).
        /// </summary>
        /// <returns>Sheet name or empty string.</returns>
        private string FormatFileContent(string columnForceToSplit)
        {
            string dataSheetName = string.Empty;

            FormatedFilePath = CopySourceFileToTemp(filePath, true);

            if (!string.IsNullOrEmpty(formatedFilePath))
            {
                Workbook xWorkbook = xApp.Workbooks.Open(formatedFilePath, oFalse, oFalse, oMissing, oMissing, oMissing, oTrue, oMissing, oMissing, oMissing, oFalse, oMissing, oFalse, oMissing, oMissing);
                Worksheet xSheet = xWorkbook.Worksheets[1];
                Range xRange = xSheet.Cells.SpecialCells(XlCellType.xlCellTypeLastCell, oMissing);

                dataSheetName = xSheet.Name;

                // Create the list of column to being formated/splitted.
                List<int> columnsToBeSplited = new List<int>();
                for (int col = 1; col <= xRange.Column; col++)
                {
                    if (xApp.WorksheetFunction.CountIf(xSheet.Columns[col], "*/*") == xRange.Row && xApp.WorksheetFunction.CountIf(xSheet.Columns[col], "*/*/*") != (xRange.Row - 1))
                    {
                        columnsToBeSplited.Add(col);

                    }
                    else if (xSheet.Cells[1, col].Value == columnForceToSplit)
                    {
                        columnsToBeSplited.Add(col);
                    }
                }

                // Split containt in two diffent column (if separator exist).
                for (int col = columnsToBeSplited.Count - 1; col >= 0; col--)
                {
                    // Insert a new empty column after the column to being splited.
                    xSheet.Columns[columnsToBeSplited[col] + 1].Insert(XlInsertShiftDirection.xlShiftToRight, oMissing);

                    // Split containt in two diffent column (if separator exist).
                    object[,] originalColumn = xSheet.Range[xSheet.Cells[1, columnsToBeSplited[col]], xSheet.Cells[xRange.SpecialCells(XlCellType.xlCellTypeLastCell).Row, columnsToBeSplited[col]]].Value2;
                    object[,] newColumn = xSheet.Range[xSheet.Cells[1, columnsToBeSplited[col] + 1], xSheet.Cells[xRange.SpecialCells(XlCellType.xlCellTypeLastCell).Row, columnsToBeSplited[col] + 1]].Value2;

                    for (int row = 1; row <= xRange.Row; row++)
                    {
                        if (originalColumn[row, 1].ToString().Contains("/"))
                        {
                            string[] content = originalColumn[row, 1].ToString().Split('/');

                            if (content.Length == 2)
                            {
                                if (string.IsNullOrEmpty(content[0].Trim()) && string.IsNullOrEmpty(content[1].Trim()))
                                {
                                    newColumn[row, 1] = originalColumn[row, 1];
                                }
                                else
                                {
                                    originalColumn[row, 1] = string.IsNullOrEmpty(content[0].Trim()) ? content[1].Trim() : content[0].Trim();
                                    newColumn[row, 1] = string.IsNullOrEmpty(content[1].Trim()) ? content[0].Trim() : content[1].Trim();
                                }
                            }
                            else if (content.Length > 2 || content.Length == 1)
                            {
                                newColumn[row, 1] = row == 1 ? $"{originalColumn[row, 1]}_1" : originalColumn[row, 1];
                            }
                        }
                        else
                        {
                            newColumn[row, 1] = row == 1 ? $"{originalColumn[row, 1]}_1" : originalColumn[row, 1];
                        }
                    }

                    // Write the updated content to the two column.
                    xSheet.Range[xSheet.Cells[1, columnsToBeSplited[col]], xSheet.Cells[xRange.SpecialCells(XlCellType.xlCellTypeLastCell).Row, columnsToBeSplited[col]]].Value2 = originalColumn;
                    xSheet.Range[xSheet.Cells[1, columnsToBeSplited[col] + 1], xSheet.Cells[xRange.SpecialCells(XlCellType.xlCellTypeLastCell).Row, columnsToBeSplited[col] + 1]].Value2 = newColumn;
                }

                // Insert a new column at left of the sheet.
                xSheet.Columns[1].Insert(XlInsertShiftDirection.xlShiftToRight, oMissing);
                // Default value for this new column.
                xSheet.Range[$"A2:A{xRange.Row}"].Value = 0;
                // Insert a new row, before the first row, and name's the columns, to save the original column names in the second row.
                xSheet.Rows[1].Insert(XlInsertShiftDirection.xlShiftDown, oMissing);
                // Flag the row containing the original column names.            
                xSheet.Cells[2, 1].Value2 = 1;

                if (columnFieldNames is null)
                {
                    columnFieldNames = new Dictionary<string, string>();
                }
                else
                {
                    columnFieldNames.Clear();
                }

                // Name column names.
                for (int col = 1; col <= xRange.Column; col++)
                {
                    xSheet.Cells[1, col].Value = $"C{col - 1}";

                    // Create a list of the column names(original and new field names).
                    if (columnFieldNames.ContainsKey(Convert.ToString(xSheet.Cells[2, col].Value)))
                    {
                        int i = 0;
                        do
                        {
                            i++;
                        } while (columnFieldNames.ContainsKey($"{Convert.ToString(xSheet.Cells[2, col].Value)} {i}"));

                        columnFieldNames.Add($"{Convert.ToString(xSheet.Cells[2, col].Value)} {i}", Convert.ToString(xSheet.Cells[1, col].Value));
                    }
                    else
                    {
                        columnFieldNames.Add(Convert.ToString(xSheet.Cells[2, col].Value), Convert.ToString(xSheet.Cells[1, col].Value));
                    }
                }

                xWorkbook.Save();
                xWorkbook.Close(oFalse, oMissing, oMissing);

                _ = Marshal.ReleaseComObject(xRange);
                _ = Marshal.ReleaseComObject(xSheet);
                _ = Marshal.ReleaseComObject(xWorkbook);
            }

            return dataSheetName;
        }

        /// <summary>
        /// Copy the source file to a temp folder.
        /// </summary>
        /// <param name="source">File path to be copied.</param>
        /// <returns>If success return temp file path, if not an empty string.</returns>
        private string CopySourceFileToTemp(string source, bool local = false)
        {
            string tmpDir = local ? Path.GetDirectoryName(source) : Helpers.ToolBox.GetTempDirectory();
            string finalPath = string.Empty;

            try
            {
                File.Copy(source, Path.Combine(tmpDir, $"ListFlow_{Path.GetFileName(source)}"), true);
                finalPath = Path.Combine(tmpDir, $"ListFlow_{Path.GetFileName(source)}");
            }
            catch (Exception ex)
            {
                _ = new Helpers.CustomException(string.Format($"{string.Format(Properties.Resources.Exception_CopyExcelSourceToTemp, ex.Message, source, tmpDir)}{Environment.NewLine}{Environment.NewLine}{Properties.Resources.Exception_ErrorDetails}{Environment.NewLine}{ex.Message}"), Properties.Resources.Exception_CopySourceFile_Title);
            }
            
            return finalPath;
        }

        /// <summary>
        /// Formats the name of the Word field according to the guidelines.
        /// Max 40 characters, Removal of special characters and Replacement of spaces with _.
        /// </summary>
        /// <param name="fieldName">Field name to be formated.</param>
        /// <returns>Formated field name.</returns>
        private string FormatMergeFieldName(string fieldName)
        {
            StringBuilder sb = new StringBuilder(fieldName);
            if (sb.Length > 40)
            {
                _ = sb.Remove(40, sb.Length - 40);
            }

            // Remove +"*%&/()=?$£[]{},;.:<>\@#€|¬-°§¦'´~^’" in the field name.
            foreach (char c in new char[] { '+', '"', '*', '%', '&', '/', '(', ')', '?', '$', '£', '[', ']', '{', '}', ',', ';', '.', ':', '<', '>', '\\', '@', '#', '€', '|', '¢', '¬', '-', '°', '§', '¦', '\'', '´', '~', '^', '’' })
            {
                _ = sb.Replace(c.ToString(), "");
            }
            _ = sb.Replace(' ', '_');

            return sb.ToString().ToLowerInvariant();
        }

        private string CheckFile()
        {
            string dataSheetName = string.Empty;

            if (!string.IsNullOrEmpty(filePath))
            {
                Workbook xWorkbook = xApp.Workbooks.Open(filePath, oFalse, oFalse, oMissing, oMissing, oMissing, oTrue, oMissing, oMissing, oMissing, oFalse, oMissing, oFalse, oMissing, oMissing);
                Worksheet xSheet = xWorkbook.Worksheets[1];
                Range xRange = xSheet.Cells.SpecialCells(XlCellType.xlCellTypeLastCell, oMissing);

                dataSheetName = xSheet.Name;

                if (columnFieldNames is null)
                {
                    columnFieldNames = new Dictionary<string, string>();
                }
                else
                {
                    columnFieldNames.Clear();
                }

                // TODO: Check if column name already exist, if yes, inform user on this issues.

                for (int col = 1; col <= xRange.Column; col++)
                {
                    columnFieldNames.Add(Convert.ToString(xSheet.Cells[1, col].Value), FormatMergeFieldName(Convert.ToString(xSheet.Cells[1, col].Value)));
                }
                xWorkbook.Close(oFalse, oMissing, oMissing);

                _ = Marshal.ReleaseComObject(xRange);
                _ = Marshal.ReleaseComObject(xSheet);
                _ = Marshal.ReleaseComObject(xWorkbook);
            }

            FormatedFilePath = filePath;

            return dataSheetName;
        }

        /// <summary>
        /// Close current hidden Excel instance.
        /// </summary>
        private void CloseExcel()
        {
            try
            {
                if (xApp != null && !xApp.Visible)
                {
                    foreach (Workbook item in xApp.Workbooks)
                    {
                        item.Close(oFalse, oMissing, oMissing);
                    }
                    xApp.Quit();
                    _ = Marshal.ReleaseComObject(xApp);
                    xApp = null;

                    // To ensure that the Excel instance is terminated correctly.
                    CloseHiddenExcelInstancies();
                }
            }
            catch (COMException ex)
            {
                _ = new Helpers.CustomException(ex.Message);
            }
            catch (Exception ex)
            {
                _ = new Helpers.CustomException(ex.Message);
            }
        }

        public override string ToString()
        {
            return $"[{nameof(ExcelData)}: FilePath={FilePath}; formatedFilePath={formatedFilePath}; SheetName:{SheetName}; ColumnFieldNames count:{columnFieldNames.Count}";
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
