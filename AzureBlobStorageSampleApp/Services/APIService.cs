﻿using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Collections.Generic;

using Newtonsoft.Json;

using Xamarin.Forms;

using AzureBlobStorageSampleApp.Shared;
using AzureBlobStorageSampleApp.Mobile.Shared;

namespace AzureBlobStorageSampleApp
{
    public static class APIService
    {
        #region Constant Fields
        static readonly TimeSpan _httpTimeout = TimeSpan.FromSeconds(60);
        static readonly JsonSerializer _serializer = new JsonSerializer();
        static readonly HttpClient _client = CreateHttpClient();
        #endregion

        #region Fields
        static int _networkIndicatorCount = 0;
        #endregion

        #region Methods
        public static async Task<List<PhotoModel>> GetAllPhotoModels() =>
        await GetDataObjectFromAPI<List<PhotoModel>>($"{BackendConstants.AzureAPIUrl}GetAllContacts");

        public static async Task<PhotoModel> GetPhotoModel(PhotoModel contact) =>
            await GetDataObjectFromAPI<PhotoModel, string>($"{BackendConstants.AzureAPIUrl}GetContact", contact.Id);

        public static async Task<HttpResponseMessage> PostPhotoModel(PhotoModel contact) =>
            await PostObjectToAPI($"{BackendConstants.AzureAPIUrl}PostContact", contact);

        public static async Task<HttpResponseMessage> PatchPhotoModel(PhotoModel contact) =>
            await PatchObjectToAPI($"{BackendConstants.AzureAPIUrl}PatchContact/{contact.Id}", contact);

        public static async Task<HttpResponseMessage> DeletePhotoModel(PhotoModel contact) =>
            await DeleteObjectFromAPI($"{BackendConstants.AzureAPIUrl}DeleteContact/{contact.Id}");

        public static async Task<HttpResponseMessage> RestoreDeletedContacts() =>
            await PostObjectToAPI($"{BackendConstants.AzureFunctionUrl}RestoreDeletedContacts/?code={BackendConstants.AzureFunctionKey_RestoreDeletedContacts}", new object());

        static async Task<T> GetDataObjectFromAPI<T>(string apiUrl) =>
            await GetDataObjectFromAPI<T, object>(apiUrl);

        static async Task<T> GetDataObjectFromAPI<T, U>(string apiUrl, U data = default(U))
        {
            var stringPayload = string.Empty;

            if (data != null)
                stringPayload = await Task.Run(() => JsonConvert.SerializeObject(data)).ConfigureAwait(false);

            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

            try
            {
                UpdateActivityIndicatorStatus(true);

                using (var stream = await _client.GetStreamAsync(apiUrl).ConfigureAwait(false))
                using (var reader = new StreamReader(stream))
                using (var json = new JsonTextReader(reader))
                {
                    if (json == null)
                        return default(T);

                    return await Task.Run(() => _serializer.Deserialize<T>(json)).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                MobileCenterHelpers.Log(e);
                return default(T);
            }
            finally
            {
                UpdateActivityIndicatorStatus(false);
            }
        }

        static async Task<HttpResponseMessage> PostObjectToAPI<T>(string apiUrl, T data)
        {
            var stringPayload = await Task.Run(() => JsonConvert.SerializeObject(data)).ConfigureAwait(false);

            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
            try
            {
                UpdateActivityIndicatorStatus(true);

                return await _client.PostAsync(apiUrl, httpContent).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                MobileCenterHelpers.Log(e);
                return null;
            }
            finally
            {
                UpdateActivityIndicatorStatus(false);
            }
        }

        static async Task<HttpResponseMessage> PatchObjectToAPI<T>(string apiUrl, T data)
        {
            var stringPayload = await Task.Run(() => JsonConvert.SerializeObject(data)).ConfigureAwait(false);

            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

            var httpRequest = new HttpRequestMessage
            {
                Method = new HttpMethod("PATCH"),
                RequestUri = new Uri(apiUrl),
                Content = httpContent
            };

            try
            {
                UpdateActivityIndicatorStatus(true);

                return await _client.SendAsync(httpRequest).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                MobileCenterHelpers.Log(e);
                return null;
            }
            finally
            {
                UpdateActivityIndicatorStatus(false);
            }
        }

        static async Task<HttpResponseMessage> DeleteObjectFromAPI(string apiUrl)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Delete, new Uri(apiUrl));

            try
            {
                UpdateActivityIndicatorStatus(true);

                return await _client.SendAsync(httpRequest).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                MobileCenterHelpers.Log(e);
                return null;
            }
            finally
            {
                UpdateActivityIndicatorStatus(false);
            }
        }

        static void UpdateActivityIndicatorStatus(bool isActivityIndicatorDisplayed)
        {
            var baseNavigationPage = Application.Current.MainPage as NavigationPage;
            var currentPage = baseNavigationPage.CurrentPage as ContentPage;
            var currentViewModel = currentPage.BindingContext as BaseViewModel;

            if (isActivityIndicatorDisplayed)
            {
                currentViewModel.IsInternetConnectionActive = true;
                _networkIndicatorCount++;
            }
            else
            {
                if (--_networkIndicatorCount == 0)
                    currentViewModel.IsInternetConnectionActive = false;
            }
        }

        static HttpClient CreateHttpClient()
        {
            var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip })
            {
                Timeout = _httpTimeout
            };

            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            return client;
        }
        #endregion
    }
}
