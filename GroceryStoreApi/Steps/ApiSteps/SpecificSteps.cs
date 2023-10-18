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

namespace GroceryStoreApi.Steps.ApiSteps
{
    [Binding]
    public class SpecificSteps
    {
        private readonly IConfiguration _configuration;
        public ScenarioContext _scenarioContext;
        private string _apiUrl;
        public RestClient _apiClient;
        public RestClient _client;
        public RestRequest _request;
        public IRestResponse _response;
        private GeneralSteps _commonSteps;
        private readonly string _authUrl;
        private readonly string _baseUrl;

        public SpecificSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            _configuration = scenarioContext.Get<IConfiguration>("Configuration");
            var Configuration = new TestConfiguration().GetConfiguration();
            _apiUrl = Configuration.GetSection("ApiUrl").Value;
            _authUrl = Configuration["AuthUrl"];
            _apiClient = new RestClient(_apiUrl);
            _request = new RestRequest();
            _response = new RestResponse();
            _commonSteps = new GeneralSteps(_scenarioContext);
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

        [Given(@"I have created a cart and added an item")]
        public void GivenIHaveCreatedACartAndAddedAnItem()
        {
            //GivenIHaveCreatedACart();

            _request = new RestRequest($"/carts/{GlobalObjects.CartId}/items", Method.POST);

            int productId = 4646;
            int quantity = 1;

            object requestBody = new
            {
                productId,
                quantity
            };

            string requestBodyJson = JsonConvert.SerializeObject(requestBody);
            _request.AddJsonBody(requestBodyJson);
            _response = _apiClient.Execute(_request);
            Assert.True(_response.IsSuccessful);
            GlobalObjects.ResponseContent = _response.Content;

            JObject jobj = JObject.Parse(GlobalObjects.ResponseContent);
            GlobalObjects.ItemId = jobj.SelectToken("itemId").ToString();
            Console.WriteLine("Item Id: " + GlobalObjects.ItemId);

            GlobalObjects.AddOrUpdate("itemId", GlobalObjects.ItemId);
        }


        [When(@"I add the following items to the cart:")]
        public void GivenIAddTheFollowingItemsToTheCart(Table table)
        {
            foreach (var row in table.Rows)
            {
                int productId = int.Parse(row["Product ID"]);
                int quantity = int.Parse(row["Quantity"]);

                _request = new RestRequest($"/carts/{GlobalObjects.CartId}/items", Method.POST);

                object requestBody = new
                {
                    productId,
                    quantity
                };

                string requestBodyJson = JsonConvert.SerializeObject(requestBody);
                _request.AddJsonBody(requestBodyJson);
                _response = _apiClient.Execute(_request);

                int actualStatusCode = (int)_response.StatusCode;
                Assert.AreEqual(201, actualStatusCode, $"Status code is not 201");
                Console.WriteLine($"Status code is {actualStatusCode}");

                Console.WriteLine(_response.Content);
            }
        }

        [Given(@"I have created a new order")]
        public void GivenIHaveCreatedANewOrder()
        {
            GivenIHaveCreatedACartAndAddedAnItem();

            string cartId = GlobalObjects.CartId;
            string customerName = Helpers.RandomString(10);

            object orderRequestBody = new
            {
                cartId,
                customerName
            };

            _request = new RestRequest($"/orders", Method.POST);
            string orderRequestBodyJson = JsonConvert.SerializeObject(orderRequestBody);
            _request.AddJsonBody(orderRequestBodyJson);
            _request.AddHeader("Authorization", $"Bearer {GlobalObjects.AccessToken}");

            _response = _apiClient.Execute(_request);
            Assert.True(_response.IsSuccessful);
            GlobalObjects.ResponseContent = _response.Content;

            JObject jobj = JObject.Parse(GlobalObjects.ResponseContent);
            GlobalObjects.OrderId = jobj.SelectToken("orderId").ToString();
            Console.WriteLine("Order Id: " + GlobalObjects.OrderId);

            GlobalObjects.AddOrUpdate("orderId", GlobalObjects.OrderId);

        }

        [When(@"I get items from cart")]
        public void WhenIGetItemsFromCart()
        {
            _request = new RestRequest($"/carts/{GlobalObjects.CartId}/items", Method.GET);
            _response = _apiClient.Execute(_request);
            Assert.True(_response.IsSuccessful);
            GlobalObjects.ResponseContent = _response.Content;

        }

        [Then(@"the cart should now contain the following items:")]
        public void ThenTheCartShouldNowContainTheFollowingItems(Table table)
        {
            WhenIGetItemsFromCart();

            JToken responseContent = JToken.Parse(GlobalObjects.ResponseContent);

            foreach (var row in table.Rows)
            {
                int productId = int.Parse(row["Product ID"]);
                int expectedQuantity = int.Parse(row["Quantity"]);

                JToken cartItem = responseContent.FirstOrDefault(item => (int)item["productId"] == productId);

                Assert.NotNull(cartItem, $"Cart should contain Product ID {productId}");
                int actualQuantity = (int)cartItem["quantity"];
                Assert.AreEqual(expectedQuantity, actualQuantity, $"Quantity for Product ID {productId} is incorrect");

                Console.WriteLine($"Product ID: {productId}, Expected Quantity: {expectedQuantity}, Actual Quantity: {actualQuantity}");

            }
            Console.WriteLine("Cart contents: " + GlobalObjects.ResponseContent);
        }



        [Then(@"the cart should now have the following properties:")]
        public void ThenTheCartShouldNowHaveTheFollowingProperties(Table table)
        {
            WhenIGetItemsFromCart();

            JToken jtoken = JToken.Parse(GlobalObjects.ResponseContent);

            if (jtoken is JArray)
            {
                JArray jarray = (JArray)jtoken;

                foreach (var item in jarray)
                {
                    JObject jobj = (JObject)item;

                    CheckProperties(jobj, table);
                }
            }
            else if (jtoken is JObject)
            {
                JObject jobj = (JObject)jtoken;

                CheckProperties(jobj, table);
            }
            else
            {
                Assert.Fail("Unexpected JSON response format: " + jtoken.GetType().Name);
            }
        }

        private void CheckProperties(JObject jobj, Table table)
        {
            Console.WriteLine("API Response Object: " + jobj);

            foreach (var row in table.Rows)
            {
                string propertyName = row["Property"];
                string expectedValue = row["Expected Value"];
                string storeProperty = row["Store Property"];

                JToken propertyValue = jobj.SelectToken(propertyName);

                if (!string.IsNullOrEmpty(expectedValue))
                {
                    if (expectedValue.Contains("<") && expectedValue.Contains(">"))
                    {
                        string placeholder = expectedValue.Substring(expectedValue.IndexOf("<") + 1, expectedValue.IndexOf(">") - expectedValue.IndexOf("<") - 1);

                        if (GlobalObjects.Get(placeholder) != null)
                        {
                            expectedValue = expectedValue.Replace($"<{placeholder}>", GlobalObjects.Get(placeholder));
                        }
                    }

                    Console.WriteLine($"Property: {propertyName}, Expected Value: {expectedValue}, Actual Value: {propertyValue}");
                    Assert.AreEqual(expectedValue, propertyValue?.ToString());
                }

                if (!string.IsNullOrEmpty(storeProperty))
                {
                    Console.WriteLine($"Property: {propertyName}, Stored Value: {propertyValue}");
                    GlobalObjects.AddOrUpdate(storeProperty, propertyValue?.ToString());
                }
            }
        }

    }
}
