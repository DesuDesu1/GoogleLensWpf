using Application.Interfaces;
using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public sealed class GetTextRowFromImage : IGetTextRowFromImage
    {
        private readonly ITextRowJsonDeserializer _jsondeserializer;
        private readonly IGetJsonString _getJson;
        public GetTextRowFromImage(IGetJsonString getJson, ITextRowJsonDeserializer jsonDeserializer)
        {
            _jsondeserializer = jsonDeserializer;
            _getJson = getJson;
        }
        public async Task<IEnumerable<TextRow>> GetTextRowList(byte[] image)
        {
            string json = await _getJson.UploadImage(image);
            return _jsondeserializer.Deserialize(json);
        }
    }
}
