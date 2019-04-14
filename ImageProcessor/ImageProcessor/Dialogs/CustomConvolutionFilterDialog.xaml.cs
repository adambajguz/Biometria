using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ImageProcessor.Dialogs
{

    public sealed partial class CustomConvolutionFilterDialog : ContentDialog
    {
        public CustomConvolutionFilterDialog()
        {
            this.InitializeComponent();
        }

        public async Task<int[,]> GetKernel()
        {
            int[,] kernel = new int[3, 3];

            try
            {
                kernel[0, 0] = Int32.Parse(V1.Text);
                kernel[0, 1] = Int32.Parse(V2.Text);
                kernel[0, 2] = Int32.Parse(V3.Text);

                kernel[1, 0] = Int32.Parse(V4.Text);
                kernel[1, 1] = Int32.Parse(V5.Text);
                kernel[1, 2] = Int32.Parse(V6.Text);

                kernel[2, 0] = Int32.Parse(V7.Text);
                kernel[2, 1] = Int32.Parse(V8.Text);
                kernel[2, 2] = Int32.Parse(V9.Text);
            }
            catch (FormatException)
            {
                await ShowError("One or more values is not an integer!");

            }
            catch (OverflowException)
            {
                await ShowError("One or more values value is out of range of the Int32 type!");
            }

            return kernel;
        }

        private async Task ShowError(string text)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Error",
                Content = text,
                CloseButtonText = "Ok"
            };

            ContentDialogResult result = await dialog.ShowAsync();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (((ComboBox)sender).SelectedIndex)
            {
                //low pass
                case 0:
                    SetKernel(1, 1, 1, 1, 1, 1, 1, 1, 1);
                    break;

                case 1:
                    SetKernel(1, 1, 1, 1, 2, 1, 1, 1, 1);
                    break;

                case 2:
                    SetKernel(1, 1, 1, 1, 4, 1, 1, 1, 1);
                    break;

                case 3:
                    SetKernel(1, 2, 1, 2, 4, 2, 1, 2, 1);
                    break;

                //high pass
                case 4:
                    SetKernel(-1, -1, -1, -1, 9, -1, -1, -1, -1);
                    break;

                case 5:
                    SetKernel(0, -1, 0, -1, 5, -1, 0, -1, 0);
                    break;

                case 6:
                    SetKernel(1, -2, 1, -2, 5, -2, 1, -2, 1);
                    break;

                case 7:
                    SetKernel(-1, -1, -1, -1, 14, -1, -1, -1, -1);
                    break;

                default:
                    SetKernel(0, 0, 0, 0, 0, 0, 0, 0, 0);
                    break;
            }
        }

        private void SetKernel(int v1, int v2, int v3, int v4, int v5, int v6, int v7, int v8, int v9)
        {
            V1.Text = v1.ToString();
            V2.Text = v2.ToString();
            V3.Text = v3.ToString();

            V4.Text = v4.ToString();
            V5.Text = v5.ToString();
            V6.Text = v6.ToString();

            V7.Text = v7.ToString();
            V8.Text = v8.ToString();
            V9.Text = v9.ToString();
        }
    }
}
