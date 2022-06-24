using System.Collections.Generic;
using System.Linq;

namespace ListFlow.Models
{
    public class FinalDocCreationSteps
    {
        #region Fields

        #endregion

        #region Properties

        public List<Entry> Entries { get; set; }

        #endregion

        #region Enums

        public enum EntryType
        {
            Information,
            Result,
            Warning,
            StartProcessing,
            EndProcessing,
            Error,
            SqlSyntax
        }

        public enum EntryCategory
        {
            TemplateTitle,
            TemplatePath,
            TemplateComment,
            OptionalFieldsRequired,
            EventTitle,
            EventLocation,
            EventDate,
            ExcelFile,
            FormatedExcelFile,
            KeepFormatedExcelFile,
            SubTemplatesCount,
            CreationDateTime,
            ProcessDuration,
            DisabledSubTemplateLoggingTitle,
            DisabledSubTemplateItem,
            Entry
        }
        #endregion

        #region Constructors

        public FinalDocCreationSteps()
        {
            Entries = new List<Entry>();
        }

        #endregion

        #region Methods

        public void Add(EntryType entryType, EntryCategory entryCategory, string message)
        {
            Entries.Add(new Entry(entryType, entryCategory, message));
        }

        public void Add(EntryType entryType, string message)
        {

            Entries.Add(new Entry(entryType, message));
        }

        public void Add(string message)
        {
            Entries.Add(new Entry(message));
        }

        public string GetEntry(EntryType entryType, EntryCategory entryCategory)
        {
            return Entries.FirstOrDefault(x => x.EntryType == entryType && x.EntryCategory == entryCategory).Message != null ? 
                Entries.FirstOrDefault(x => x.EntryType == entryType && x.EntryCategory == entryCategory).Message : 
                string.Empty;
        }

        public List<Entry> GetEntries()
        {
            return Entries.Where(x => x.EntryCategory == EntryCategory.Entry).ToList();
        }

        public List<Entry> GetDisabledSubTemplateEntries()
        {
            return Entries.Where(x => x.EntryCategory == EntryCategory.DisabledSubTemplateItem).ToList();
        }

        #endregion
    }

    public class Entry
    {
        #region Properties

        public FinalDocCreationSteps.EntryType EntryType { get; set; }
        public FinalDocCreationSteps.EntryCategory EntryCategory { get; set; }
        public string Message { get; set; }

        #endregion

        #region Constructors

        public Entry(FinalDocCreationSteps.EntryType entryType, FinalDocCreationSteps.EntryCategory entryCategory, string message)
        {
            EntryType = entryType;
            EntryCategory = entryCategory;
            Message = message;
        }

        public Entry(FinalDocCreationSteps.EntryType entryType, string message)
        {
            EntryType = entryType;
            EntryCategory = FinalDocCreationSteps.EntryCategory.Entry;
            Message = message;
        }

        public Entry(string message)
        {
            EntryType = FinalDocCreationSteps.EntryType.Information;
            EntryCategory = FinalDocCreationSteps.EntryCategory.Entry;
            Message = message;
        }

        #endregion
    }
}
