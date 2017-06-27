using Newtonsoft.Json.Linq;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Nutrition_Scanner
{
    class ImageAnalyser
    {
        const string subscriptionKey = "936a76423dd84d17aeb917e13039b7b3";
        const string uriBase = "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0/ocr";

        Dictionary<string, string> _nutrients;


        public ImageAnalyser()
        {
            _nutrients = new Dictionary<string, string>();
        }

        byte[] GetImageAsByteArray(MediaFile file)
        {
            var stream = file.GetStream();
            BinaryReader binaryReader = new BinaryReader(stream);
            return binaryReader.ReadBytes((int)stream.Length);
        }

        public async Task<Dictionary<string, string>> makePredictionRequest(MediaFile file)
        {
            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            // Request parameters. A third optional parameter is "details".
            string requestParameters = "detectOrientation=true";

            // Assemble the URI for the REST API Call.
            string uri = uriBase + "?" + requestParameters;

            HttpResponseMessage response;

            byte[] byteData = GetImageAsByteArray(file);

            using (var content = new ByteArrayContent(byteData))
            {

                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(uri, content);


                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();

                    JObject rss = JObject.Parse(responseString);

                    List<List<string>> regionStrings = new List<List<string>>();

                    //Querying with LINQ
                    //Get all Prediction Values
                    var regions = rss["regions"];
                    foreach (var region in regions)
                    {
                        List<string> regionString = new List<string>();

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

                            var words = item["words"];
                            string fullWord = "";
                            foreach (var word in words)
                            {
                                fullWord += word["text"];
                            }
                            regionString.Add(fullWord);
                        }

                        regionStrings.Add(regionString);
                    }

                    CheckString(regionStrings);
                }


                //Get rid of file once we have finished using it
                file.Dispose();
            }

            return _nutrients;
        }

        void CheckString(List<List<string>> regions)
        {
            List<string> items = new List<string>();
            List<string> values = new List<string>();

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
                    values = region;
                }
                else if (item > 2)
                {
                    items = region;
                }

            }


            foreach (var item in items)
            {
                Debug.WriteLine("Items: " + item);
            }


            var removeValues = new List<string>();

            foreach (var value in values)
            {
                Debug.WriteLine("Values: " + value);
                if (!(Regex.Match(value, "[0-9]").Success))
                {
                    removeValues.Add(value);
                }

            }

            foreach (var removeValue in removeValues)
            {
                values.Remove(removeValue);
            }

            for (int i = 0; i < Math.Min(values.Count, items.Count); i++)
            {
                Debug.WriteLine("Item: " + items.ElementAt(i) + "; Value: " + values.ElementAt(i));
            }

            _nutrients = items.Zip(values, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);

        }


    }
}
