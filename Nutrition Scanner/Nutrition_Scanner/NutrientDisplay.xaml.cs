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
    public partial class NutrientDisplay : ContentPage
    {
        public NutrientDisplay()
        {
            InitializeComponent();
        }

        async void loadNutrients(object sender, System.EventArgs e)
        {
            loading.IsRunning = true;
            List<NutrientModel> nutrientInformation = await AzureManager.AzureManagerInstance.GetNutrientInfo(DateTime.Today);

            NutrientList.ItemsSource = nutrientInformation;
            loading.IsRunning = false;
        }

        private async void clearNutrients(object sender, EventArgs e)
        {
            await AzureManager.AzureManagerInstance.ClearNutrientInfo(DateTime.Today);
        }
    }
}