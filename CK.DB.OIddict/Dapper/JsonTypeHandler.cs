﻿using System;
using System.Data;
using Dapper;
using Newtonsoft.Json;

namespace CK.DB.OIddict.Dapper
{
    public class JsonTypeHandler<T> : SqlMapper.TypeHandler<T>
    {
        private readonly JsonSerializerSettings _jsonSettings = new() { };

        public override void SetValue( IDbDataParameter parameter, T value )
        {
            throw new Exception( "This code should not be reached." );
// parameter.Value = JsonConvert.SerializeObject( value, _jsonSettings );
        }

        public override T Parse( object value )
        {
            if( value is not string json ) throw new NotImplementedException();

            return JsonConvert.DeserializeObject<T>( json, _jsonSettings )!;
        }
    }
}
