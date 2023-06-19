using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using Newtonsoft.Json;

namespace CK.DB.OIddict.Dapper
{
    internal static class JsonTypeConverter
    {
        static readonly JsonSerializerSettings _jsonSettings = new() { };

        public static string ToJson( HashSet<string> collection ) =>
        JsonConvert.SerializeObject( collection, _jsonSettings );

        public static HashSet<string> FromJson( string json ) =>
        FromJson<HashSet<string>>( json );

        public static T FromJson<T>( string json )
        {
            if( typeof( T ) == typeof( HashSet<string> ) )
            {
                var collection = (T)(object)JsonConvert.DeserializeObject<HashSet<string>>( json, _jsonSettings );
                return collection;
            }

            if( typeof( T ) == typeof( HashSet<Uri> ) )
            {
                var collection = (T)(object)JsonConvert.DeserializeObject<HashSet<Uri>>( json, _jsonSettings );
                return collection;
            }

            if( typeof( T ) == typeof( Dictionary<string, JsonElement> ) )
            {
                var collection = (T)(object)JsonConvert.DeserializeObject<Dictionary<string, JsonElement>>
                ( json, _jsonSettings );
                return collection;
            }

            if( typeof( T ) == typeof( Dictionary<CultureInfo, string> ) )
            {
                var collection = (T)(object)JsonConvert.DeserializeObject<Dictionary<CultureInfo, string>>
                ( json, _jsonSettings );
                return collection;
            }

            throw new NotSupportedException();
        }

        public static string ToJson( Dictionary<string, JsonElement> collection ) =>
        JsonConvert.SerializeObject( collection, _jsonSettings );

        public static Dictionary<string, JsonElement> FromJson2( string json ) =>
        FromJson<Dictionary<string, JsonElement>>( json );

        public static string ToJson( HashSet<Uri> collection ) =>
        JsonConvert.SerializeObject( collection, _jsonSettings );

        public static HashSet<Uri> FromJson3( string json ) =>
        FromJson<HashSet<Uri>>( json );

        public static string ToJson( Dictionary<CultureInfo, string> collection ) =>
        JsonConvert.SerializeObject( collection, _jsonSettings );

        public static Dictionary<CultureInfo, string> FromJson4( string json ) =>
        FromJson<Dictionary<CultureInfo, string>>( json );
    }
}
