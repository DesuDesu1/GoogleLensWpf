using Application.Exceptions;
using Application.Interfaces;
using System.Net.Http.Headers;

namespace Infrastructure
{
    public sealed class GoogleLensUploader 
    {
        private readonly HttpClient _httpClient;

        public GoogleLensUploader(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> UploadImage(byte[] image)
        {
            var uploadControlUrl = await GetUploadId();
            var linkPartResponse = await UploadAndFinalizeImage(uploadControlUrl, image);
            var linkPart = GetLinkPart(linkPartResponse);
            var searchResult = await GetJsonString(linkPart);

            return searchResult;
        }

        private async Task<string> GetUploadId()
        {
            try
            {
                var startRequest = new HttpRequestMessage(HttpMethod.Post, "https://lens.google.com/_/upload/");
                startRequest.Headers.TryAddWithoutValidation("x-client-side-image-upload", "true");
                startRequest.Headers.TryAddWithoutValidation("x-goog-upload-command", "start");
                startRequest.Headers.TryAddWithoutValidation("x-goog-upload-protocol", "resumable");

                var startResponse = await _httpClient.SendAsync(startRequest);
                startResponse.EnsureSuccessStatusCode();

                var uploadControlUrl = startResponse.Headers.GetValues("X-Goog-Upload-Control-URL").FirstOrDefault();
                if (string.IsNullOrEmpty(uploadControlUrl))
                {
                    throw new Exception("Upload control URL not found.");
                }

                return uploadControlUrl;
            }
            catch (HttpRequestException ex)
            {
                // Handle no internet connection error
                throw new NoInternetConnectionException("No internet connection.", ex);
            }
            catch (Exception ex)
            {
                // Log or handle other exceptions as needed
                throw;
            }
        }

        private async Task<string> UploadAndFinalizeImage(string uploadControlUrl, byte[] image)
        {
            try
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
            catch (HttpRequestException ex)
            {
                // Handle no internet connection error
                throw new NoInternetConnectionException("No internet connection.", ex);
            }
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
                var processedResult = await ProcessSearchResultAsync(reader);

                return processedResult;
            }
        }
        private async Task<string> ProcessSearchResultAsync(StreamReader reader)
        {
            string processedResult = "";
            string line;
            string data = "";

            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (line.Length > 100 && line.IndexOf("data:[") != -1)
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