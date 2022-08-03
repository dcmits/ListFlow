using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using ListFlow.Models;
using System.IO;
using System.Collections.Generic;
using System.Xml.Linq;

namespace ListFlow.Views
{
    /// <summary>
    /// Interaction logic for WizardProcessReportView.xaml
    /// </summary>
    public partial class WizardProcessReportView : Window, INotifyPropertyChanged
    {
        #region Fields

        // FlowDocument for the screen.
        private FlowDocument screenFlowDoc;
        // FlowDocuement for the Clipboard.
        private FlowDocument clipboardFlowDoc;
        // True if Event details fields was mandatory or optional.
        private bool viewEventsDetails;
        // True if the Excel columns was renamed.
        private bool useRenameColumns;

        #endregion

        #region Command Routing

        public static readonly RoutedCommand CopyClipboardCommand = new RoutedCommand();
        public static readonly RoutedCommand CloseWindowCommand = new RoutedCommand();

        #endregion

        #region Constructors

        public WizardProcessReportView(FinalDocCreationSteps documentCreationEvent, bool viewEventsDetails, bool useRenameColumns)
        {
            InitializeComponent();

            _ = CommandBindings.Add(new CommandBinding(CopyClipboardCommand, CopyClipboardCommand_Executed, CopyClipboardCommand_CanExecuted));
            _ = CommandBindings.Add(new CommandBinding(CloseWindowCommand, CloseWindowCommand_Executed));

            this.viewEventsDetails = viewEventsDetails;
            this.useRenameColumns = useRenameColumns;

            ScreenFlowDoc = CreateFlowDocument(documentCreationEvent);
            clipboardFlowDoc = CreateFlowDocument(documentCreationEvent, true);
            DataContext = this;
        }

        #endregion

        #region Properties

        public FlowDocument ScreenFlowDoc
        {
            get => screenFlowDoc;
            set
            {
                if (screenFlowDoc != value)
                {
                    screenFlowDoc = value;
                    OnPropertyChanged(nameof(ScreenFlowDoc));
                }
            }
        }

        #endregion

        #region Methods

        private FlowDocument CreateFlowDocument(FinalDocCreationSteps docCreationEvents, bool clipboard = false)
        {
            FlowDocument flowDoc = new FlowDocument
            {
                FontFamily = new FontFamily("Calibri"),
                Foreground = FindResource("TextBoxForeground") as SolidColorBrush,
                Background = FindResource("TextBoxBackground") as SolidColorBrush,
                PageWidth = 808,
                FontSize = 12,
                TextAlignment = TextAlignment.Left
            };

            if (clipboard)
            {
                flowDoc.Foreground = FindResource("FlowDocumentForeground") as SolidColorBrush;
                flowDoc.Background = FindResource("FlowDocumentBackground") as SolidColorBrush;
            }

            // Title.
            Paragraph p = new Paragraph();
            if (!clipboard)
            {
                p.Inlines.Add(new InlineUIContainer(new Image()
                {
                    Height = 48,
                    Width = 48,
                    Margin = new Thickness(0, 0, 15, 0),
                    Source = FindResource("ListFlow_Large") as ImageSource
                })
                {
                    BaselineAlignment = BaselineAlignment.Center
                });
            }
            Run run = new Run(Properties.Resources.AppTitle)
            {
                FontSize = 36
            };
            p.Inlines.Add(run);
            run = new Run($"\t{typeof(WizardProcessReportView).Assembly.GetName().Version}")
            {
                FontSize = 12,
                FontWeight = FontWeights.Light
            };
            p.Inlines.Add(run);
            flowDoc.Blocks.Add(p);

            // Selected MainTemplate Details.
            p = new Paragraph
            {
                FontSize = 18,
                Margin = new Thickness(0),
                Padding = new Thickness(0)
            };
            run = new Run(Properties.Resources.FlowDoc_SelectedTemplateDetails)
            {
                FontWeight = FontWeights.SemiBold
            };
            p.Inlines.Add(run);
            flowDoc.Blocks.Add(p);

            // Create the table with 2 columns.
            Table table = new Table() { BorderThickness = new Thickness(0), BorderBrush = null };
            TableColumn tableColumn = new TableColumn
            {
                Width = new GridLength(200)
            };
            table.Columns.Add(tableColumn);
            table.Columns.Add(new TableColumn()
            {
                Width = new GridLength(575)
            });

            // Create the row group and add it to the table.
            TableRowGroup tableRowGroup = new TableRowGroup();
            table.RowGroups.Add(tableRowGroup);

            // Selected MainTemplate Title.
            tableRowGroup.Rows.Add(AddTemplateDetail(docCreationEvents, Properties.Resources.FlowDoc_Title, FinalDocCreationSteps.EntryCategory.TemplateTitle));
            // Selected MainTemplate Path.            
            tableRowGroup.Rows.Add(AddTemplateDetail(docCreationEvents, Properties.Resources.FlowDoc_File, FinalDocCreationSteps.EntryCategory.TemplatePath));
            // Selected MainTemplate Comment.            
            tableRowGroup.Rows.Add(AddTemplateDetail(docCreationEvents, Properties.Resources.FlowDoc_Comment, FinalDocCreationSteps.EntryCategory.TemplateComment));
            // Selected MainTemplate OptionalFieldsRequired.           
            tableRowGroup.Rows.Add(AddTemplateDetail(docCreationEvents, Properties.Resources.FlowDoc_OptionalFieldsRequired, FinalDocCreationSteps.EntryCategory.OptionalFieldsRequired));
            if (viewEventsDetails)
            {
                // Selected MainTemplate Event Title.            
                tableRowGroup.Rows.Add(AddTemplateDetail(docCreationEvents, Properties.Resources.FlowDoc_EventTitle, FinalDocCreationSteps.EntryCategory.EventTitle));
                // Selected MainTemplate Event Location.            
                tableRowGroup.Rows.Add(AddTemplateDetail(docCreationEvents, Properties.Resources.FlowDoc_EventLocation, FinalDocCreationSteps.EntryCategory.EventLocation));
                // Selected MainTemplate Event Date.            
                tableRowGroup.Rows.Add(AddTemplateDetail(docCreationEvents, Properties.Resources.FlowDoc_EventDate, FinalDocCreationSteps.EntryCategory.EventDate));
            }
            if (useRenameColumns)
            {
                // Original selected Excel File.            
                tableRowGroup.Rows.Add(AddTemplateDetail(docCreationEvents, Properties.Resources.FlowDoc_OriginalExcelFile, FinalDocCreationSteps.EntryCategory.ExcelFile));
                // Formated Excel File (with renamed columns).            
                tableRowGroup.Rows.Add(AddTemplateDetail(docCreationEvents, Properties.Resources.FlowDoc_ColumnsRenamedExcelFile, FinalDocCreationSteps.EntryCategory.FormatedExcelFile));
                // Keep the formated Excel File.            
                tableRowGroup.Rows.Add(AddTemplateDetail(docCreationEvents, Properties.Resources.FlowDoc_KeepRenamedExcelFile, FinalDocCreationSteps.EntryCategory.KeepFormatedExcelFile));
            }
            else
            {
                // Selected Excel File.            
                tableRowGroup.Rows.Add(AddTemplateDetail(docCreationEvents, Properties.Resources.FlowDoc_ExcelFile, FinalDocCreationSteps.EntryCategory.ExcelFile));
            }
            // Selected Sub-template Count.            
            tableRowGroup.Rows.Add(AddTemplateDetail(docCreationEvents, Properties.Resources.FlowDoc_SubTemplatesCount, FinalDocCreationSteps.EntryCategory.SubTemplatesCount));
            // Selected Sub-template Count.            
            tableRowGroup.Rows.Add(AddTemplateDetail(docCreationEvents, Properties.Resources.FlowDoc_CreationDateTime, FinalDocCreationSteps.EntryCategory.CreationDateTime));

            // Add the table to the FlowDocument.
            flowDoc.Blocks.Add(table);

            // Add disabled sub-templates list if not empty.
            if (docCreationEvents.GetDisabledSubTemplateEntries().Count > 0)
            {
                // Disabled sub-templates list title.
                p = new Paragraph
                {
                    FontSize = 18,
                    Margin = new Thickness(0, 10, 0, 0)
                };
                run = new Run(Properties.Resources.FlowDoc_DisabledSubTemplateList)
                {
                    FontWeight = FontWeights.SemiBold
                };
                p.Inlines.Add(run);
                flowDoc.Blocks.Add(p);

                // Disabled sub-templates list items.
                List disabledList = new List();
                foreach (Entry entry in docCreationEvents.GetDisabledSubTemplateEntries())
                {
                    ListItem listItem = new ListItem();
                    p = new Paragraph
                    {
                        FontSize = 14,
                        FontWeight = FontWeights.Light,
                        Foreground = FindResource("TextErrorForegroundBrush") as SolidColorBrush
                    };
                    run = new Run(entry.Message);
                    p.Inlines.Add(run);
                    listItem.Blocks.Add(p);
                    disabledList.ListItems.Add(listItem);
                }
                flowDoc.Blocks.Add(disabledList);
            }

            // Entries title.
            p = new Paragraph
            {
                FontSize = 18,
                Margin = new Thickness(0, 10, 0, 0)
            };
            run = new Run(Properties.Resources.FlowDoc_Steps)
            {
                FontWeight = FontWeights.SemiBold
            };
            p.Inlines.Add(run);
            flowDoc.Blocks.Add(p);

            List list = new List();
            foreach (Entry entry in docCreationEvents.GetEntries())
            {
                ListItem listItem = new ListItem();
                p = new Paragraph
                {
                    FontSize = 14,
                    FontWeight = FontWeights.Light
                };
                switch (entry.EntryType)
                {
                    case FinalDocCreationSteps.EntryType.Information:
                        p.Foreground = clipboard ? flowDoc.Foreground = FindResource("FlowDocumentForeground") as SolidColorBrush : FindResource("TextForegroundBrush") as SolidColorBrush;

                        run = new Run(entry.Message);
                        p.Inlines.Add(run);

                        break;
                    case FinalDocCreationSteps.EntryType.Result:
                        p.Foreground = FindResource("TextResultForegroundBrush") as SolidColorBrush;

                        run = new Run(entry.Message);
                        p.Inlines.Add(run);

                        break;
                    case FinalDocCreationSteps.EntryType.Warning:
                        p.Foreground = FindResource("TextWarningForegroundBrush") as SolidColorBrush;

                        run = new Run(entry.Message);
                        p.Inlines.Add(run);

                        break;
                    case FinalDocCreationSteps.EntryType.Error:
                        p.Foreground = FindResource("TextErrorForegroundBrush") as SolidColorBrush;

                        run = new Run(entry.Message);
                        p.Inlines.Add(run);

                        break;
                    case FinalDocCreationSteps.EntryType.StartProcessing:
                        p.Foreground = clipboard ? flowDoc.Foreground = FindResource("StartEndProcessPrintForegroundBrush") as SolidColorBrush : FindResource("StartEndProcessScreenForegroundBrush") as SolidColorBrush;
                        //p.Foreground = clipboard ? FindResource("StartEndProcessPrintForegroundBrush") as SolidColorBrush : FindResource("StartEndProcessScreenForegroundBrush") as SolidColorBrush;
                        p.FontSize = 16;
                        p.Margin = new Thickness(0, 8, 0, 0);
                        p.FontStyle = FontStyles.Italic;
                        FontWeight = FontWeights.SemiBold;

                        run = new Run(entry.Message);
                        p.Inlines.Add(run);

                        break;
                    case FinalDocCreationSteps.EntryType.EndProcessing:
                        p.Foreground = clipboard ? flowDoc.Foreground = FindResource("StartEndProcessPrintForegroundBrush") as SolidColorBrush : FindResource("StartEndProcessScreenForegroundBrush") as SolidColorBrush;
                        //p.Foreground = clipboard ? FindResource("StartEndProcessPrintForegroundBrush") as SolidColorBrush : FindResource("StartEndProcessScreenForegroundBrush") as SolidColorBrush;
                        p.FontSize = 16;
                        p.Margin = new Thickness(0, 0, 0, 8);
                        p.FontStyle = FontStyles.Italic;
                        FontWeight = FontWeights.SemiBold;

                        run = new Run(entry.Message);
                        p.Inlines.Add(run);

                        break;
                    case FinalDocCreationSteps.EntryType.SqlSyntax:
                        // List of styles and contents composing the message.
                        List<TextStyling> textStylings = ParseTextStyling(entry.Message);

                        // Applies the style defined in the message resource.
                        foreach (TextStyling item in textStylings)
                        {
                            Run runSqlSyntax = new Run(item.Text);

                            Brush brush = clipboard ?  FindResource($"{item.Styling}Clipboard") as SolidColorBrush : TryFindResource(item.Styling) as SolidColorBrush;
                            if (item.Styling.Contains("Background"))
                            {
                                runSqlSyntax.Background = brush != null ? brush : clipboard ? FindResource("TextErrorBackgroundBrushClipboard") as SolidColorBrush : FindResource("TextErrorBackgroundBrush") as SolidColorBrush;
                                runSqlSyntax.Foreground = clipboard ? FindResource("TextForegroundBrushClipboard") as SolidColorBrush : FindResource("TextForegroundBrush") as SolidColorBrush;
                            }
                            else
                            {
                                runSqlSyntax.Foreground = brush != null ? brush : clipboard ? FindResource("TextForegroundBrushClipboard") as SolidColorBrush : FindResource("TextForegroundBrush") as SolidColorBrush;
                            }
                            p.Inlines.Add(runSqlSyntax);
                        }

                        break;
                    default:
                        break;
                }
                listItem.Blocks.Add(p);
                list.ListItems.Add(listItem);
            }
            flowDoc.Blocks.Add(list);

            // Process duration.
            p = new Paragraph()
            {
                FontSize = 16,
                Margin = new Thickness(0, 10, 0, 0),
                FontWeight = FontWeights.SemiBold,
                Foreground = FindResource("TextWarningForegroundBrush") as SolidColorBrush
            };
            p.Inlines.Add(new Run(docCreationEvents.GetEntry(FinalDocCreationSteps.EntryType.Warning, FinalDocCreationSteps.EntryCategory.ProcessDuration)));
            flowDoc.Blocks.Add(p);

            return flowDoc;
        }

        private TableRow AddTemplateDetail(FinalDocCreationSteps docCreationEvents, string itemTitle, FinalDocCreationSteps.EntryCategory entryCategory)
        {
            TableRow tableRow = new TableRow();

            Paragraph p = new Paragraph(new Run(itemTitle))
            {
                FontSize = 14,
                FontWeight = FontWeights.DemiBold
            };
            tableRow.Cells.Add(new TableCell(p));

            p = new Paragraph
            {
                FontSize = 14,
                FontWeight = FontWeights.Normal
            };
            p.Inlines.Add(new Run()
            {
                Text = docCreationEvents.GetEntry(FinalDocCreationSteps.EntryType.Information, entryCategory)
            });
            tableRow.Cells.Add(new TableCell(p));

            return tableRow;
        }

        private List<TextStyling> ParseTextStyling(string content)
        {
            List<TextStyling> textStylings = new List<TextStyling>();

            foreach (XElement xElement in XDocument.Parse($"<root>{content}</root>").Element("root").Elements())
            {
                // SQL queries are embedded as a XML comment <!-- --> in the element because they may contain special characters that are not supported.
                if (string.IsNullOrEmpty(xElement.Value.ToString()))
                {
                    textStylings.Add(new TextStyling(((XComment)xElement.LastNode).Value, xElement.Name.LocalName));
                }
                else
                {
                    textStylings.Add(new TextStyling(xElement.Value, xElement.Name.LocalName));
                }
            }

            return textStylings;
        }

        #endregion

        #region Events

        private void CopyClipboardCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TextRange range = new TextRange(clipboardFlowDoc.ContentStart, clipboardFlowDoc.ContentEnd);

            using (MemoryStream stream = new MemoryStream())
            {
                range.Save(stream, DataFormats.Rtf);
                Clipboard.SetData(DataFormats.Rtf, Encoding.UTF8.GetString(stream.ToArray()));
            }
        }

        private void CopyClipboardCommand_CanExecuted(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = clipboardFlowDoc.Blocks.Count > 0;
        }

        private void CloseWindowCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        #region Properties Change (Events)

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion

        #endregion
    }

    public class TextStyling
    {
        public string Text { get; set; }
        public string Styling { get; set; }

        public TextStyling(string text, string styling)
        {
            Text = text;
            Styling = styling;
        }
    }
}
