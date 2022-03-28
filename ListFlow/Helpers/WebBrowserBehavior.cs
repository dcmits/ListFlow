using System;
using System.Windows;
using System.Windows.Controls;

namespace ListFlow.Helpers
{
    /// <summary>
    /// WebBrowser Source Binding.
    /// </summary>
    public static class WebBrowserBehavior
    {
        #region Depedencies

        public static readonly DependencyProperty BindableSourceProperty = DependencyProperty.RegisterAttached("BindableSource", typeof(string), typeof(WebBrowserBehavior), new UIPropertyMetadata(null, BindableSourcePropertyChanged));

        #endregion

        #region Getter/Setter

        public static string GetBindableSource(DependencyObject obj)
        {
            return (string)obj.GetValue(BindableSourceProperty);
        }

        public static void SetBindableSource(DependencyObject obj, string value)
        {
            obj.SetValue(BindableSourceProperty, value);
        }

        #endregion

        #region Properties

        public static void BindableSourcePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            WebBrowser browser = obj as WebBrowser;
            if (browser != null)
            {
                string uri = e.NewValue as string;
                browser.Source = !string.IsNullOrEmpty(uri) ? new Uri(uri) : null;
            }
        }

        #endregion
    }
}
