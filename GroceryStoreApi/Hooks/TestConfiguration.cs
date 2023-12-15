using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;

namespace Hooks
{
    public class TestConfiguration
    {
        public IConfiguration GetConfiguration()
        {
            using (var file = File.OpenText("Properties\\launchSettings.json"))     // Open the "launchSettings.json" file for reading
            {
                
                var reader = new JsonTextReader(file);      // Create a JsonTextReader to read the JSON content from the file
                var jObject = JObject.Load(reader);     // Load the JSON content into a JObject

                // Extract the environment variables from the "profiles" section
                var variables = jObject.GetValue("profiles")    // get the value associated with the "profiles" key in the JSON
                   .SelectMany(profiles => profiles.Children())     //flatten the list of child elements under "profiles."
                   .SelectMany(profile => profile.Children<JProperty>())    //flatten the list of properties within each profile.
                   .Where(prop => prop.Name == "environmentVariables")      //filter properties with the name "environmentVariables."
                   .SelectMany(prop => prop.Value.Children<JProperty>())    //flatten the list of properties within "environmentVariables."
                   .ToList();   //convert the filtered properties into a list

                // Set the environment variables based on the extracted values
                foreach (var variable in variables)
                {
                    Environment.SetEnvironmentVariable(variable.Name, variable.Value.ToString());
                }
            }

            var environment = Environment.GetEnvironmentVariable("ENVIRONMENT");    // Get the value of the "ENVIRONMENT" environment variable
            var fileName = $"appsettings.{environment}.json";   // Create the appsettings filename based on the environment
            return new ConfigurationBuilder().AddJsonFile(fileName).Build();    // Build a configuration using ConfigurationBuilder

        }
    }

    public class TestConfigurationRefactored
    {
        public IConfiguration GetConfiguration()
        {
            var launchSettings = LoadLaunchSettings();  // Load the JSON from launchSettings.json
            var environmentVariables = GetEnvironmentVariables(launchSettings); // Extract the "environmentVariables" for the current launchSettings profile
            SetEnvironmentVariables(environmentVariables);  // Set the environment variables
            var environment = Environment.GetEnvironmentVariable("ENVIRONMENT");    // Get the value of the "ENVIRONMENT" environment variable
            return LoadAppSettingsConfiguration(environment);   // Load the appsettings configuration for the environment
        }

        private JObject LoadLaunchSettings()
        {
            using (var file = File.OpenText("Properties\\launchSettings.json")) // Open the "launchSettings.json" file for reading
            {
                var reader = new JsonTextReader(file);  // Create a JsonTextReader to read the JSON content from the file
                return JObject.Load(reader);    // Load the JSON content into a JObject
            }
        }

        private JObject GetEnvironmentVariables(JObject launchSettings)
        {
            var profileName = launchSettings.GetValue("profiles").First?.Path;  // Get the profile name from the JSON

            if (profileName != null)
            {
                string path = $"profiles.{profileName}.environmentVariables";// create the path to the profile's environment variables
                return launchSettings.SelectToken(path) as JObject; // extract the environment variables
            }
            return null;    // Return null or handle the case where no profile is found
        }


        private void SetEnvironmentVariables(JObject environmentVariables)
        {
            if (environmentVariables != null)
            {
                //Set environment variables for all properties of the environmentVariables JSON.
                foreach (var property in environmentVariables.Properties())
                {
                    Environment.SetEnvironmentVariable(property.Name, property.Value.ToString());
                }
            }
        }

        private IConfiguration LoadAppSettingsConfiguration(string environment)
        {
            // Build a configuration for the environment defined in the JSON
            var fileName = $"appsettings.{environment}.json";
            return new ConfigurationBuilder().AddJsonFile(fileName).Build();
        }
    }
}
