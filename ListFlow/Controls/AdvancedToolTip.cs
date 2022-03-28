using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace ListFlow.Controls
{
    public class AdvancedToolTip : MarkupExtension
    {
        #region Constructors

        public AdvancedToolTip()
        {
        }

        public AdvancedToolTip(string title, string message, AdvancedToolTipUC.Icon icon)
        {
            Title = title;
            Message = message;
            IconType = icon;
        }

        #endregion

        #region Properties

        public string Title { get; set; }
        public string Message { get; set; }
        public double MaxTextWidth { get; set; }
        public AdvancedToolTipUC.Icon IconType { get; set; }

        #endregion

        #region Methods

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            AdvancedToolTipUC advancedToolTip = new AdvancedToolTipUC();

            if (!string.IsNullOrEmpty(Title))
            {
                advancedToolTip.TitleText = Title;
            }

            if (!string.IsNullOrEmpty(Message))
            {
                advancedToolTip.MessageText = Message;
            }

            advancedToolTip.MaxTextWidth = MaxTextWidth;

            if (IconType == AdvancedToolTipUC.Icon.Editable)
            {
                advancedToolTip.MessageText = $"{advancedToolTip.MessageText} {Properties.Resources.CanBeEdited}";
            }

            if (IconType == AdvancedToolTipUC.Icon.Uneditable)
            {
                advancedToolTip.MessageText = $"{advancedToolTip.MessageText} {Properties.Resources.CannotBeEdited}";
            }

            advancedToolTip.IconType = IconType;

            ToolTip masquedToolTip = new ToolTip
            {
                HasDropShadow = false,
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
                Content = advancedToolTip
            };

            return masquedToolTip;
        }

        #endregion
    }
}
