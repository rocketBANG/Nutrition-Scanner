using Newtonsoft.Json.Linq;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Nutrition_Scanner
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddNutrients : ContentPage
    {
        public AddNutrients()
        {
            InitializeComponent();
        }
        const string subscriptionKey = "936a76423dd84d17aeb917e13039b7b3";
        const string uriBase = "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0/ocr";

        private async void loadFromCamera(object sender, EventArgs e)
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

            ld_prediction.IsRunning = true;
            getNutrients(file);
            ld_prediction.IsRunning = false;

        }

        private async void loadFromGallery(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();

            MediaFile file = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
            {
                PhotoSize = PhotoSize.Medium
            });


            if (file == null)
                return;

            image.Source = ImageSource.FromStream(() =>
            {
                return file.GetStream();
            });

            ld_prediction.IsRunning = true;
            getNutrients(file);
            ld_prediction.IsRunning = false;
        }

        private async void getNutrients(MediaFile file)
        {
            ImageAnalyser im = new ImageAnalyser();

            Dictionary<string, string> nutrientDictionary = await im.makePredictionRequest(file);

            foreach (KeyValuePair<string, string> nutrientPair in nutrientDictionary)
            {
                AzureManager am = AzureManager.AzureManagerInstance;
                NutrientModel nm = new NutrientModel()
                {
                    Date = DateTime.Today,
                    Nutrient = nutrientPair.Key,
                    Value = nutrientPair.Value
                };
                List<NutrientModel> existing = await am.GetNutrientInfo(nutrientPair.Key);
                if (existing.Count > 0)
                {
                    NutrientModel newModel = existing[0];
                    newModel.Value += nm.Value;
                    await am.UpdateNutrientInfo(newModel);
                }
                else
                {
                    await am.PostNutrientInfo(nm);
                }
            }
        }




        private async void addManually(object sender, EventArgs e)
        {

        }
    }
}