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
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.WindowsAzure.MobileServices;

namespace Nutrition_Scanner
{
    public partial class MainPage : ContentPage
    {
        const string subscriptionKey = "936a76423dd84d17aeb917e13039b7b3";
        const string uriBase = "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0/ocr";

        List<string> Items;
        List<string> Values;

        public MainPage()
        {
            InitializeComponent();

            BoxView view = new BoxView();

        }

        private async void loadCamera(object sender, EventArgs e)
        {
            MobileServiceClient client = AzureManager.AzureManagerInstance.AzureClient;

            NutrientConsumption nutrition = new NutrientConsumption()
            {
                ID = "goodbye",
                Longitude = 0.320f,
                Latitude = 0.69f
            };

            await AzureManager.AzureManagerInstance.PostHotDogInformation(nutrition);

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

        private async void loadGallery(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();

            //if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            //{
            //    await DisplayAlert("No Camera", ":( No camera available.", "OK");
            //    return;
            //}

            MediaFile file = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
            {
                PhotoSize = PhotoSize.Medium
            });

            //MediaFile file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            //{
            //    AllowCropping = true,
            //    PhotoSize = PhotoSize.Medium,
            //    Directory = "Sample",
            //    Name = $"{DateTime.UtcNow}.jpg"
            //});

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

                    List<List<string>> regionStrings = new List<List<string>>();

                    //Querying with LINQ
                    //Get all Prediction Values
                    var regions = rss["regions"];
                    foreach(var region in regions)
                    {
                        List<string> regionString = new List<string>();
                        PredictionLabel.Text += "\n\n" + "NEW REGION" + "\n";
                        var lines = region["lines"];
                        var boundingBoxes = from p in lines select (string)p["boundingBox"];
                        //var Tag = from p in rss["Predictions"] select (string)p["Tag"];

                        ////Truncate values to labels in XAML
                        //foreach (var item in Tag)
                        //{
                        //    TagLabel.Text += item + ": \n";
                        //}
                        foreach (var item in lines)
                        {
                            var boundingBox = (string)item["boundingBox"];
                            PredictionLabel.Text += "\n" + boundingBox + "\n";
                            var words = item["words"];
                            string fullWord = "";
                            foreach (var word in words)
                            {
                                PredictionLabel.Text += word["boundingBox"] + "\n";
                                PredictionLabel.Text += word["text"] + "\n";
                                fullWord += word["text"];
                            }
                            regionString.Add(fullWord);
                        }

                        regionStrings.Add(regionString);
                    }


                    Debug.WriteLine(JsonPrettyPrint(responseString));
                    checkString(regionStrings);
                }


                //Get rid of file once we have finished using it
                file.Dispose();
            }
        }

        void checkString(List<List<string>> regions)
        {
            Items = new List<string>();
            Values = new List<string>();

            PredictionLabel.Text = "";
            foreach (var region in regions)
            {
                int g = 0;
                int item = 0;

                foreach (var textString in region)
                {
                    var lowerString = textString.ToLower();
                    if (lowerString.Contains("energy") || lowerString.Contains("protein") || lowerString.Contains("fat") || lowerString.Contains("salt") || lowerString.Contains("sodium") ||
                        lowerString.Contains("sugar") || lowerString.Contains("carbohydrate"))
                    {
                        item++;
                    }
                    else if (Regex.Match(textString, "[0-9]").Success)
                    {
                        g++;
                    }
                    else
                    {
                        Debug.WriteLine("did not match: " + textString);
                    }
                }

                if (g > 3)
                {
                    Values = region;
                }
                else if (item > 2)
                {
                    Items = region;
                }

            }


            foreach (var item in Items)
            {
                Debug.WriteLine("Items: " + item);
            }


            var removeValues = new List<string>();

            foreach (var value in Values)
            {
                Debug.WriteLine("Values: " + value);
                if (!(Regex.Match(value, "[0-9]").Success))
                {
                    removeValues.Add(value);
                }

            }

            foreach (var removeValue in removeValues)
            {
                Values.Remove(removeValue);
            }

            for (int i = 0; i < Math.Min(Values.Count, Items.Count); i++)
            {
                PredictionLabel.Text += "Item: " + Items.ElementAt(i) + "; Value: " + Values.ElementAt(i) + "\n";
                Debug.WriteLine("Item: " + Items.ElementAt(i) + "; Value: " + Values.ElementAt(i));
            }


        }

        static string JsonPrettyPrint(string json)
        {
            if (string.IsNullOrEmpty(json))
                return string.Empty;

            json = json.Replace(Environment.NewLine, "").Replace("\t", "");

            StringBuilder sb = new StringBuilder();
            bool quote = false;
            bool ignore = false;
            int offset = 0;
            int indentLength = 3;

            foreach (char ch in json)
            {
                switch (ch)
                {
                    case '"':
                        if (!ignore) quote = !quote;
                        break;
                    case '\'':
                        if (quote) ignore = !ignore;
                        break;
                }

                if (quote)
                    sb.Append(ch);
                else
                {
                    switch (ch)
                    {
                        case '{':
                        case '[':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', ++offset * indentLength));
                            break;
                        case '}':
                        case ']':
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', --offset * indentLength));
                            sb.Append(ch);
                            break;
                        case ',':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', offset * indentLength));
                            break;
                        case ':':
                            sb.Append(ch);
                            sb.Append(' ');
                            break;
                        default:
                            if (ch != ' ') sb.Append(ch);
                            break;
                    }
                }
            }

            return sb.ToString().Trim();
        }
    }

}
