using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ListFlow.Helpers
{
    public static class LayoutHelper
    {
        /// <summary>
        /// Retourne les contours absolus du contrôle relatif au container.
        /// </summary>
        /// <param name="element">Elément source.</param>
        /// <param name="relativeTo">Container.</param>
        /// <returns>Rectangle définissant les contours.</returns>
        public static Rect GetRelativePlacement(UIElement element, UIElement relativeTo)
        {
            Point absolutePos = element.PointToScreen(new Point(0, 0));
            Point posRelativeTo = relativeTo.PointToScreen(new Point(0, 0));

            Point topLeft = new Point(absolutePos.X - posRelativeTo.X, absolutePos.Y - posRelativeTo.Y);
            Point bottomRight = element.PointToScreen(new Point(element.RenderSize.Width, element.RenderSize.Height));

            Rect bounds = Rect.Empty;
            bounds.Union(topLeft);
            bounds.Union(bottomRight);

            return bounds;
        }

        /// <summary>
        /// Parcours l'arbre visuel pour rechercher le contrôle parent correpondant au type.
        /// </summary>
        /// <typeparam name="T">Type recherché</typeparam>
        /// <param name="child">Contrôle enfant point de départ de la recherche</param>
        /// <returns>Contrôle parent correspondant au type, null si pas de parent correspondant au type</returns>
        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);

            if (parent == null)
            {
                return null;
            }

            if (parent is T currentParent)
            {
                return currentParent;
            }
            else
            {
                return FindParent<T>(parent);
            }
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        public static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T)
                    return (T)child;
                else
                {
                    T childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        public static IEnumerable<string> GetTextBlockLines(TextBlock source)
        {
            string text = source.Text;
            int offset = 0;
            TextPointer lineStart = source.ContentStart.GetPositionAtOffset(1, LogicalDirection.Forward);

            do
            {
                TextPointer lineEnd = lineStart != null ? lineStart.GetLineStartPosition(1) : null;
                int length = lineEnd != null ? lineStart.GetOffsetToPosition(lineEnd) : text.Length - offset;
                yield return text.Substring(offset, length);
                offset += length;
                lineStart = lineEnd;
            }
            while (lineStart != null);
        }


        private static StringBuilder sbListControls;

        public static StringBuilder GetVisualTreeInfo(Visual element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(String.Format("Element {0} is null !", element.ToString()));
            }

            Canvas canvas = element as Canvas;

            sbListControls = new StringBuilder();

            // Ajoute l'en-tête du fichier svg.
            sbListControls.AppendLine(@"<?xml version=""1.0"" standalone=""no""?>");            
            sbListControls.AppendLine(string.Format(@"<svg version=""1.1"" xmlns=""http://www.w3.org/2000/svg""  viewBox=""0 -20 {0} {1}"">",
                canvas.ActualWidth,
                canvas.ActualHeight + 20));
            sbListControls.AppendLine(@"<desc>Room XXV</desc>");

            //sbListControls.AppendLine(string.Format(@"<svg version=""1.1"" xmlns=""http://www.w3.org/2000/svg""  viewBox=""0 0 {0} {1}"">",
            //    canvas.ActualWidth + canvas.Margin.Left + canvas.Margin.Right,
            //    canvas.ActualHeight + canvas.Margin.Top + canvas.Margin.Bottom));

            //x="0px" y="0px" viewBox="0 0 324.1 247.5"

            GetControlsList(element, 0);

            // Ajoute le pied du fichier svg.
            sbListControls.AppendLine(@"</svg>");

            return sbListControls;
        }


        private static void GetControlsList(Visual control, int level)
        {
            int ChildNumber = VisualTreeHelper.GetChildrenCount(control);

            for (int i = 0; i <= ChildNumber - 1; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(control, i);

                if (v is Path)
                {
                    Path path = v as Path;
                    //string fill = (path.Fill.ToString().StartsWith("#FF") && path.Fill.ToString().Length == 9) ? path.Fill.ToString().Replace("#FF", "#") : path.Fill.ToString();

                    sbListControls.AppendLine(string.Format(@"<path d=""{0}"" fill=""{1}""/>", path.Data, BrushColorToHex(path.Fill)));
                    // Original.
                    //sbListControls.Append(new string(' ', level * indent));
                    //sbListControls.Append(v.GetType());
                    //sbListControls.Append(Environment.NewLine);
                }
                else if (v is Line)
                {
                    Line line = v as Line;
                    //string stroke = (line.Stroke.ToString().StartsWith("#FF") && line.Stroke.ToString().Length == 9) ? line.Stroke.ToString().Replace("#FF", "#") : line.Stroke.ToString();
                    //string stroke = (line.Stroke.ToString().StartsWith("#FF") && line.Stroke.ToString().Length == 9) ? line.Stroke.ToString().Replace("#FF", "#") : line.Stroke.ToString();

                    sbListControls.AppendLine(string.Format(@"<line x1=""{0}"" y1=""{1}"" x2=""{2}"" y2=""{3}"" fill=""{4}"" stroke=""{5}"" stroke-width=""{6}""/>", 
                        line.X1, line.Y1, line.X2, line.Y2, BrushColorToHex(line.Fill), BrushColorToHex(line.Stroke), line.StrokeThickness));
                }
                else if (v is Polyline)
                {
                    Polyline polyline = v as Polyline;

                    sbListControls.AppendLine(string.Format(@"<polyline points=""{0}"" fill=""{1}"" stroke=""{2}"" stroke-width=""{3}""/>",
                        polyline.Points, BrushColorToHex(polyline.Fill), BrushColorToHex(polyline.Stroke), polyline.StrokeThickness));
                }
                //else if (v is TextBlock)
                //{
                //    TextBlock textBlock = v as TextBlock;

                //    //sbListControls.AppendLine(string.Format(@"<text transform=""matrix({0} {1} {2} {3} {4} {5})"" fill=""{6}"" font-family=""{7}"" font-size=""{8}""/>",
                //    //    textBlock.FontFamily, textBlock.FontSize));



                //    //< text id = "seat012" transform = "matrix(5.143517e-17 -0.84 1 6.123234e-17 148.6099 21.5301)" class="st7 st8 st9">#12</text>
                //    //< text id = "seat060" transform = "matrix(0.84 0 0 1 100.8999 57.9901)" class="st7 st8 st9">#60</text>

                //  	//.st7{fill:#1D1D1B;}
	               // //.st8{ font-family:'ArialNarrow'; }
	               // //.st9{ font-size:4px; }

                //        // Deux lignes.
                //        //<text transform="matrix(1 0 0 1 451.4932 252.3446)"><tspan x="0" y="0" class="st8 seatBase0">#10 </tspan><tspan x="0" y="4.8" class="st8 seatBase0">Test</tspan></text>
                //        // Simple ligne.
                //        //<text transform="matrix(1 0 0 1 451.4932 273.2629)" class="st8 st9">#12 zzezfe </text>
                //        // Class.
                //        //.st7{ fill: none; }
                //        //.st8{ font - family:'MyriadPro-Regular'; }
                //        //.st9{ font - size:5px; }
                //        //.seatBase0{ font - size:4px; }

                //    if (textBlock.Text.Contains("ANTIGUA AND BARBUDA"))
                //    {
                //        Console.WriteLine(textBlock);
                //    }
                //}
                else
                {
                    // Console.WriteLine(v.GetType());
                }


                if (VisualTreeHelper.GetChildrenCount(v) > 0)
                {
                    GetControlsList(v, level + 1);
                }
            }
        }

        private static string BrushColorToHex(Brush brush)
        {
            return (brush != null) ? (brush.ToString().StartsWith("#FF") && brush.ToString().Length == 9) ? brush.ToString().Replace("#FF", "#") : brush.ToString() : "none";
        }

        public static System.Windows.Media.Brush ColorCodeToBrush(string colorCode)
        {
            return (System.Windows.Media.Brush)(new System.Windows.Media.BrushConverter()).ConvertFromString(colorCode);
        }

        public static string SolidColorBrushToHex(SolidColorBrush brush)
        {
            if (brush != null)
            {
                string hex = brush.Color.ToString();

                return hex.StartsWith("#FF") && hex.Length == 9 ? hex.Remove(1, 2) : hex;
            }

            return string.Empty;
        }

        public static string HexColorRemoveOpacity(string hex)
        {
            if (!string.IsNullOrEmpty(hex))
            {
                return hex.StartsWith("#FF") && hex.Length == 9 ? hex.Remove(1, 2) : hex;
            }
            return hex;
        }


        //public static List<string>GetTextBlockLines(TextBlock sender)
        //{
        //    int start = 0;
        //    int length = 0;

        //    List<string> tokens = new List<string>();

        //    foreach (object lineMetrics in GetLineMetrics(sender))
        //    {
        //        length = GetLength(lineMetrics);
        //        tokens.Add(sender.Text.Substring(start, length));

        //        start += length;
        //    }

        //    return tokens;
        //}

        //private static int GetLength(object lineMetrics)
        //{
        //    PropertyInfo propertyInfo = lineMetrics.GetType().GetProperty("Length", BindingFlags.Instance
        //        | BindingFlags.NonPublic);

        //    return (int)propertyInfo.GetValue(lineMetrics, null);
        //}

        //private static IEnumerable GetLineMetrics(TextBlock textBlock)
        //{
        //    ArrayList metrics = new ArrayList();
        //    FieldInfo fieldInfo = typeof(TextBlock).GetField("_firstLine", BindingFlags.Instance
        //        | BindingFlags.NonPublic);
        //    metrics.Add(fieldInfo.GetValue(textBlock));

        //    fieldInfo = typeof(TextBlock).GetField("_subsequentLines", BindingFlags.Instance
        //        | BindingFlags.NonPublic);

        //    object nextLines = fieldInfo.GetValue(textBlock);
        //    if (nextLines != null)
        //    {
        //        metrics.AddRange((ICollection)nextLines);
        //    }

        //    return metrics;
        //}
    }
}
