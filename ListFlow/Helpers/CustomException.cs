using System;

namespace ListFlow.Helpers
{
    public class CustomException : Exception
    {
        public CustomException()
        {
        }

        public CustomException(string message) : base(message)
        {
            _ = Controls.MessageBoxUC.Show(null, Properties.Resources.Exception_MessageBox_TitleText, message, Controls.MessageBoxUC.MessageType.Error);
        }

        public CustomException(string message, string title) : base(message)
        {

            _ = Controls.MessageBoxUC.Show(null, title, message, Controls.MessageBoxUC.MessageType.Error);
        }

        public CustomException(string message, Exception innerException) : base(message, innerException)
        {
            _ = Controls.MessageBoxUC.Show(null, Properties.Resources.Exception_MessageBox_TitleText, $"{message}{Environment.NewLine}{Environment.NewLine}{innerException.Message}", Controls.MessageBoxUC.MessageType.Error);

        }

        public CustomException(string message, Exception innerException, string title)
        {
            if (innerException != null)
            {
                _ = Controls.MessageBoxUC.Show(null, title, $"{message}{Environment.NewLine}{Environment.NewLine}{innerException.Message}", Controls.MessageBoxUC.MessageType.Error);
            }
            else
            {
                _ = Controls.MessageBoxUC.Show(null, title, message, Controls.MessageBoxUC.MessageType.Error);
            }
        }
    }
}
