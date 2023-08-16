using GoogleLensWpf.Interfaces;
using GoogleLensWpf.JsonConverters;
using GoogleLensWpf.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GoogleLensWpf.Services
{
    public sealed class OCRProcessingService : IOCRProcessingService
    {
        private readonly IOCRService ocrservice;
        public event EventHandler<OCRResult> NewOCRResult;

        public OCRProcessingService(IOCRService ocrservice)
        {
            this.ocrservice = ocrservice;
        }
        public async Task PerformOcr(Image image)
        {
            var jsonstring = await ocrservice.GetJsonString(image.Data);
            var jsonBytes = Encoding.UTF8.GetBytes(jsonstring);
            using (var jsonStream = new MemoryStream(jsonBytes))
            {
                var _jsonSerializerOptions = new JsonSerializerOptions();
                _jsonSerializerOptions.Converters.Add(new ParsedCharactersConverter());
                var result = await JsonSerializer.DeserializeAsync<IEnumerable<TextRow>>(jsonStream, _jsonSerializerOptions);

                // Merge bounding boxes into text
                string ocrResult = MergeBoundingBoxes(result);
                var OCREventArgs = new OCRResult(image, result, ocrResult);
                // Raise NewOCRResult event
                OnNewOCRResult(OCREventArgs);
            }
        }
        private void OnNewOCRResult(OCRResult ocrResult)
        {
            NewOCRResult?.Invoke(this, ocrResult);
        }
        private string MergeBoundingBoxes(IEnumerable<TextRow> rows)
        {
            StringBuilder mergedText = new StringBuilder();
            foreach (var row in rows)
            {
                foreach (var symbol in row.Symbols)
                {
                    mergedText.Append(string.Join("", symbol.characters));
                }
                mergedText.AppendLine();
            }
            return mergedText.ToString();
        }
    }
}
