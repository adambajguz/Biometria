using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ImageProcessor.Dialogs
{

    public sealed partial class NiblackBinaryzationDialog : ContentDialog
    {
        public NiblackBinaryzationDialog()
        {
            this.InitializeComponent();
        }

        public int SValue => (int)SliderValue.Value;

        public double KValue => SliderValue2.Value;
    }
}
