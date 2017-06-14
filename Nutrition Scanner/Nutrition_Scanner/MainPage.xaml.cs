using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace Nutrition_Scanner
{
    public partial class MainPage : ContentPage
    {
        const string subscriptionKey = "936a76423dd84d17aeb917e13039b7b3";
        const string uriBase = "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0/ocr";

        public MainPage()
        {
            InitializeComponent();

            BoxView view = new BoxView();
        }

        private async void loadCamera(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", ":( No camera available.", "OK");
                return;
            }

            MediaFile file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                AllowCropping = true,
                PhotoSize = PhotoSize.Medium,
                Directory = "Sample",
                Name = $"{DateTime.UtcNow}.jpg"
            });

            if (file == null)
                return;

            image.Source = ImageSource.FromStream(() =>
            {
                return file.GetStream();
            });

            double width = image.Width;
            double height = image.Height;

            TagLabel.Text = "";

            await MakePredictionRequest(file);
        }

        static byte[] GetImageAsByteArray(MediaFile file)
        {
            var stream = file.GetStream();
            BinaryReader binaryReader = new BinaryReader(stream);
            return binaryReader.ReadBytes((int)stream.Length);
        }

        async Task MakePredictionRequest(MediaFile file)
        {
            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            // Request parameters. A third optional parameter is "details".
            string requestParameters = "detectOrientation=true";

            // Assemble the URI for the REST API Call.
            string uri = uriBase + "?" + requestParameters;

            HttpResponseMessage response;

            byte[] byteData = GetImageAsByteArray(file);

            TagLabel.Text = "test";

            using (var content = new ByteArrayContent(byteData))
            {

                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(uri, content);


                if (response.IsSuccessStatusCode)
                {
                    TagLabel.Text = "sup";
                    var responseString = await response.Content.ReadAsStringAsync();

                    JObject rss = JObject.Parse(responseString);

                    

                    //Querying with LINQ
                    //Get all Prediction Values
                    var regions = rss["regions"];
                    var lines0 = regions[0]["lines"];
                    var boundingBoxes = from p in regions[0]["lines"] select (string)p["boundingBox"];
                    //var Tag = from p in rss["Predictions"] select (string)p["Tag"];

                    ////Truncate values to labels in XAML
                    //foreach (var item in Tag)
                    //{
                    //    TagLabel.Text += item + ": \n";
                    //}
                    foreach (var item in lines0)
                    {
                        var boundingBox = (string)item["boundingBox"];
                        PredictionLabel.Text += "\n" + boundingBox + "\n";
                        var words = item["words"];
                        foreach (var word in words)
                        {
                            PredictionLabel.Text += word["boundingBox"] + "\n";
                            PredictionLabel.Text += word["text"] + "\n";
                        }
                    }

                }

                //Get rid of file once we have finished using it
                file.Dispose();
            }
        }
    }

}
