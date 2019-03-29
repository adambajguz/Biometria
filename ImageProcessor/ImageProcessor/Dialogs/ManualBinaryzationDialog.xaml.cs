using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ImageProcessor.Dialogs
{

    public sealed partial class ManualBinaryzationDialog : ContentDialog
    {
        public ManualBinaryzationDialog()
        {
            this.InitializeComponent();
        }

        public int TresholdValue => (int)SliderValue.Value;
    }
}
