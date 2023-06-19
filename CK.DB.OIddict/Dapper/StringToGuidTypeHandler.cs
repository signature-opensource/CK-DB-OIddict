using System;
using System.Data;
using Dapper;

namespace CK.DB.OIddict.Dapper
{
    public class StringToGuidTypeHandler : SqlMapper.TypeHandler<string>
    {
        /// <inheritdoc />
        public override void SetValue( IDbDataParameter parameter, string value )
        {
            parameter.Value = value;
        }

        /// <inheritdoc />
        public override string Parse( object value )
        {
            if( value is Guid guidValue )
                return guidValue.ToString();

            if( value is not string stringValue )
                throw new ArgumentException( "string expected", nameof( value ) );
            return stringValue;
        }
    }
}
