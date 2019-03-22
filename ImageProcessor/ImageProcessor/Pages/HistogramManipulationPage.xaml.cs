using System.Linq;
using ImageProcessor.Data;
using LiveCharts;
using LiveCharts.Uwp;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ImageProcessor.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HistogramManipulationPage : Page
    {
        private WriteableBitmap editingBitmap;

        private readonly MainPage parentMainPage;

        private ImageHistogramData bitmapHistogramData;

        public HistogramManipulationPage()
        {
            this.InitializeComponent();

            Frame frame = (Frame)Window.Current.Content;
            parentMainPage = (MainPage)frame.Content;

            DataContext = this;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.editingBitmap = e.Parameter as WriteableBitmap;

            UpdateHistograms();
        }

        private void UpdateHistograms()
        {
            bitmapHistogramData = new ImageHistogramData(editingBitmap);
            SetHistograms(bitmapHistogramData);
        }

        private void SetHistograms(ImageHistogramData data)
        {
            SeriesCollection SeriesCollectionR = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Red",
                    Values = new ChartValues<int>(data.R.AsEnumerable()),
                    Fill = new SolidColorBrush(Colors.Red),
                }
            };
            RPlot.Series = SeriesCollectionR;
            RPlot.AxisY.Clear();

            RPlot.AxisY.Add(
            new Axis
            {
                MinValue = 0,
                MaxValue = data.R.Max()
            });

            SeriesCollection SeriesCollectionG = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Green",
                    Values = new ChartValues<int>(data.G.AsEnumerable()),
                    Fill = new SolidColorBrush(Colors.Green),
                }
            };
            GPlot.Series = SeriesCollectionG;
            GPlot.AxisY.Clear();

            GPlot.AxisY.Add(
            new Axis
            {
                MinValue = 0,
                MaxValue = data.G.Max()
            });

            SeriesCollection SeriesCollectionB = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Blue",
                    Values = new ChartValues<int>(data.B.AsEnumerable()),
                    Fill = new SolidColorBrush(Colors.Blue),
                }
            };
            BPlot.Series = SeriesCollectionB;
            BPlot.AxisY.Clear();

            BPlot.AxisY.Add(
            new Axis
            {
                MinValue = 0,
                MaxValue = data.B.Max()
            });

            SeriesCollection SeriesCollectionC = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "(R+G+B)/3",
                    Values = new ChartValues<int>(data.C.AsEnumerable()),
                    Fill = new SolidColorBrush(Colors.Black),
                }
            };
            CPlot.Series = SeriesCollectionC;
            CPlot.AxisY.Clear();

            CPlot.AxisY.Add(
            new Axis
            {
                MinValue = 0,
                MaxValue = data.C.Max()
            });
        }
    }

}
