﻿using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Refit;

namespace AzureBlobStorageSampleApp
{
    public static class RefitExtensions
	{
		public static T For<T>(string hostUrl) => RestService.For<T>(hostUrl, GetNewtonsoftJsonRefitSettings());
		public static T For<T>(HttpClient client) => RestService.For<T>(client, GetNewtonsoftJsonRefitSettings());

		public static RefitSettings GetNewtonsoftJsonRefitSettings() => new RefitSettings(new NewtonsoftJsonContentSerializer(new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }));
	}
}