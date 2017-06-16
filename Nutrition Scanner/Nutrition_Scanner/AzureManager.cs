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
        private IMobileServiceTable<NutrientConsumption> nutrientconsumptionTable;

        private AzureManager()
        {
            this.client = new MobileServiceClient("http://nutritionalscanner.azurewebsites.net");
            this.nutrientconsumptionTable = this.client.GetTable<NutrientConsumption>();
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

        public async Task<List<NutrientConsumption>> GetHotDogInformation()
        {
            return await this.nutrientconsumptionTable.ToListAsync();
        }

        public async Task PostHotDogInformation(NutrientConsumption notHotDogModel)
        {
            await this.nutrientconsumptionTable.InsertAsync(notHotDogModel);
        }

        public async Task UpdateHotDogInformation(NutrientConsumption notHotDogModel)
        {
            await this.nutrientconsumptionTable.UpdateAsync(notHotDogModel);
        }


        public async Task DeleteHotDogInformation(NutrientConsumption notHotDogModel)
        {
            await this.nutrientconsumptionTable.DeleteAsync(notHotDogModel);
        }


    }
}
