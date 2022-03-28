using System.Windows.Controls;
using System.Windows;

namespace Update.Helpers
{
    public static class TextBoxBehavior
    {
        public static bool GetSelectAllTextOnFocus(TextBox textBox)
        {
            return (bool)textBox.GetValue(SelectAllTextOnFocusProperty);
        }

        public static void SetSelectAllTextOnFocus(TextBox textBox, bool value)
        {
            textBox.SetValue(SelectAllTextOnFocusProperty, value);
        }

        public static readonly DependencyProperty SelectAllTextOnFocusProperty =
            DependencyProperty.RegisterAttached(
                "SelectAllTextOnFocus",
                typeof(bool),
                typeof(TextBoxBehavior),
                new UIPropertyMetadata(false, OnSelectAllTextOnFocusChanged));

        private static void OnSelectAllTextOnFocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBox textBox = d as TextBox;
            if (textBox == null) return;

            if (e.NewValue is bool == false) return;

            if ((bool)e.NewValue)
            {
                textBox.GotFocus += SelectAll;
                textBox.PreviewMouseDown += IgnoreMouseButton;
            }
            else
            {
                textBox.GotFocus -= SelectAll;
                textBox.PreviewMouseDown -= IgnoreMouseButton;
            }
        }

        private static void SelectAll(object sender, RoutedEventArgs e)
        {
            TextBox textBox = e.OriginalSource as TextBox;
            if (textBox == null) return;
            textBox.SelectAll();
        }

        private static void IgnoreMouseButton(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null || (!textBox.IsReadOnly && textBox.IsKeyboardFocusWithin)) return;

            e.Handled = true;
            textBox.Focus();
        }
    }
}

//public class TextBoxBehavior
//{
//    public static bool GetSelectAllTextOnFocus(TextBox textBox)
//    {
//        return (bool)textBox.GetValue(SelectAllTextOnFocusProperty);
//    }

//    public static void SetSelectAllTextOnFocus(TextBox textBox, bool value)
//    {
//        textBox.SetValue(SelectAllTextOnFocusProperty, value);
//    }

//    public static readonly DependencyProperty SelectAllTextOnFocusProperty =
//        DependencyProperty.RegisterAttached(
//            "SelectAllTextOnFocus",
//            typeof(bool),
//            typeof(TextBoxBehavior),
//            new UIPropertyMetadata(false, OnSelectAllTextOnFocusChanged));

//    private static void OnSelectAllTextOnFocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
//    {
//        var textBox = d as TextBox;
//        if (textBox == null) return;

//        if (e.NewValue is bool == false) return;

//        if ((bool)e.NewValue)
//        {
//            textBox.GotFocus += SelectAll;
//            textBox.PreviewMouseDown += IgnoreMouseButton;
//        }
//        else
//        {
//            textBox.GotFocus -= SelectAll;
//            textBox.PreviewMouseDown -= IgnoreMouseButton;
//        }
//    }

//    private static void SelectAll(object sender, RoutedEventArgs e)
//    {
//        var textBox = e.OriginalSource as TextBox;
//        if (textBox == null) return;
//        textBox.SelectAll();
//    }

//    private static void IgnoreMouseButton(object sender, System.Windows.Input.MouseButtonEventArgs e)
//    {
//        var textBox = sender as TextBox;
//        if (textBox == null || (!textBox.IsReadOnly && textBox.IsKeyboardFocusWithin)) return;

//        e.Handled = true;
//        textBox.Focus();
//    }
//}
