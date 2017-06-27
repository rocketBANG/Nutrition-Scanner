using Nutrition_Scanner.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Nutrition_Scanner
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NutrientData : ContentPage
    {
        ObservableCollection<NutrientCellModel> _nutrients;

        public NutrientData(Dictionary<string, string> data)
        {
            InitializeComponent();

            ListView listView = new ListView();

            listView.ItemTemplate = new DataTemplate(typeof(NutrientCell));
            listView.ItemSelected += OnSelection;

            _nutrients = new ObservableCollection<NutrientCellModel>();

            listView.ItemsSource = _nutrients;

            mainLayout.Children.Add(listView);
            

            if(data != null)
            {
                foreach (var nutrient in data)
                {
                    string[] possibleValues = Regex.Split(nutrient.Value, "[^0-9.]+");
                    string valueString = possibleValues[0] != "" ? possibleValues[0] : possibleValues[1];
                    double value = Double.Parse(valueString);
                    _nutrients.Add(new NutrientCellModel { Name = nutrient.Key, Value = value });
                }
            }

            Button addButton = new Button();
            addButton.Text = "Add new";
            addButton.Clicked += addNewCell;

            Button submitButton = new Button();
            submitButton.Text = "Submit";
            submitButton.Clicked += submitDataAsync;

            Button closeButton = new Button();
            closeButton.Text = "Close";
            closeButton.Clicked += closePage;

            mainLayout.Children.Add(addButton);
            mainLayout.Children.Add(submitButton);
            mainLayout.Children.Add(closeButton);

        }

        private async void closePage(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        private async void OnSelection(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
            {
                return; //ItemSelected is called on deselection, which results in SelectedItem being set to null
            }

           if( await DisplayAlert("Delete?", "", "OK", "cancel"))
            {
                _nutrients.Remove((NutrientCellModel)e.SelectedItem);
            }

            //((ListView)sender).SelectedItem = null; //uncomment line if you want to disable the visual selection state.
        }


        public void removeCell(View cell)
        {
            mainLayout.Children.Remove(cell);
        }


        private async void submitDataAsync(object sender, EventArgs e)
        {
            foreach (var nutrient in _nutrients)
            {
                AzureManager am = AzureManager.AzureManagerInstance;
                NutrientModel nm = new NutrientModel()
                {
                    Date = DateTime.Today,
                    Nutrient = nutrient.Name,
                    Value = nutrient.Value
                };
                List<NutrientModel> existing = await am.GetNutrientInfo(nutrient.Name, DateTime.Today);
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

            await Navigation.PopModalAsync();

        }

        private void addNewCell(object sender, EventArgs e)
        {
            _nutrients.Add(new NutrientCellModel { Name = "", Value = 0 });
        }

    }
}