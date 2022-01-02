using System;
using Microsoft.Extensions.Configuration;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using Microsoft.Identity.Client; // MSAL
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;

namespace console_application
{
    class Program
    {
        static async Task Main(string[] args)
        {

            var builder = new ConfigurationBuilder().AddJsonFile($"appsettings.json", false, true);
            var config = builder.Build();

            var proxyUrl = config["proxyUrl"];

            Console.WriteLine("Testing HTTP Client connectivity with proxy using RestSharp...");
            var client = new RestClient("https://login.microsoftonline.com/1aea6c8a-20a4-4995-a64b-f460c93fcab9/v2.0");

            // https://docs.microsoft.com/en-us/dotnet/api/system.net.webproxy?view=net-6.0
            var proxy = new WebProxy(proxyUrl);
            client.Proxy = proxy;


            var request = new RestRequest(".well-known/openid-configuration", DataFormat.Json);

            IRestResponse response = client.Get(request);
            if (response.ErrorException != null)
            {
                Console.WriteLine(response.ErrorException.Message);
            } else {
                Console.WriteLine(response.Content);
                Console.WriteLine("proxy connection succeeded. See the above resposne from Azure AD.");
            }

            Console.WriteLine("Testing with MSAL...");


            ConfidentialClientApplicationOptions options = new ConfidentialClientApplicationOptions();
            options.ClientId ="f044c4fc-d1b7-42b4-8e81-35b4f6d3bef7";
            options.TenantId = "1aea6c8a-20a4-4995-a64b-f460c93fcab9";
            options.ClientSecret = "fFJ7Q~fCNMRGnowI2UybhLbAvm2cWbEi3~Wa6";
            options.LogLevel = LogLevel.Verbose;

            var staticClientWithProxyFactory = new StaticClientWithProxyFactory();
    
            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder
            .CreateWithApplicationOptions(options)
            .WithHttpClientFactory(staticClientWithProxyFactory)
            .WithLogging(MyLoggingMethod, LogLevel.Info,
                       enablePiiLogging: true, 
                       enableDefaultPlatformLogging: true)
            .Build();

            var scopes = new List<String>();
            scopes.Add("https://graph.microsoft.com/.default");

            // There are no public methods in MSAL that I could find that query just the service discovery endpoint.
            try {
                var authenticationResult = await app.AcquireTokenForClient(scopes).ExecuteAsync();
                Console.WriteLine(authenticationResult.AccessToken);
                Console.WriteLine("Test succedded with MSAL. See above access token.");
            } catch(Exception e) {
                Console.WriteLine(e.Message);
            }

            // app.GetAuthorizationRequestUrl(scopes).
        }

        static void MyLoggingMethod(LogLevel level, string message, bool containsPii)
        {
            Console.WriteLine($"MSAL {level} {containsPii} {message}");
        }
    }

        class StaticClientWithProxyFactory : IMsalHttpClientFactory
    {
        private static readonly HttpClient s_httpClient;

        static StaticClientWithProxyFactory()
        {
            var builder = new ConfigurationBuilder().AddJsonFile($"appsettings.json", false, true);
            var config = builder.Build();
            var proxyUrl = config["proxyUrl"];

            var webProxy = new WebProxy(
                new Uri(proxyUrl),
                BypassOnLocal: false);

            // webProxy.Credentials = new NetworkCredential("user", "pass");

            var proxyHttpClientHandler = new HttpClientHandler
            {
                Proxy = webProxy,
                UseProxy = true,
            };

            s_httpClient = new HttpClient(proxyHttpClientHandler);
           
        }

        public HttpClient GetHttpClient()
        {
            return s_httpClient;
        }
    }
}
