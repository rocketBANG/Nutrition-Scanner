using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;

namespace Nutrition_Scanner
{
    public class AzureManager
    {

        private static AzureManager instance;
        private MobileServiceClient client;
        private IMobileServiceTable<NutrientModel> nutrientTable;

        private AzureManager()
        {
            this.client = new MobileServiceClient("http://nutritionalscanner.azurewebsites.net");
            this.nutrientTable = this.client.GetTable<NutrientModel>();
        }

        public MobileServiceClient AzureClient
        {
            get { return client; }
        }

        public static AzureManager AzureManagerInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AzureManager();
                }

                return instance;
            }
        }

        public async Task<List<NutrientModel>> GetAllNutrientInfo()
        {
            return await this.nutrientTable.ToListAsync();
        }


        public async Task<List<NutrientModel>> GetNutrientInfo(DateTime time)
        {
            return await this.nutrientTable.Where(nutrientModel => nutrientModel.Date == time).ToListAsync();
        }

        public async Task<List<NutrientModel>> GetNutrientInfo(string nutrient)
        {
            return await this.nutrientTable.Where(nutrientModel => nutrientModel.Nutrient == nutrient).ToListAsync();
        }

        public async Task PostNutrientInfo(NutrientModel nutrientModel)
        {
            await this.nutrientTable.InsertAsync(nutrientModel);
        }

        public async Task UpdateNutrientInfo(NutrientModel nutrientModel)
        {
            await this.nutrientTable.UpdateAsync(nutrientModel);
        }


        public async Task DeleteNutrientInfo(NutrientModel nutrientModel)
        {
            await this.nutrientTable.DeleteAsync(nutrientModel);
        }


    }
}
