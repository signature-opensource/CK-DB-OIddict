using System;

namespace CK.DB.OIddict.Dapper
{
    internal class DateTimeUtc
    {
        private readonly DateTime _dateTime;

        public DateTime DateTime
        {
            get => _dateTime;
            init => _dateTime = DateTime.SpecifyKind( value, DateTimeKind.Utc );
        }

        public DateTimeUtc() { }

        private DateTimeUtc( DateTime dateTime ) => DateTime = dateTime;

        public static implicit operator DateTimeUtc( DateTime dateTime ) => new( dateTime );
    }
}
