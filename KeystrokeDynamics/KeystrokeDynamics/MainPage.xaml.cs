using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KeystrokeDynamics.Models;
using Windows.System;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace KeystrokeDynamics
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private string TextToType = "The quick brown fox jumps over a lazy dog".ToUpper();
        private int currentLetter = 0;
        StringBuilder stringBuilder;
        StringBuilder stringBuilder2;

        public MainPage()
        {
            this.InitializeComponent();
            TextToRegister.Text = TextToType;
            stringBuilder = new StringBuilder(TextToType);
            stringBuilder2 = new StringBuilder();
        }

        public List<KeystrokeData> Data = new List<KeystrokeData>();
        public Dictionary<char, long> Vector { get; set; }

        KeystrokeAcquisitionData KeystrokeAcquisitionData;
        private void TextToRegister_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key >= VirtualKey.A && e.Key <= VirtualKey.Z)
            {
                KeystrokeAcquisitionData = new KeystrokeAcquisitionData()
                {
                    Key = (char)(e.Key - VirtualKey.A + 'A'),
                    KeyDownTimeStamp = DateTime.Now
                };
            }
        }

        private async void TextToRegister_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            TextBox s = (TextBox)sender;
            if (KeystrokeAcquisitionData != null)
            {
                KeystrokeAcquisitionData.KeyUpTimeStamp = DateTime.Now;

                char k = (char)(e.Key - VirtualKey.A + 'A');
                if (k == stringBuilder[currentLetter])
                {
                    stringBuilder[currentLetter++] = ' ';
                    stringBuilder2.Append(k);

                    if (currentLetter >= TextToType.Length)
                    {
                        currentLetter = 0;
                        stringBuilder2.Clear();
                        stringBuilder = new StringBuilder(TextToType);

                        var xxx = await Task.Run(() => Cdd());
                        datagrid.ItemsSource = await Task.Run(() => xxx.Select(p => new { Letter = p.Key, Ticks = p.Value }).OrderBy(x => x.Letter).ToList());
                    }
                    else if (stringBuilder[currentLetter] == ' ')
                    {
                        ++currentLetter;
                        stringBuilder2.Append(' ');
                    }


                }
                TextToRegister.Text = stringBuilder.ToString();
                s.Text = stringBuilder2.ToString();
                s.SelectionStart = s.Text.Length;
                s.SelectionLength = 0;

                Data.Add(KeystrokeAcquisitionData.ToKeystrokeData());
                KeystrokeAcquisitionData = null;
            }
        }

        private void Register_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Vector = Cdd();
        }


        private Dictionary<char, long> Cdd()
        {
            return Data.GroupBy(x => x.Key)
                                             .Select(g => new
                                             {
                                                 Letter = g.Key,
                                                 Average = (long)g.Average(x => x.DwellTime.Ticks)
                                             })
                                             .ToDictionary(x => x.Letter, x => x.Average);
        }
    }
}
