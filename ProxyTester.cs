using System;
using Microsoft.Extensions.Configuration;
using RestSharp;
using RestSharp.Authenticators;

namespace console_application
{
    class ProxyTester
    {
        const string BaseUrl = "https://api.twilio.com/2008-08-01";

    readonly IRestClient _client;

    string _accountSid;

    public ProxyTester(string accountSid, string secretKey) 
    {
        _client = new RestClient(BaseUrl);
        _client.Authenticator = new HttpBasicAuthenticator(accountSid, secretKey);
        _accountSid = accountSid;
    }

    public T Execute<T>(RestRequest request) where T : new()
    {
        var response = _client.Execute<T>(request);

        if (response.ErrorException != null)
        {
            const string message = "Error retrieving response.  Check inner details for more info.";
            var twilioException = new Exception(message, response.ErrorException);
            throw twilioException;
        }
        return response.Data;
    }
    }
}
