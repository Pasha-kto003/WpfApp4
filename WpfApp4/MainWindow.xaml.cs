using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        string url = @"http://localhost:53922/WeatherForecast";
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(List<WeatherData>));
            var request = WebRequest.Create(url);
            var response = request.GetResponse();
            using (var stream = response.GetResponseStream())
            {
                var answer = (List<WeatherData>)jsonSerializer.ReadObject(stream);
                listbox1.ItemsSource = null;
                listbox1.Items.Clear();
                listbox1.ItemsSource = answer;
            }
        }

        private void Button_ClickPost(object sender, RoutedEventArgs e)
        {
            var request = WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";
            var data = new WeatherData
            {
                //"2021-11-10T23:50:08.386Z"
                date = DateTime.UtcNow.ToBinary(),
                summary = "большой",
                temperatureC = 30,
                temperatureF = 86 
            };

            DataContractJsonSerializer dataContractJson = new DataContractJsonSerializer(typeof(WeatherData));
            using (var stream = request.GetRequestStream())
            {
                dataContractJson.WriteObject(stream, data);
            }

            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                Title = response.StatusCode.ToString();
                if(response.StatusCode == HttpStatusCode.OK)
                {
                    using (var stream = response.GetResponseStream())
                    {
                        var answer = (WeatherData)dataContractJson.ReadObject(stream);
                        listbox1.ItemsSource = null;
                        listbox1.Items.Add(answer);
                    }
                }

            }
            catch (Exception error)
            {

                MessageBox.Show(error.Message);
            }
           
        }

        WeatherData data;
        private void listSelect(object sender, SelectionChangedEventArgs e)
        {
            data = (WeatherData)listbox1.SelectedItem;
            datePicker.SelectedDate = DateTime.FromBinary(data.date);
            textC.Text = data.temperatureC.ToString();
            textSummary.Text = data.summary;
        }

        private void Button_Click_Put(object sender, RoutedEventArgs e)
        {
            if (data.id == 0) return;
            data.date = ((DateTime)datePicker.SelectedDate).ToBinary();
            data.temperatureC = int.Parse(textC.Text);
            data.summary = textSummary.Text;
            DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(WeatherData));
            var request = HttpWebRequest.Create(url);
            request.Method = "PUT";
            request.ContentType ="application/json";
            request.Headers.Set("20", "text/plain");
            using (var stream = request.GetRequestStream())
            {
                jsonSerializer.WriteObject(stream, data);
            }

            var response = request.GetResponse();
            string result = null;
            using (var stream = response.GetResponseStream())
            {
                byte[] array = new byte[response.ContentLength];
                stream.Read(array, 0, array.Length);
                result = Encoding.UTF8.GetString(array);
            }
            if (result == "false")
            {
                MessageBox.Show("Не удалось обновть объект на сервере");
                return;
            }
            else
            {
                listbox1.Items[listbox1.SelectedIndex] = data;
            }
        }
    }
    [Serializable]
    struct WeatherData
    {
        public int id;
        public long date;
        public int temperatureC;
        public int temperatureF;
        public string summary;
        public override string ToString()
        {
            return $"{DateTime.FromBinary(date).ToLongDateString()} {temperatureC} {temperatureF} {summary}";
        }
    }
    
}
