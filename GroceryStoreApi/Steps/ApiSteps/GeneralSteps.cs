using Hooks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using RestSharp;
using TechTalk.SpecFlow;


namespace GroceryStoreApi.Steps.ApiSteps
{
    [Binding]
    public class GeneralSteps
    {
        private readonly IConfiguration _configuration;
        public ScenarioContext _scenarioContext;
        private string _apiUrl;
        public RestClient _apiClient;
        public RestClient _client;
        public RestRequest _request;
        public IRestResponse _response;
        private readonly string _authUrl;
        private readonly string _baseUrl;


        public GeneralSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            _configuration = scenarioContext.Get<IConfiguration>("Configuration");
            var Configuration = new TestConfiguration().GetConfiguration();
            _apiUrl = Configuration.GetSection("ApiUrl").Value;
            _authUrl = Configuration["AuthUrl"];
            _apiClient = new RestClient(_apiUrl);
            _request = new RestRequest();
            _response = new RestResponse();
        }

        [Given(@"I get the API access token")]
        public void GivenIGetTheAPIAccessToken()
        {

            string clientName = Helpers.RandomString(10);
            string clientEmail = Helpers.RandomEmail(10);

            var authRequestBody = new
            {
                clientName,
                clientEmail
            };

            _request = new RestRequest("/api-clients", Method.POST);
            _request.AddJsonBody(authRequestBody);
            _response = _apiClient.Execute(_request);
            Assert.True(_response.IsSuccessful);

            int actualStatusCode = (int)_response.StatusCode;
            Assert.AreEqual(201, actualStatusCode, $"Status code is not 201");
            Console.WriteLine($"Status code is {actualStatusCode}");

            GlobalObjects.AccessTokenResponseContent = _response.Content;

            JObject jobj = JObject.Parse(GlobalObjects.AccessTokenResponseContent);
            GlobalObjects.AccessToken = jobj.SelectToken("accessToken").ToString();
            Console.WriteLine(GlobalObjects.AccessToken);

            _apiClient.AddDefaultHeader("Authorization", $"Bearer {GlobalObjects.AccessToken}");
        }



        [Given(@"I set a (GET|POST|PUT|PATCH|DELETE) request at the API endpoint (.+)")]
        public void GivenISetRequestAtTheAPIEndpoint(string httpMethod, string endpoint)
        {
            _request = new RestRequest(endpoint, GetHttpMethod(httpMethod));

        }

        [Given(@"I set a (GET|POST|PUT|PATCH|DELETE) request at the API path variable endpoint (.+)")]
        public void GivenISetRequestAtTheAPIPathVariableEndpoint(string httpMethod, string endpoint)
        {
            string replacedEndpoint = ReplacePropertyPlaceholderWithActualValue(endpoint);

            _request = new RestRequest(replacedEndpoint, GetHttpMethod(httpMethod));

            Console.WriteLine("Endpoint: " + replacedEndpoint);
        }
        private string ReplacePropertyPlaceholderWithActualValue(string endpoint)
        {
            return endpoint.Replace("<:cartId>", GlobalObjects.CartId)
                           .Replace("<:itemId>", GlobalObjects.ItemId)
                           .Replace("<:orderId>", GlobalObjects.OrderId);
        }

        private Method GetHttpMethod(string httpMethod)
        {
            switch (httpMethod.ToUpper())
            {
                case "GET":
                    return Method.GET;
                case "POST":
                    return Method.POST;
                case "PUT":
                    return Method.PUT;
                case "PATCH":
                    return Method.PATCH;
                case "DELETE":
                    return Method.DELETE;
                default:
                    return Method.GET;
            }
        }


        [When(@"I input the following values in the request body:")]
        public void GivenIInputTheFollowingValuesInTheRequestBody(Table table)
        {

            var requestBody = new Dictionary<string, string>();

            foreach (var row in table.Rows)
            {
                string propertyName = row["Property"];
                string propertyValue = row["Value"];

                if (propertyValue.Contains("<") && propertyValue.Contains(">"))
                {
                    string placeholder = propertyValue.Substring(propertyValue.IndexOf("<") + 1, propertyValue.IndexOf(">") - propertyValue.IndexOf("<") - 1);

                    if (GlobalObjects.Get(placeholder) != null)
                    {
                        propertyValue = propertyValue.Replace($"<{placeholder}>", GlobalObjects.Get(placeholder));
                    }
                }

                requestBody.Add(propertyName, propertyValue);
            }

            _request.AddJsonBody(requestBody);

        }

        [When(@"I send the API request")]
        public void WhenISendTheAPIRequest()
        {
            _response = _apiClient.Execute(_request);
            GlobalObjects.ResponseContent = _response.Content;
            Assert.True(_response.IsSuccessful);
            //Console.WriteLine(GlobalObjects.ResponseContent);

        }

        [Then(@"the API should return the status (\d+)")]
        public void ThenTheAPIShouldReturnTheStatus(int expectedStatusCode)
        {
            int actualStatusCode = (int)_response.StatusCode;
            Assert.AreEqual(expectedStatusCode, actualStatusCode, $"Status code is not {expectedStatusCode}");
            Console.WriteLine($"Status code is {actualStatusCode}");


        }

        /*
                [Then(@"the API response object should have the following properties:")]
                public void ThenTheAPIResponseShouldHaveTheFollowingProperties(Table table)
                {
                    JObject jobj = JObject.Parse(GlobalObjects.ResponseContent);
                    Console.WriteLine("API Response Object: " + jobj);

                    foreach (var row in table.Rows)
                    {
                        string propertyName = row["Property"];
                        string expectedValue = row["Expected Value"];
                        string storeProperty = row["Store Property"];

                        JToken propertyValue = jobj.SelectToken(propertyName);

                        if (!string.IsNullOrEmpty(expectedValue))
                        {
                            Console.WriteLine($"Property: {propertyName}, Expected Value: {expectedValue}, Actual Value: {propertyValue}");
                            Assert.AreEqual(expectedValue, propertyValue?.ToString());
                        }

                        if (!string.IsNullOrEmpty(storeProperty))
                        {
                            Console.WriteLine($"Property: {propertyName}, Stored Value: {propertyValue}");
                            GlobalObjects.AddOrUpdate(storeProperty, propertyValue?.ToString());
                        }

                    }
                }*/

        [Then(@"the API response object should have the following properties:")]
        public void ThenTheAPIResponseShouldHaveTheFollowingProperties(Table table)
        {
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