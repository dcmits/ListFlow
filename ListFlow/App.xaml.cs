using System.Threading;
using System.Windows;

namespace ListFlow
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex mutex = null;

        // Avoid app launching multiple time.
        protected override void OnStartup(StartupEventArgs e)
        {
            mutex = new Mutex(true, "ListFlow", out bool aIsNewInstance);

            if (!aIsNewInstance)
            {
                _ = Controls.MessageBoxUC.Show(null, ListFlow.Properties.Resources.AppTitle, ListFlow.Properties.Resources.App_Running, Controls.MessageBoxUC.MessageType.Information);

                // Exiting the application because is allready running. 
                Current.Shutdown();
            }

            base.OnStartup(e);
        }
    }
}
