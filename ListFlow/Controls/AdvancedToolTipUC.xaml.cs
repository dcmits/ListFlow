using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ListFlow.Controls
{
    /// <summary>
    /// Interaction logic for AdvancedToolTip.xaml
    /// </summary>
    public partial class AdvancedToolTipUC : UserControl
    {
        #region Enums

        public enum Icon
        {
            None,
            Help,
            Editable,
            Uneditable
        }

        #endregion

        #region Constructors

        public AdvancedToolTipUC()
        {
            InitializeComponent();
        }

        #endregion

        #region Dependency Properties

        #region TitleText

        public static readonly DependencyProperty TitleTextProperty = DependencyProperty.Register("TitleText", typeof(string), typeof(AdvancedToolTipUC), new PropertyMetadata("", TitleTextPropertyChanged));

        public string TitleText
        {
            get => (string)GetValue(TitleTextProperty);
            set => SetValue(TitleTextProperty, value);
        }

        private static void TitleTextPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            AdvancedToolTipUC advancedToolTipUC = (AdvancedToolTipUC)sender;
            advancedToolTipUC.OnTitleTextPropertyChanged(e);
        }

        private void OnTitleTextPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            tbkTitleText.Visibility = string.IsNullOrEmpty((string)e.NewValue) ? Visibility.Collapsed : Visibility.Visible;
        }

        #endregion

        #region MessageText

        public static readonly DependencyProperty MessageTextProperty = DependencyProperty.Register("MessageText", typeof(string), typeof(AdvancedToolTipUC), new PropertyMetadata("", MessageTextPropertyChanged));

        public string MessageText
        {
            get => (string)GetValue(MessageTextProperty);
            set => SetValue(MessageTextProperty, value);
        }

        private static void MessageTextPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            AdvancedToolTipUC advancedToolTipUC = (AdvancedToolTipUC)sender;
            advancedToolTipUC.OnMessageTextPropertyChanged(e);
            if (e.NewValue.ToString().Length > 200)
            {
                
                System.Console.WriteLine("Test");
                
            }
        }

        private void OnMessageTextPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            tbkMessageText.Visibility = string.IsNullOrEmpty((string)e.NewValue) ? Visibility.Collapsed : Visibility.Visible;
        }

        #endregion

        #region MaxTextWidth

        public static readonly DependencyProperty MaxTextWidthProperty = DependencyProperty.Register("MaxTextWidth", typeof(double), typeof(AdvancedToolTipUC), new PropertyMetadata(300d, MaxTextWidthPropertyChanged));

        public double MaxTextWidth
        {
            get => (double)GetValue(MaxTextWidthProperty);
            set
            {
                if (value == 0)
                {
                    SetValue(MaxTextWidthProperty, 300d);
                }
                else
                {
                    SetValue(MaxTextWidthProperty, value);
                }
            }
        }

        private static void MaxTextWidthPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            AdvancedToolTipUC advancedToolTipUC = (AdvancedToolTipUC)sender;
            advancedToolTipUC.OnMaxTextWidthPropertyChanged(e);
        }

        private void OnMaxTextWidthPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            grdMain.MaxWidth = (double)e.NewValue;
        }

        #endregion

        #region IconType

        public static readonly DependencyProperty IconTypeProperty = DependencyProperty.Register("IconType", typeof(Icon), typeof(AdvancedToolTipUC), new PropertyMetadata(Icon.None, IconTypePropertyChanged));

        public Icon IconType
        {
            get => (Icon)GetValue(IconTypeProperty);
            set => SetValue(IconTypeProperty, value);
        }

        private static void IconTypePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            AdvancedToolTipUC advancedToolTipUC = (AdvancedToolTipUC)sender;
            advancedToolTipUC.OnIconTypePropertyChanged(e);
        }

        private void OnIconTypePropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            string largeImageSourceName;
            string smallImageSourceName;

            switch (IconType)
            {
                case Icon.Help:
                    largeImageSourceName = "HelpOn_Large";
                    smallImageSourceName = "HelpOn_Small";
                    break;
                case Icon.Editable:
                    largeImageSourceName = "Editable_Large";
                    smallImageSourceName = "Editable_Small";
                    break;
                case Icon.Uneditable:
                    largeImageSourceName = "Uneditable_Large";
                    smallImageSourceName = "Uneditable_Small";
                    break;
                default:
                    largeImageSourceName = "HelpOn_Large";
                    smallImageSourceName = "HelpOn_Small";
                    break;
            }

            if (IconType != Icon.None)
            {
                imgIcon.Source = Helpers.LayoutHelper.GetTextBlockLines(tbkTitleText).Count() > 1 ? (System.Windows.Media.ImageSource)Application.Current.Resources[largeImageSourceName] : (System.Windows.Media.ImageSource)Application.Current.Resources[smallImageSourceName];
            }
            else
            {
                imgIcon.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #endregion
    }
}
