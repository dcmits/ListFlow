using System;
using System.Windows;

namespace ListFlow.Helpers
{
    public static class UiRefresh
    {
        private static Action EmptyDelegate = delegate() { };

        public static void Refresh(this UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(EmptyDelegate, System.Windows.Threading.DispatcherPriority.Render);
        }

    }
}
