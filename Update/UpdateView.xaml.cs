using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.XPath;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using System.IO;

namespace Update
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class UpdateView : Window, INotifyPropertyChanged
    {
        #region Fields

        // Dossier de l'application SeatFlow.
        private string destinationPath;
        // Dossier contenant les fichiers sources pour les mises à jours.
        private string sourcePath;
        // Texte affiché à l'attention de l'utilisateur.
        private string infoMessage;
        // Icon de la progression des étapes de mise à jour.
        private DrawingImage infoIcon;
        // Paramètres de la mise à jour.
        private Version version;

        #endregion

        #region Command Routing

        public static readonly RoutedCommand CloseWindowCommand = new RoutedCommand();
        public static readonly RoutedCommand UpdateCommand = new RoutedCommand();
        public static readonly RoutedCommand UpdateQuitCommand = new RoutedCommand();

        #endregion

        #region Properties

        // Liste des actions effectuées lors de la mise à jour.
        public ObservableCollection<Action> Actions { get; set; }
        // Message d'information à l'attention del'utilisateur.
        public string InfoMessage
        {
            get => infoMessage;
            set
            {
                if (infoMessage != value)
                {
                    infoMessage = value;
                    OnPropertyChanged(nameof(InfoMessage));
                }
            }
        }
        // Icone qui illustre le message d'information à l'attention del'utilisateur.
        public DrawingImage InfoIcon
        {
            get => infoIcon;
            set
            {
                if (infoIcon != value)
                {
                    infoIcon = value;
                    OnPropertyChanged(nameof(InfoIcon));
                }
            }
        }

        #endregion

        #region Constants

        private const string VersionXmlFileName = "seatflowupdate.xml";
        private const string ReleaseNoteFileName = "ReleaseNotes.docx";
        private const string BackupDataFolder = "Backups";
        private const string ConfigFileElementName = "/configuration/applicationSettings/SeatFlow.Properties.Settings/setting/value";
        private const string ConfigFileName = "SeatFlow.exe.config";
        private const string BackupFileExt = ".bak";
        private const string XSDPathAttributeName = "noNamespaceSchemaLocation";
        private const string DataFolderName = "SeatFlowData";
        private const string SchemaFileName = "SeatFlow.xsd";

        #endregion

        #region Constructors

        public UpdateView()
        {
            InitializeComponent();

            Actions = new ObservableCollection<Action>();
            dgwUpdate.ItemsSource = Actions;
            InfoIcon = null;
            InfoMessage = string.Empty;

            DataContext = this;

            _ = CommandBindings.Add(new CommandBinding(CloseWindowCommand, CloseWindowCommand_Executed, CloseWindowCommand_CanExecute));
            _ = CommandBindings.Add(new CommandBinding(UpdateCommand, UpdateCommand_Executed, UpdateCommand_CanExecute));
            _ = CommandBindings.Add(new CommandBinding(UpdateQuitCommand, UpdateQuitCommand_Executed, UpdateQuitCommand_CanExecute));


            SetButtonStatus(!LoadParameters());
        }

        private void UpdateQuitCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void UpdateQuitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void UpdateCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void UpdateCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void CloseWindowCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Charge les paramètres de la mise à jour depuis le fichier SeatFlowUpdate.xml.
        /// </summary>
        /// <returns>True si toutes les conditions sont réunies pour pouoir effectuer la mise à jour, false dans le cas contraire.</returns>
        private bool LoadParameters()
        {
            // Compose les chemins d'accès aux sources et destination de la mise à jour.
            sourcePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            destinationPath = Directory.GetParent(sourcePath).FullName; 

            // Fichier XML des paramètres pour la mise à jour.
            string xmlFilePath = Path.Combine(sourcePath, VersionXmlFileName);

            if (File.Exists(xmlFilePath))
            {
                try
                {
                    // Charge les paramètres depuis le fichier XML.                
                    XDocument xDoc = XDocument.Load(Path.Combine(sourcePath, VersionXmlFileName));
                    version = DeserializeObject<Version>($"/Version", xDoc);

                    // Vérifie que la version actuellement installée corresponde à la version prévue.
                    if (File.Exists(Path.Combine(destinationPath, "SeatFlow.exe")))
                    {
                        string exeCurrentVersion = FileVersionInfo.GetVersionInfo(Path.Combine(destinationPath, "SeatFlow.exe")).FileVersion;

                        if (string.Compare(version.Before, exeCurrentVersion) == 0)
                        {
                            InfoMessage = string.Format(Properties.Resources.Info_BeforeUpdate, Properties.Resources.Button_Update.Replace("_", string.Empty), version.After);
                            InfoIcon = FindResource("Information_Large") as DrawingImage;
                        }
                        else
                        {
                            // La version courante ne correspond pas à la version attendue.
                            InfoMessage = new StringBuilder().AppendLine(string.Format(Properties.Resources.Info_WrongVersion, exeCurrentVersion, version.Before)).AppendLine().
                                AppendLine($"{Properties.Resources.Info_UpdateCannotBePerformed}").
                                Append($"{Properties.Resources.Info_ContactSupport}").ToString();
                            InfoIcon = FindResource("AlertError_Large") as DrawingImage;

                            return false;
                        }
                    }
                    else
                    {
                        // Si SeatFlow.exe n'existe pas dans le dossier, interdit la mise à jour.
                        InfoMessage = new StringBuilder().AppendLine(string.Format(Properties.Resources.Info_AppNotFound, destinationPath)).AppendLine().
                            AppendLine($"{Properties.Resources.Info_UpdateCannotBePerformed}").
                            Append($"{Properties.Resources.Info_ContactSupport}").ToString();
                        InfoIcon = FindResource("AlertError_Large") as DrawingImage;

                        return false;
                    }
                }
                catch (Exception ex)
                {
                    InfoMessage = new StringBuilder().AppendLine($"{Properties.Resources.Info_XMLParse}").
                        AppendLine($"{Properties.Resources.Info_Exception} {ex.Message}").
                        AppendLine($"{Properties.Resources.Info_XMLFilePath} {xmlFilePath}").AppendLine().
                        AppendLine($"{Properties.Resources.Info_UpdateCannotBePerformed}").
                        Append($"{Properties.Resources.Info_ContactSupport}").ToString();
                    InfoIcon = FindResource("AlertError_Large") as DrawingImage;

                    return false;
                }            
            }
            else
            {
                InfoMessage = new StringBuilder().AppendLine($"{Properties.Resources.Info_XMLFileUnavailable}").
                    AppendLine($"{Properties.Resources.Info_XMLFilePath} {xmlFilePath}").AppendLine().
                    AppendLine($"{Properties.Resources.Info_UpdateCannotBePerformed}").
                    Append($"{Properties.Resources.Info_ContactSupport}").ToString();
                InfoIcon = FindResource("AlertError_Large") as DrawingImage;

                return false;
            }

            return true;
        }
       
        /// <summary>
        /// Remplace les fichiers par leurs nouvelles version.
        /// </summary>
        /// <returns>True si tous les fichiers ont été remplacé, False en cas d'erreur.</returns>
        private bool CopyFiles(string dataPath)
        {
            bool result = true;

            string fileName = string.Empty;

            // Fichiers contenu dans la liste.
            foreach (Version.File file in version.Files)
            {
                if (!file.NotToUpdate)
                {
                    try
                    {
                        if (file.Name.StartsWith($"[{DataFolderName}]", StringComparison.OrdinalIgnoreCase))
                        {
                            // Remplace/copie un fichier de données (data).
                            fileName = Path.Combine(dataPath, file.Name.Replace($"[{DataFolderName}]/", string.Empty));
                            File.Copy(Path.Combine(sourcePath, file.Name.Replace($"[{DataFolderName}]", DataFolderName)), fileName, true);
                        }
                        else
                        {
                            // Remplace/copie un fichier de l'application (exe, dll et autre).
                            fileName = file.Name;
                            File.Copy(Path.Combine(sourcePath, fileName), Path.Combine(destinationPath, fileName), true);
                        }
                        Actions.Add(new Action(string.Format(Properties.Resources.Action_ReplaceFile, fileName), Action.Status.Ended));
                    }
                    catch (Exception ex)
                    {
                        Actions.Add(new Action(string.Format(Properties.Resources.Action_ReplaceFileError, fileName)));
                        Actions.Add(new Action(string.Format(Properties.Resources.Action_ErrorCode, ex.Message)));
                        Actions.Add(new Action(Properties.Resources.Action_UpdateAborted));
                        Actions.Add(new Action(Properties.Resources.Info_ContactSupport));

                        result = false;
                        break;
                    }
                }
            }

            // Fichier ReleaseNotes.
            try
            {
                File.Copy(Path.Combine(sourcePath, ReleaseNoteFileName), Path.Combine(destinationPath, ReleaseNoteFileName), true);
                Actions.Add(new Action(string.Format(Properties.Resources.Action_ReplaceFile, ReleaseNoteFileName), Action.Status.Ended));
            }
            catch (Exception ex)
            {
                Actions.Add(new Action(string.Format(Properties.Resources.Action_ReplaceFileError, ReleaseNoteFileName)));
                Actions.Add(new Action(string.Format(Properties.Resources.Action_ErrorCode, ex.Message)));
                Actions.Add(new Action(Properties.Resources.Action_UpdateAborted));
                Actions.Add(new Action(Properties.Resources.Info_ContactSupport));

                result = false;
            }

            return result;
        }


        /// <summary>
        /// Mise à jour des données dans le fichier XML.
        /// </summary>
        /// <param name="dataPath">Chemin d'accèss au fichier XML.</param>
        /// <returns>True si les mises à jour ont été effectuées sans erreur, false dans le cas contraire.</returns>
        private bool UpdateData(string dataPath)
        {
            if (version.UpdateData.Settings)
            {
                //if (!UpdateSettings(dataPath))
                //{
                //    return false;
                //}
            }

            //if (version.UpdateData.Assignments)
            //{
            //    if (!UpdateAssignments(dataPath))
            //    {
            //        return false;
            //    }
            //}

            return true;
        }

        /// <summary>
        /// Supprime tous les fichiers situés dans le dossier Udpate local.
        /// </summary>
        private void CleanFiles()
        {
            if (Directory.Exists(sourcePath))
            {
                Directory.GetFiles(sourcePath, "seatflow*.*", SearchOption.AllDirectories).ToList().ForEach(File.Delete);
                Directory.GetFiles(sourcePath, "*.docx", SearchOption.TopDirectoryOnly).ToList().ForEach(File.Delete);
                if (Directory.Exists(DataFolderName))
                {
                    Directory.Delete(DataFolderName, true);
                }

                Actions.Add(new Action(Properties.Resources.Action_DeleteUdpateFiles, Action.Status.Ended));
            }
        }

        /// <summary>
        /// Modifie le statut des boutons.
        /// </summary>
        /// <param name="exit">True active le bouton Exit, false active le bouton Update.</param>
        private void SetButtonStatus(bool exit = true)
        {
            //if (exit)
            //{
            //    btnExit.IsEnabled = true;
            //    btnExit.Visibility = Visibility.Visible;
            //    btnUpdate.Visibility = Visibility.Collapsed;
            //}
            //else
            //{
            //    btnExit.Visibility = Visibility.Collapsed;
            //    btnUpdate.IsEnabled = true;
            //    btnUpdate.Visibility = Visibility.Visible;
            //}
        }

        /// <summary>
        /// Dessérialise un objet contenu dans le code XML.
        /// </summary>
        /// <typeparam name="T">Objet à désérializer.</typeparam>
        /// <param name="xPath">Chemin XPath de l'objet dans le code XML.</param>
        /// <param name="xDocument">Document XML contenant les données.</param>
        /// <returns>Object désérialisé.</returns>
        private T DeserializeObject<T>(string xPath, XDocument xDocument)
        {
            IEnumerable<XElement> xElements = xDocument.XPathSelectElements(xPath);

            string xml = InnerXML(xElements);

            var xmlSerializer = new XmlSerializer(typeof(T), typeof(T).GetNestedTypes());

            using (var sr = new StringReader(xml))
            {
                return (T)xmlSerializer.Deserialize(sr);
            }
        }

        /// <summary>
        /// Convertit une liste d'élément XML en chaine de carctaères.
        /// </summary>
        /// <param name="xelements">Liste des éléments à convertir.</param>
        /// <returns>Chaine de caractères contenant les éléments XML.</returns>
        private string InnerXML(IEnumerable<XElement> xelements)
        {
            StringBuilder st = new StringBuilder();

            foreach (XElement xElement in xelements)
            {
                st.Append(xElement);
            }

            return st.ToString();
        }

        #endregion

        #region Events

        /// <summary>
        /// Lance la mise à jour.
        /// </summary>
        private void Update()
        {
            //bool result = false;

            //Actions.Clear();
            //Actions.Add(new Action(string.Format(Properties.Resources.Action_UpdateStarted, version.Before, version.After), Action.Status.Comment));

            //// Extrait le chemin d'accès au fichier XML des données.
            //string dataPath = ExtractDataPathFromConfigFile();

            //// Remplace les fichiers exe et dll de l'application.
            //if (CopyFiles(Path.GetDirectoryName(dataPath)))
            //{
            //    // Si les données doivent être mise à jour.
            //    if (version.NeedUpdateData || version.UpdateSchema)
            //    {
            //        if (!string.IsNullOrEmpty(dataPath))
            //        {
            //            // Sauvegarde le fichier XML avant mise à jour.
            //            string backupFolder = BackupData(dataPath);
            //            if (!string.IsNullOrEmpty(backupFolder))
            //            {
            //                // Extrait le chemin d'accès au fichier XSD.
            //                string schemaPath = ExtractSchemaFromXml(dataPath);
            //                if (!string.IsNullOrEmpty(schemaPath))
            //                {
            //                    // Sauvegarde le fichier XSD avant mise à jour.
            //                    if (BackupSchema(schemaPath, backupFolder))
            //                    {
            //                        // Mise à jour des données.
            //                        if(UpdateData(dataPath))
            //                        {
            //                            result = true;

            //                            // Mise à jour du schéma XSD.
            //                            if (version.UpdateSchema)
            //                            {
            //                                result = ReplaceSchema(schemaPath);
            //                            }

            //                            if (result)
            //                            {
            //                                // Supprime les fichiers qui ne sont plus neccessaire dans le dossier Update local.
            //                                CleanFiles();

            //                                Actions.Add(new Action(Properties.Resources.Action_UpdatedSuccessfully, Action.Status.Ended));
            //                                Actions.Add(new Action(Properties.Resources.Action_RestartSeatFlow, Action.Status.Ended));
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}

            //if (result)
            //{

            //    InfoMessage = new StringBuilder().AppendLine(string.Format(Properties.Resources.Info_UpdateEndedSuccessfully, version.After)).AppendLine().
            //        Append($"{Properties.Resources.Info_RestartSeatFlow}").ToString();
            //    InfoIcon = FindResource("CheckOk_Large") as DrawingImage;
            //}
            //else
            //{
            //    InfoMessage = new StringBuilder().AppendLine($"{Properties.Resources.Info_UpdateAborted}").AppendLine().
            //        Append($"{Properties.Resources.Info_ContactSupport}").ToString();
            //    InfoIcon = FindResource("AlertError_Large") as DrawingImage;
            //}

            SetButtonStatus();
        }

        #region Commands Binding

        private void CloseWindowCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        #endregion


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
}
