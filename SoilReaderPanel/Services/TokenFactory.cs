﻿using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace SoilReaderPanel.Services
{
    public class TokenFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ParticleAuthCreds _auth;
        private readonly Uri authUri = new Uri("https://api.particle.io/oauth/token/");
        private readonly Uri deviceUri = new Uri("https://api.particle.io/v1/devices");
        private string accessToken;
        private string refreshToken;
        private DateTime tokenExpiration;
        public IConfiguration Configuration { get; }

        public TokenFactory(IHttpClientFactory httpClientFactory, IConfiguration config)
        { 
            _httpClientFactory = httpClientFactory;
            _auth = new ParticleAuthCreds(
                username: config["particle:username"],
                password: config["particle:password"],
                client_id: config["particle:client_id"],
                client_secret: config["particle:client_secret"]
                );

        }

        //maybe implement later, not sure of the necessity of a refresh token for this app
        //private void RefreshToken()
        //{

        //}
        public async Task<bool> ValidateID(string deviceId)
        {
            if (accessToken == null)
            {
                await GenerateToken();
            }

            int ttl = (tokenExpiration - DateTime.Now).Minutes;

            if (ttl < 2)
            {
                await GenerateToken();
            }
            var client = _httpClientFactory.CreateClient();
            string uri = $"https://api.particle.io/v1/devices/{deviceId}?access_token={accessToken}";

            var cts = new CancellationTokenSource();
            cts.CancelAfter(2000);

            HttpResponseMessage result = await client.GetAsync(uri, cts.Token);
            try
            {
                if (!result.IsSuccessStatusCode) return false;
                DeviceJSON jsonDict = JsonConvert.DeserializeObject<DeviceJSON>(result.Content.ReadAsStringAsync().Result);
                return true;
            }
            catch
            {
                return false;
            }
            
          
        }
        public async Task<string> GetData(string deviceId, string field)
        {
            if (accessToken == null)
            {
                await GenerateToken();
            }

            int ttl = (tokenExpiration - DateTime.Now).Minutes;

            if (ttl < 2) {
                await GenerateToken();
            }

            var client = _httpClientFactory.CreateClient();
            string postString = "";
            string uri = $"https://api.particle.io/v1/devices/{deviceId}/{field}?access_token={accessToken}";
            Dictionary<string, string> jsonDict = (await Post(postString, uri));
            return jsonDict["return_value"];

        }

        private async Task<bool> GenerateToken()
        {
            var client = _httpClientFactory.CreateClient();
            string postString = $"grant_type=password&username={_auth.username}&password={_auth.password}&client_id={_auth.client_id}&client_secret={_auth.client_secret}";
            string uri = "https://api.particle.io/oauth/token";
            Dictionary<string, string> jsonDict = (await Post(postString, uri));
            accessToken = jsonDict["access_token"];
            refreshToken = jsonDict["refresh_token"];
            string tte = jsonDict["expires_in"];
            tokenExpiration = DateTime.Now.AddMilliseconds(double.Parse(tte));

            return true;

        }

        private async Task<Dictionary<string, string>> Post(string postString, string uri)
        {
            var client = _httpClientFactory.CreateClient();
            StringContent body = new StringContent(postString,
            Encoding.UTF8,
            "application/x-www-form-urlencoded");

            var cts = new CancellationTokenSource();
            cts.CancelAfter(1000);
            try
            {
                
                HttpResponseMessage result = await client.PostAsync(uri, body, cts.Token);
                Dictionary<string, string> jsonDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(result.Content.ReadAsStringAsync().Result);
                if (!result.IsSuccessStatusCode)
                {
                    jsonDict.Add("return_value", "fail");                
                }
                return jsonDict;
            }
            catch
            {
                Dictionary<string, string> jsonDict = new Dictionary<string, string>{ { "return_value", "fail" } };
                return jsonDict;
            }
            
            
        }

        

    }
    class ParticleAuthCreds
    {
        public readonly string username;
        public readonly string password;
        public readonly string client_id;
        public readonly string client_secret;

        public ParticleAuthCreds(string username, string password, string client_id, string client_secret)
        {
            this.username = username;
            this.password = password;
            this.client_id = client_id;
            this.client_secret = client_secret;
        }
    }
}
