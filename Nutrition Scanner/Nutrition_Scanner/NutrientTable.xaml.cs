using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Nutrition_Scanner
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NutrientTable : ContentPage
    {
        public NutrientTable()
        {
            InitializeComponent();
        }

        async void Handle_ClickedAsync(object sender, System.EventArgs e)
        {
            loading.IsRunning = true;
            List<NutrientModel> notHotDogInformation = await AzureManager.AzureManagerInstance.GetNutrientInfo(DateTime.Today);

            HotDogList.ItemsSource = notHotDogInformation;
            loading.IsRunning = false;
        }
    }
}