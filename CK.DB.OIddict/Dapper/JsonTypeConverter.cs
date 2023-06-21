using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using Newtonsoft.Json;
using static Newtonsoft.Json.JsonConvert;

namespace CK.DB.OIddict.Dapper
{
    internal static class JsonTypeConverter
    {
        private static readonly JsonSerializerSettings _jsonSettings = new();

        public static T? FromJson<T>( string? json )
        {
            if( json is null ) return default;

            if( _deserializationMap.TryGetValue( typeof( T ), out var deserializer ) )
                return (T)deserializer.Invoke( json );

            throw new NotSupportedException();
        }

        private static readonly Dictionary<Type, Func<string, object>> _deserializationMap = new()
        {
            { typeof( HashSet<string> ), Deserialize<HashSet<string>> },
            { typeof( HashSet<Uri> ), Deserialize<HashSet<Uri>> },
            { typeof( Dictionary<string, JsonElement> ), Deserialize<Dictionary<string, JsonElement>> },
            { typeof( Dictionary<CultureInfo, string> ), Deserialize<Dictionary<CultureInfo, string>> },
        };

        private static T Deserialize<T>( string json ) => DeserializeObject<T>( json, _jsonSettings )!;

        public static string? ToJson( HashSet<string>? collection ) => SerializeToJson( collection );
        public static string? ToJson( Dictionary<string, JsonElement>? collection ) => SerializeToJson( collection );
        public static string? ToJson( HashSet<Uri>? collection ) => SerializeToJson( collection );
        public static string? ToJson( Dictionary<CultureInfo, string>? collection ) => SerializeToJson( collection );

        private static string? SerializeToJson( object? collection ) =>
        collection is null ? null : SerializeObject( collection, _jsonSettings );
    }
}
