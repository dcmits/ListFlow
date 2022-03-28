using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.ComponentModel;

namespace ListFlow.Helpers
{
    public static class TextBlockBehavior
    {
        #region ShrinkToFit

        // #ShrinkToFit

        /// <summary>
        /// Ajuste le texte dans le TextBlock selon la taille disponible.
        /// </summary>
        public static readonly DependencyProperty ShrinkToFitProperty = DependencyProperty.RegisterAttached("ShrinkToFit", typeof(bool), typeof(TextBlockBehavior), new PropertyMetadata(false, new PropertyChangedCallback(ShrinkToFitChanged)));

        public static bool GetShrinkToFit(TextBlock obj)
        {
            return (bool)obj.GetValue(ShrinkToFitProperty);
        }

        public static void SetShrinkToFit(TextBlock obj, bool value)
        {
            obj.SetValue(ShrinkToFitProperty, value);
        }

        private static void ShrinkToFitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBlock textBlock = d as TextBlock;

            if (e.NewValue != null)
            {
                textBlock.AddHandler(TextBlock.SizeChangedEvent, TargetSizeChangedEventHandler);
                {
                    var withBlock = DependencyPropertyDescriptor.FromProperty(TextBlock.TextProperty, typeof(TextBlock));
                    withBlock.AddValueChanged(textBlock, TargetTextChangedEventHandler);
                }
                textBlock.AddHandler(TextBlock.LoadedEvent, TargetLoadedEventHandler);
            }
            else
            {
                textBlock.RemoveHandler(TextBlock.SizeChangedEvent, TargetSizeChangedEventHandler);
                {
                    var withBlock = DependencyPropertyDescriptor.FromProperty(TextBlock.TextProperty, typeof(TextBlock));
                    withBlock.RemoveValueChanged(textBlock, TargetTextChangedEventHandler);
                }

                textBlock.RemoveHandler(TextBlock.LoadedEvent, TargetLoadedEventHandler);
            }
        }

        private static readonly RoutedEventHandler TargetSizeChangedEventHandler = new RoutedEventHandler(TargetSizeChanged);

        private static void TargetSizeChanged(object Target, RoutedEventArgs e)
        {
            Update(Target as TextBlock);
        }

        private static readonly EventHandler TargetTextChangedEventHandler = new EventHandler(TargetTextChanged);

        private static void TargetTextChanged(object Target, EventArgs e)
        {
            Update(Target as TextBlock);
        }

        private static readonly RoutedEventHandler TargetLoadedEventHandler = new RoutedEventHandler(TargetLoaded);

        private static void TargetLoaded(object Target, RoutedEventArgs e)
        {
            Update(Target as TextBlock);
        }

        private static readonly HashSet<TextBlock> Shrinkging = new HashSet<TextBlock>();

        private static void Update(TextBlock Target)
        {
            if (Target.IsLoaded)
            {
                var Clip = System.Windows.Controls.Primitives.LayoutInformation.GetLayoutClip(Target);

                if (Clip != null)
                {
                    if (!Shrinkging.Contains(Target))
                    {
                        Shrinkging.Add(Target);
                    }

                    Target.FontSize -= 0.05;
                }
                else if (Target.FontSize < TextElement.GetFontSize(Target.Parent))
                {
                    if (Shrinkging.Contains(Target))
                    {
                        Shrinkging.Remove(Target);
                    }
                    else
                    { 
                        Target.FontSize += 0.05;
                    }
                }
            }
        }
    }

    #endregion
}
