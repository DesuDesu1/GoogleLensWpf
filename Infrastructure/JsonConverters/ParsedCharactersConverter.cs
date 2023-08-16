using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Infrastructure.JsonConverters
{
    public sealed class ParsedCharactersConverter : JsonConverter<IEnumerable<TextRow>>
    {
        public override IEnumerable<TextRow>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            List<TextRow> rows = new();
            List<BoundingBoxes> data = new();
            List<double> values = new(6);
            string? characters;
            while (reader.Read())
            {
                if (data.Count > 0 && reader.TokenType == JsonTokenType.EndArray)
                {
                    if (reader.Read() && FindListOfDouble(ref reader, values))
                    {
                        rows.Add(new TextRow(data, values));
                        data.Clear();
                        continue;
                    }
                }
                // Getting the RowBoundingBox array for TextRow struct
                if (reader.TokenType != JsonTokenType.String)
                {
                    continue;
                }
                characters = reader.GetString();
                if (characters is null)
                {
                    continue;
                }
                if (!reader.Read() || !FindListOfDouble(ref reader, values))
                {
                    continue;
                }
                if (!reader.Read()
                    || reader.TokenType != JsonTokenType.StartArray)
                {
                    continue;
                }

                if (!reader.Read()
                    || reader.TokenType != JsonTokenType.EndArray)
                {
                    continue;
                }
                if (!reader.Read()
                    || reader.TokenType != JsonTokenType.String)
                {
                    if (reader.TokenType != JsonTokenType.Null)
                    {
                        continue;
                    }
                }
                characters += reader.GetString();

                if (!reader.Read()
                    || reader.TokenType != JsonTokenType.Number)
                {
                    continue;
                }
                if (!reader.Read()
                   || reader.TokenType != JsonTokenType.EndArray)
                {
                    continue;
                }
                data.Add(new(characters, values.ToArray()));
            }
            return rows;
        }

        public override void Write(Utf8JsonWriter writer, IEnumerable<TextRow> value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        private bool FindListOfDouble(ref Utf8JsonReader reader, List<double> values)
        {
            double d;
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                return false;
            }

            values.Clear();

            while (reader.Read() && reader.TokenType == JsonTokenType.Number && reader.TryGetDouble(out d))
            {
                values.Add(d);
            }

            if (values.Count == 0)
            {
                return false;
            }

            if (reader.TokenType != JsonTokenType.EndArray)
            {
                return false;
            }

            return true;
        }
    }
}
