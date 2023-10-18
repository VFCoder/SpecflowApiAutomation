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
            using (var file = File.OpenText("Properties\\launchSettings.json"))
            {
                var reader = new JsonTextReader(file);
                var jObject = JObject.Load(reader);

                var variables = jObject.GetValue("profiles")
               .SelectMany(profiles => profiles.Children())
               .SelectMany(profile => profile.Children<JProperty>())
               .Where(prop => prop.Name == "environmentVariables")
               .SelectMany(prop => prop.Value.Children<JProperty>())
               .ToList();

                foreach (var variable in variables)
                {
                    Environment.SetEnvironmentVariable(variable.Name, variable.Value.ToString());
                }
            }

            var environment = Environment.GetEnvironmentVariable("ENVIRONMENT");

            var fileName = $"appsettings.{environment}.json";

            return new ConfigurationBuilder().AddJsonFile(fileName).Build();

        }

    }
}
