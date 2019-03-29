using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;

namespace ImageProcessor.Pages
{
    public partial class MainPage
    {

        private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e) => throw new Exception("Failed to load Page " + e.SourcePageType.FullName);

        public const string PixelManagerTag = "Pixel Manager";
        public const string HistogramManipulationTag = "Histograms";

        // List of ValueTuple holding the Navigation Tag and the relative Navigation Page
        private readonly List<(string Tag, Type Page)> _pages = new List<(string Tag, Type Page)>
        {
            (PixelManagerTag, typeof(PixelManagerPage)),
            (HistogramManipulationTag, typeof(HistogramManipulationPage)),
        };

        private bool navigated = false;

        public void NavView_Navigate(string navItemTag, object parameter)
        {
            ContentFrameShow();
            ContentFrameMinimize.IsEnabled = true;
            ContentFrameClose.IsEnabled = true;

            Type _page = null;

            var item = _pages.FirstOrDefault(p => p.Tag.Equals(navItemTag));
            _page = item.Page;

            // Get the page type before navigation so you can prevent duplicate
            // entries in the backstack.
            var preNavPageType = ContentFrame.CurrentSourcePageType;
            var preNavPageType2 = ContentFrame.SourcePageType;

            // Only navigate if the selected page isn't currently loaded.
            if (!(_page is null) && !navigated)// && !Type.Equals(preNavPageType, _page) && !Type.Equals(preNavPageType, _page))
            {
                navigated = true;
                ContentFrame.Navigate(_page, parameter);
            }

            ContentFramePageName.Text = navItemTag;
        }


        private void ContentFrameMinimize_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = (ToggleButton)sender;
            if ((bool)toggleButton.IsChecked)
                ContentFrameShow();
            else
                ContentFrameCollapse();
        }

        private void ContentFrameShow()
        {
            ContentFrame.Visibility = Visibility.Visible;
            ContentFrameRow.Height = new GridLength(1, GridUnitType.Star);
            ContentFrameMinimize.IsChecked = true;
        }

        private void ContentFrameCollapse()
        {
            ContentFrame.Visibility = Visibility.Collapsed;
            ContentFrameRow.Height = GridLength.Auto;
            ContentFrameMinimize.IsChecked = false;
        }

        private void ContentFrameClose_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame_Reset();
        }

        public void ContentFrame_Reset()
        {
            if (ContentFrame.CanGoBack)
                ContentFrame.GoBack();
            ContentFrame.Content = null;

            navigated = false;
            ContentFramePageName.Text = "";

            ContentFrameMinimize.IsEnabled = false;
            ContentFrameClose.IsEnabled = false;
            ContentFrameCollapse();
        }
    }
}
