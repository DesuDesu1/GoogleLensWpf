using Application.Interfaces;
using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.JsonConverters
{
    public class JsonDeserializerWrapper : ITextRowJsonDeserializer
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public JsonDeserializerWrapper()
        {
            _jsonSerializerOptions = new JsonSerializerOptions();
            _jsonSerializerOptions.Converters.Add(new ParsedCharactersConverter());
        }

        public IEnumerable<TextRow> Deserialize(string json)
        {
            return JsonSerializer.Deserialize<IEnumerable<TextRow>>(json, _jsonSerializerOptions);
        }
    }

}
