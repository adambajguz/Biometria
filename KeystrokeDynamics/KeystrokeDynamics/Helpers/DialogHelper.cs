namespace KeystrokeDynamics.Helpers
{
    using System;
    using System.Threading.Tasks;
    using Windows.UI.Xaml.Controls;

    public static class DialogHelper
    {
        public static async Task<ContentDialogResult> ShowMessage(string content, string title = "Keystroke Dynamics", string closeText = "Close")
        {
            ContentDialog aboutDialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = closeText
            };

            return await aboutDialog.ShowAsync();
        }
    }
}
