using System;
using System.Data;
using Dapper;

namespace CK.DB.OIddict.Dapper
{
    public class GuidToStringTypeHandler : SqlMapper.TypeHandler<Guid>
    {
        /// <inheritdoc />
        public override void SetValue( IDbDataParameter parameter, Guid value )
        {
            parameter.Value = value.ToString();
        }

        /// <inheritdoc />
        public override Guid Parse( object value )
        {
            if( value is Guid guidValue )
                return guidValue;

            if( value is not string stringValue )
                throw new ArgumentException( "string expected", nameof( value ) );
            return Guid.Parse( stringValue );
        }
    }
}
