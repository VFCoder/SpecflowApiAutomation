using AventStack.ExtentReports.Gherkin.Model;
using AventStack.ExtentReports;
using Hooks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.CommonModels;
using System.Reflection.Metadata;

namespace GroceryStoreApi.Steps.ApiSteps
{
    [Binding]
    public class CartTests
    {
        private readonly IConfiguration _configuration;
        public ScenarioContext _scenarioContext;
        private string _apiUrl;
        public RestClient _apiClient;
        public RestClient _client;
        public RestRequest _request;
        public IRestResponse _response;
        private GenericSteps _commonSteps;
        private readonly string _authUrl;
        private readonly string _baseUrl;
        private List<JObject> _products;

        public CartTests(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            _configuration = scenarioContext.Get<IConfiguration>("Configuration");
            var Configuration = new TestConfiguration().GetConfiguration();
            _apiUrl = Configuration.GetSection("ApiUrl").Value;
            _authUrl = Configuration["AuthUrl"];
            _apiClient = new RestClient(_apiUrl);
            _request = new RestRequest();
            _response = new RestResponse();
            _commonSteps = new GenericSteps(_scenarioContext);
        }

/*        [Given(@"I have created a cart")]
        public void GivenIHaveCreatedACart()
        {
            _request = new RestRequest("/carts", Method.POST);
            _response = _apiClient.Execute(_request);
            Assert.True(_response.IsSuccessful);
            GlobalObjects.ResponseContent = _response.Content;

            JObject jobj = JObject.Parse(GlobalObjects.ResponseContent);
            GlobalObjects.CartId = jobj.SelectToken("cartId").ToString();
            Console.WriteLine("Cart Id: " + GlobalObjects.CartId);

            GlobalObjects.AddOrUpdate("cartId", GlobalObjects.CartId);

        }*/

        [When(@"I get items from cart")]
        public void WhenIGetItemsFromCart()
        {
            _request = new RestRequest($"/carts/{GlobalObjects.CartId}/items", Method.GET);
            _response = _apiClient.Execute(_request);
            Assert.True(_response.IsSuccessful);
            GlobalObjects.ResponseContent = _response.Content;

        }


        [When(@"I add (.*) and (.*) to the cart")]
        public void WhenIAddAndToTheCart(string productId, string quantity)
        {
            int productIdInt = int.Parse(productId);
            int quantityInt = int.Parse(quantity);

            _request = new RestRequest($"/carts/{GlobalObjects.CartId}/items", Method.POST);

            object requestBody = new
            {
                productId = productIdInt,
                quantity = quantityInt
            };

            string requestBodyJson = JsonConvert.SerializeObject(requestBody);
            _request.AddJsonBody(requestBodyJson);
            _response = _apiClient.Execute(_request);

            int actualStatusCode = (int)_response.StatusCode;
            Assert.AreEqual(201, actualStatusCode, $"Status code is not 201");
            Console.WriteLine($"Status code is {actualStatusCode}");
        }


        [Then(@"the cart should contain those (.*) and (.*)")]
        public void ThenTheCartShouldContainThoseAnd(int productId, int quantity)
        {
            _request = new RestRequest($"/carts/{GlobalObjects.CartId}/items", Method.GET);
            _response = _apiClient.Execute(_request);
            Assert.True(_response.IsSuccessful);

            JToken responseContent = JToken.Parse(_response.Content);
            bool found = false;

            foreach (var item in responseContent)
            {
                int actualProductId = (int)item["productId"];
                int actualQuantity = (int)item["quantity"];

                if (actualProductId == productId && actualQuantity == quantity)
                {
                    found = true;
                    break; // Item found, no need to continue searching
                }
            }

            Assert.IsTrue(found, $"Cart should contain Product ID {productId} with Quantity {quantity}");
            Console.WriteLine($"Product ID: {productId}, Expected Quantity: {quantity} found in the cart");
            Console.WriteLine("Cart contents: " + _response.Content);
        }


    }
}

