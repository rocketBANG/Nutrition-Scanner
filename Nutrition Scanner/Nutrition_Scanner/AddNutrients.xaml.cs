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
            loading_data.IsRunning = true;

            ImageAnalyser im = new ImageAnalyser();

            Dictionary<string, string> nutrientDictionary = await im.makePredictionRequest(file);

            NutrientData nutrientDataPage = new NutrientData(nutrientDictionary);

            loading_data.IsRunning = false;

            await Navigation.PushModalAsync(nutrientDataPage);

        }




        private async void addManually(object sender, EventArgs e)
        {
            NutrientData nutrientDataPage = new NutrientData(null);
            await Navigation.PushModalAsync(nutrientDataPage);
        }
    }
}