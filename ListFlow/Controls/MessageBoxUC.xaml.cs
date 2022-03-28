using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ListFlow.Controls
{
    /// <summary>
    /// MessageBox revisité.
    /// </summary>
    public partial class MessageBoxUC : Window
    {
        #region Fields

        // Instance. 
        static MessageBoxUC messageBox;
        // Action de l'utilisateur.
        static MessageBoxResult messageBoxResult = MessageBoxResult.No;

        #endregion
        
        #region Enums

        // Icône utilisée.
        private enum MessageIcon
        {
            None,
            Warning,
            Question,
            Information,
            Error
        }

        // Profil du message (défini les boutons et l'icône).
        public enum MessageType
        {
            YesNo,
            YesNoCancel,
            Information,
            Error,
            Warning
        }

        #endregion

        #region Constructors

        private MessageBoxUC()
        {
            InitializeComponent();

            btnCancel.Click += new RoutedEventHandler(Button_Click);
            btnNo.Click += new RoutedEventHandler(Button_Click);
            btnOk.Click += new RoutedEventHandler(Button_Click);
            btnYes.Click += new RoutedEventHandler(Button_Click);

            Mouse.OverrideCursor = null;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Affiche la fenêtre de message.
        /// </summary>
        /// <param name="owner">PArent de la fenêtre de message (pour le positionnement).</param>
        /// <param name="title">Titre de du message.</param>
        /// <param name="message">Corps du message.</param>
        /// <param name="type">Type de message (influence les boutons et l'icône)</param>
        /// <returns>Code du bouton pressé par l'utilisateur.</returns>
        public static MessageBoxResult Show(Window owner, string title, string message, MessageType type)
        {
            switch (type)
            {
                case MessageType.YesNo:
                    return ShowMessageBox(owner, title, message, MessageBoxButton.YesNo, MessageIcon.Question);
                case MessageType.YesNoCancel:
                    return ShowMessageBox(owner, title, message, MessageBoxButton.YesNoCancel, MessageIcon.Question);
                case MessageType.Information:
                    return ShowMessageBox(owner, title, message, MessageBoxButton.OK, MessageIcon.Information);
                case MessageType.Error:
                    return ShowMessageBox(owner, title, message, MessageBoxButton.OK, MessageIcon.Error);
                case MessageType.Warning:
                    return ShowMessageBox(owner, title, message, MessageBoxButton.OK, MessageIcon.Warning);
                default:
                    return MessageBoxResult.No;
            }
        }

        /// <summary>
        /// Affiche la fenêtre de message.
        /// </summary>
        /// <param name="owner">PArent de la fenêtre de message (pour le positionnement).</param>
        /// <param name="title">Titre de du message.</param>
        /// <param name="message">Corps du message.</param>
        /// <param name="button">Boutons à afficher.</param>
        /// <param name="icon">Icône à afficher.</param>
        /// <returns>Code du bouton pressé par l'utilisateur.</returns>
        private static MessageBoxResult ShowMessageBox(Window owner, string title, string message, MessageBoxButton button, MessageIcon icon)
        {
            messageBox = new MessageBoxUC();
            messageBox.tbkTitleText.Text = title;
            messageBox.tbkMessageText.Text = message;

            SetButtons(button);
            SetIcon(icon);
            messageBox.Owner = owner;
            messageBox.ShowDialog();

            return messageBoxResult;
        }

        /// <summary>
        /// Affiche/Masque les boutons.
        /// </summary>
        /// <param name="button">Profil de bouton à afficher.</param>
        private static void SetButtons(MessageBoxButton button)
        {
            switch (button)
            {
                case MessageBoxButton.OK:
                    messageBox.btnCancel.Visibility = Visibility.Collapsed;
                    messageBox.btnYes.Visibility = Visibility.Collapsed;
                    messageBox.btnNo.Visibility = Visibility.Collapsed;
                    messageBox.btnOk.Focus();
                    break;
                case MessageBoxButton.OKCancel:
                    messageBox.btnYes.Visibility = Visibility.Collapsed;
                    messageBox.btnNo.Visibility = Visibility.Collapsed;
                    messageBox.btnOk.Focus();
                    break;
                case MessageBoxButton.YesNoCancel:
                    messageBox.btnOk.Visibility = Visibility.Collapsed;
                    messageBox.btnNo.Focus();
                    break;
                case MessageBoxButton.YesNo:
                    messageBox.btnCancel.Visibility = Visibility.Collapsed;
                    messageBox.btnOk.Visibility = Visibility.Collapsed;
                    messageBox.btnNo.Focus();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Affiche l'icône liée au profile.
        /// </summary>
        /// <param name="icon">Icône à afficher.</param>
        private static void SetIcon(MessageIcon icon)
        {
            string imageSourceName = string.Empty;

            switch (icon)
            {
                case MessageIcon.Warning:
                    imageSourceName = "Warning_Large";
                    break;
                case MessageIcon.Question:
                    imageSourceName = "Question_Large";
                    break;
                case MessageIcon.Information:
                    imageSourceName = "Information_Large";
                    break;
                case MessageIcon.Error:
                    imageSourceName = "Error_Large";
                    break;
                case MessageIcon.None:
                default:
                    break;
            }

            if (icon != MessageIcon.None)
            {
                messageBox.imgIcon.Source = Helpers.LayoutHelper.GetTextBlockLines(messageBox.tbkTitleText).Count() > 1 ? (System.Windows.Media.ImageSource)Application.Current.Resources[imageSourceName] : (System.Windows.Media.ImageSource)Application.Current.Resources[imageSourceName];
            }
            else
            {
                messageBox.imgIcon.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// GEstionnaire des boutons.
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender == btnOk)
            {
                messageBoxResult = MessageBoxResult.OK;
            }
            else if (sender == btnCancel)
            {
                messageBoxResult = MessageBoxResult.Cancel;
            }
            else if (sender == btnNo)
            {
                messageBoxResult = MessageBoxResult.No;
            }
            else if (sender == btnYes)
            {
                messageBoxResult = MessageBoxResult.Yes;
            }
            else
            {
                messageBoxResult = MessageBoxResult.None;
            }

            messageBox.Close();
            messageBox = null;
        }

        #endregion
    }
}
