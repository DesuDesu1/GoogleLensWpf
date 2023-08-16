using GoogleLensWpf.Interfaces;
using GoogleLensWpf.JsonConverters;
using GoogleLensWpf.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace GoogleLensWpf.Services
{
    public sealed class GoogleLensOCRService : IOCRService
    {
        private Stopwatch stopwatch = new Stopwatch();
        private readonly HttpClient _httpClient;
        public GoogleLensOCRService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetJsonString(byte[] image)
        {
            var uploadControlUrl = await GetUploadId();
            var linkPartResponse = await UploadAndFinalizeImage(uploadControlUrl, image);
            var linkPart = GetLinkPart(linkPartResponse);
            return await GetJsonString(linkPart);
        }

        private async Task<string> GetUploadId()
        {
            var startRequest = new HttpRequestMessage(HttpMethod.Post, "https://lens.google.com/_/upload/");
            startRequest.Headers.TryAddWithoutValidation("x-client-side-image-upload", "true");
            startRequest.Headers.TryAddWithoutValidation("x-goog-upload-command", "start");
            startRequest.Headers.TryAddWithoutValidation("x-goog-upload-protocol", "resumable");
            var startResponse = await _httpClient.SendAsync(startRequest);
            startResponse.EnsureSuccessStatusCode();
            var uploadControlUrl = startResponse.Headers.GetValues("X-Goog-Upload-Control-URL").FirstOrDefault();
            return uploadControlUrl;
        }

        private async Task<string> UploadAndFinalizeImage(string uploadControlUrl, byte[] image)
        {
            var uploadRequest = new HttpRequestMessage(HttpMethod.Post, uploadControlUrl);
            uploadRequest.Headers.TryAddWithoutValidation("x-client-side-image-upload", "true");
            uploadRequest.Headers.TryAddWithoutValidation("x-goog-upload-offset", "0");
            uploadRequest.Headers.TryAddWithoutValidation("x-goog-upload-command", "upload, finalize");
            uploadRequest.Content = new ByteArrayContent(image);
            uploadRequest.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded;charset=utf-8");

            var uploadResponse = await _httpClient.SendAsync(uploadRequest);
            uploadResponse.EnsureSuccessStatusCode();

            var linkPartResponse = await uploadResponse.Content.ReadAsStringAsync();
            return linkPartResponse;
        }

        private string GetLinkPart(string linkPartResponse)
        {
            var unescapedLinkPartResponse = System.Text.RegularExpressions.Regex.Unescape(linkPartResponse);
            var linkPart = unescapedLinkPartResponse.Split("\"")[3];
            linkPart = linkPart.Substring(linkPart.LastIndexOf('=') + 1);

            return linkPart;
        }

        private async Task<string> GetJsonString(string linkPart)
        {
            var link = "https://lens.google.com/search?p=" + linkPart;
            var searchRequest = new HttpRequestMessage(HttpMethod.Get, link);
            var searchResponse = await _httpClient.SendAsync(searchRequest);
            searchResponse.EnsureSuccessStatusCode();

            // Read the search result as a stream
            using (var searchResultStream = await searchResponse.Content.ReadAsStreamAsync())
            using (var reader = new StreamReader(searchResultStream))
            {
                // Process the search result stream and save strings starting with "data:"
                return await ProcessSearchResultAsync(reader);
            }
        }
        private async Task<string> ProcessSearchResultAsync(StreamReader reader)
        {
            string processedResult = "";
            string line;
            string data = "";
            int i = 0;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (line.IndexOf("data:[") != -1)
                {
                    data = line;
                    int startIndex = data.LastIndexOf("data:[") + 5;
                    int endIndex = data.LastIndexOf("],");

                    if (startIndex != -1 && endIndex != -1)
                    {
                        processedResult = data.Substring(startIndex, endIndex - startIndex + 1);
                        break;
                    }
                }
                else
                {
                    continue;
                }
            }
            return processedResult;
        }
    }

}
