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
    public class ProductTests
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

        public ProductTests(ScenarioContext scenarioContext)
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

        [Given(@"I have a valid access token")]
        public void GivenIHaveAValidAccessToken()
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
/*
            //scnearioContext method:
            //string accessTokenResponseContent = _response.Content;

            //JObject jobj = JObject.Parse(accessTokenResponseContent);
            //string accessToken = jobj.SelectToken("accessToken").ToString();

            // Store the access token in ScenarioContext
            //_scenarioContext["accessToken"] = accessToken;*/

            GlobalObjects.AccessTokenResponseContent = _response.Content;

            JObject jobj = JObject.Parse(GlobalObjects.AccessTokenResponseContent);
            GlobalObjects.AccessToken = jobj.SelectToken("accessToken").ToString();
            Console.WriteLine("Access token: " + GlobalObjects.AccessToken);

            _apiClient.AddDefaultHeader("Authorization", $"Bearer {GlobalObjects.AccessToken}");
        }

        [When(@"I search for all products")]
        public void GivenISearchForAllProducts()
        {
            _request = new RestRequest("/products", Method.GET);
            _response = _apiClient.Execute(_request);
            Assert.AreEqual(200, (int)_response.StatusCode, "Verify if invalid request type/endpoint was entered");
            _products = JArray.Parse(_response.Content).ToObject<List<JObject>>();
            GlobalObjects.ResponseContent = _response.Content;
        }

        [Then(@"the product list should be returned with valid properties")]
        public void ThenTheProductListShouldBeReturnedWithValidProperties()
        {
            Assert.AreEqual(200, (int)_response.StatusCode);
            Assert.IsTrue(_products.Any());
            foreach (var product in _products)
            {
                Assert.IsTrue(product["id"].Type == JTokenType.Integer, "invalid id type");
                Assert.IsTrue(product["id"].ToString().Length == 4, "invalid id length");
                Assert.IsTrue(product["category"].Type == JTokenType.String, "invalid category type");
                Assert.IsTrue(product["name"].Type == JTokenType.String, "invalid name type");
                Assert.IsTrue(product["inStock"].Type == JTokenType.Boolean, "invalid inStock type");
            }
            Console.WriteLine(GlobalObjects.ResponseContent);

        }

        [When(@"I search all products by category (.*)")]
        public void WhenISearchAllProductsByCategory(string filter)
        {
            _scenarioContext["categoryFilter"] = filter;

            var parameter = "category";
            var parameterValue = _scenarioContext["categoryFilter"].ToString();

            _request = new RestRequest("/products", Method.GET);
            _request.AddParameter(parameter, parameterValue);

            _response = _apiClient.Execute(_request);
            GlobalObjects.ResponseContent = _response.Content;
            Assert.AreEqual(200, (int)_response.StatusCode, "Verify if invalid category/request type/endpoint was entered");
            _products = JArray.Parse(_response.Content).ToObject<List<JObject>>();
        }

        [Then(@"all products in the response should belong to that category")]
        public void ThenAllProductsInTheResponseShouldBelongToThatCategory()
        {
            var categoryFilter = _scenarioContext["categoryFilter"].ToString();

            Assert.IsTrue(_products.All(p => p["category"].ToString() == categoryFilter));
            Console.WriteLine($"All products belong to {categoryFilter} category");
            Console.WriteLine(GlobalObjects.ResponseContent);
        }

        [When(@"I search all products with results parameter set to (\d+)")]
        public void WhenISearchAllProductsWithResultsParameterSetTo(int numberOfResults)
        {
            _request = new RestRequest("/products", Method.GET);
            _request.AddParameter("results", numberOfResults.ToString());
            _response = _apiClient.Execute(_request);
            Assert.AreEqual(200, (int)_response.StatusCode, "Verify if invalid request type/endpoint was entered");
            GlobalObjects.ResponseContent = _response.Content;
            _products = JArray.Parse(_response.Content).ToObject<List<JObject>>();
        }

        [Then(@"only that number of results should be returned")]
        public void ThenOnlyThatNumberOfResultsShouldBeReturned()
        {
            int expectedNumberOfResults = int.Parse(_request.Parameters.Find(p => p.Name == "results").Value.ToString());
            Assert.AreEqual(expectedNumberOfResults, _products.Count, "Number of results did not match");
            Console.WriteLine($"{expectedNumberOfResults} results searched, {_products.Count} results returned");
            Console.WriteLine(GlobalObjects.ResponseContent);

        }

        [When(@"I search all products filtered by (.*)")]
        public void WhenISearchAllProductsFilteredBy(bool availability)
        {
            _scenarioContext["availabilityFilter"] = availability;

            var parameter = "available"; 
            var parameterValue = availability.ToString().ToLower(); 


            _request = new RestRequest("/products", Method.GET);
            _request.AddParameter(parameter, parameterValue);

            _response = _apiClient.Execute(_request);
            GlobalObjects.ResponseContent = _response.Content;
            Assert.AreEqual(200, (int)_response.StatusCode, "Verify if invalid category/request type/endpoint was entered");
            _products = JArray.Parse(_response.Content).ToObject<List<JObject>>();
        }

        [Then(@"only products with that availability should be returned")]
        public void ThenOnlyProductsWithThatAvailabilityShouldBeReturned()
        {
            var availabilityFilter = _scenarioContext["availabilityFilter"];

            Console.WriteLine(availabilityFilter);
            Console.WriteLine(_products);

            Assert.IsTrue(_products.All(p => (bool)p["inStock"] == (bool)availabilityFilter));
            Console.WriteLine($"Availability filter successful, all products displayed are available = {availabilityFilter}");
            Console.WriteLine(GlobalObjects.ResponseContent);
        }

        /*       [When(@"I set the category filter to (.*)")]
               public void WhenISetTheCategoryFilterToDairy(string categoryFilter)
               {
                   _scenarioContext["categoryFilter"] = categoryFilter;

                   var parameter = "category";
                   var parameterValue = _scenarioContext["categoryFilter"].ToString();

                   _request = new RestRequest("/products", Method.GET);
                   _request.AddParameter(parameter, parameterValue);

       *//*            _response = _apiClient.Execute(_request);
                   GlobalObjects.ResponseContent = _response.Content;
                   Assert.AreEqual(200, (int)_response.StatusCode, "Verify if invalid category/request type/endpoint was entered");
                   _products = JArray.Parse(_response.Content).ToObject<List<JObject>>();*//*
               }

               [When(@"I set the results filter to (.*)")]
               public void WhenISetTheResultsFilterTo(int resultsFilter)
               {
                   throw new PendingStepException();
               }

               [When(@"I set the availability filter to (.*)")]
               public void WhenISetTheAvailabilityFilterTo(bool availabilityFilter)
               {
                   throw new PendingStepException();
               }*/

        [When(@"I set the category filter to (.*)")]
        public void WhenISetTheCategoryFilterTo(string categoryFilter)
        {
            if (!string.IsNullOrWhiteSpace(categoryFilter))
            {
                _scenarioContext["categoryFilter"] = categoryFilter;
            }
        }

        [When(@"I set the results filter to (.*)")]
        public void WhenISetTheResultsFilterTo(string resultsFilter)
        {
            if (!string.IsNullOrWhiteSpace(resultsFilter) && int.TryParse(resultsFilter, out int parsedResult))
            {
                _scenarioContext["resultsFilter"] = parsedResult;
            }
        }


        [When(@"I set the availability filter to (.*)")]
        public void WhenISetTheAvailabilityFilterTo(string availabilityFilter)
        {
            if (!string.IsNullOrWhiteSpace(availabilityFilter))
            {
                _scenarioContext["availabilityFilter"] = availabilityFilter;
            }
        }



        [When(@"I execute the product search")]
        public void WhenIExecuteTheProductSearch()
        {
            string categoryFilter = _scenarioContext.ContainsKey("categoryFilter") ? _scenarioContext["categoryFilter"].ToString() : null;
            int? resultsFilter = _scenarioContext.ContainsKey("resultsFilter") ? int.Parse(_scenarioContext["resultsFilter"].ToString()) : null;
            string availabilityFilter = _scenarioContext.ContainsKey("availabilityFilter") ? _scenarioContext["availabilityFilter"].ToString() : null;

            _request = new RestRequest("/products", Method.GET);

            if (!string.IsNullOrEmpty(categoryFilter))
            {
                _request.AddParameter("category", categoryFilter);
            }

            if (resultsFilter.HasValue)
            {
                _request.AddParameter("results", resultsFilter.Value.ToString());
            }

            if (!string.IsNullOrEmpty(availabilityFilter))
            {
                _request.AddParameter("available", availabilityFilter.ToLower());
            }

            _response = _apiClient.Execute(_request);
            _products = JArray.Parse(_response.Content).ToObject<List<JObject>>();
            GlobalObjects.ResponseContent = _response.Content;
            Console.WriteLine("Search Results: " + GlobalObjects.ResponseContent);
        }


        [Then(@"the filtered products should match the criteria")]
        public void ThenTheFilteredProductsShouldMatchTheCriteria()
        {

            if (_scenarioContext.ContainsKey("categoryFilter"))
            {
                var categoryFilter = _scenarioContext["categoryFilter"].ToString();
                Assert.IsTrue(_products.All(p => p["category"].ToString() == categoryFilter));
                Console.WriteLine($"Products successfully filtered by category {categoryFilter}");
            }

            if (_scenarioContext.ContainsKey("resultsFilter"))
            {
                int expectedNumberOfResults = int.Parse(_request.Parameters.Find(p => p.Name == "results").Value.ToString());
                Assert.AreEqual(expectedNumberOfResults, _products.Count, "Number of results did not match");
                Console.WriteLine($"Products successfully filtered by results {_products.Count}");
            }

            if (_scenarioContext.ContainsKey("availabilityFilter"))
            {
                string availabilityFilter = _scenarioContext["availabilityFilter"].ToString().ToLower();
                bool expectedAvailability = availabilityFilter == "true";
                Assert.IsTrue(_products.All(p => (bool)p["inStock"] == expectedAvailability));
                Console.WriteLine($"Products successfully filtered by availability {availabilityFilter}");
            }
        }


    }
}

