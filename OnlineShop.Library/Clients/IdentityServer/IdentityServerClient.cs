﻿using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OnlineShop.Library.Common.Models;
using OnlineShop.Library.IdentityServer;
using OnlineShop.Library.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace OnlineShop.Library.Clients.IdentityServer;

public class IdentityServerClient : IIdentityServerClient
{
    public IdentityServerClient(HttpClient client, IOptions<ServiceAddressOptions> options)
    {
        HttpClient = client;
        HttpClient.BaseAddress = new Uri(options.Value.IdentityServer);
    }
    
    public HttpClient HttpClient { get; init; }
    
    public async Task<Token> GetApiToken(IdentityServerApiOptions options)
    {
        var keyValues = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("scope", options.Scope),
            new KeyValuePair<string, string>("client_secret", options.ClientSecret),
            new KeyValuePair<string, string>("grant_type", options.GrantType),
            new KeyValuePair<string, string>("client_id", options.ClientId)
        };

        var content = new FormUrlEncodedContent(keyValues);
        var response = await HttpClient.PostAsync("/connect/token", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        var token = JsonConvert.DeserializeObject<Token>(responseContent);
        return token;
    }
    
    public async Task<Token> GetApiToken(IdentityServerUserNamePassword options)
    {
        var keyValues = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("scope", options.Scope),
            new KeyValuePair<string, string>("username", options.UserName),
            new KeyValuePair<string, string>("password", options.Password),
            new KeyValuePair<string, string>("grant_type", options.GrantType),
            new KeyValuePair<string, string>("client_id", options.ClientId)
        };

        var content = new FormUrlEncodedContent(keyValues);
        var response = await HttpClient.PostAsync("/connect/token", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        var token = JsonConvert.DeserializeObject<Token>(responseContent);
        return token;
    }
}