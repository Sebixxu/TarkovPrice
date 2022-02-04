using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using TarkovMarket.Classes;

namespace TarkovMarket
{
    public class TarkovMarketApi : ITarkovApi
    {
        private static string _apiKey;
        private const string Url = "https://tarkov-market.com";
        private const string UrlParameters = "?x-api-key=";

        public TarkovMarketApi(string apiKey)
        {
            _apiKey = apiKey;
        }

        public async Task<GetAllItemsResult> GetAllItems()
        {
            string link = "/api/v1/items/all";
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(Url + link);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            // List data response.
            var response = await client.GetAsync(UrlParameters + _apiKey);  // Blocking call! Program will wait here until a response is received or a timeout occurs.

            // Dispose once all HttpClient calls are complete. This is not necessary if the containing object will be disposed of; for example in this case the HttpClient instance will be disposed automatically when the application terminates so the following call is superfluous.
            client.Dispose();

            return new GetAllItemsResult
            {
                TarkovMarketItemDatas = response.Content.ReadAsAsync<IEnumerable<TarkovMarketItemData>>().Result,
                Status = response.IsSuccessStatusCode ? Status.Ok : Status.Failed,
                Message = response.ReasonPhrase
            };
        }
    }
}
