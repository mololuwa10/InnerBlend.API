using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InnerBlend.API.Data
{
    public static class DateTimeExtension
    {
        public static DateTime? ToUtc(this DateTime? dateTime)
        {
            if (dateTime.HasValue && dateTime.Value.Kind == DateTimeKind.Utc) 
            {
                return dateTime;
            }
            
            return dateTime?.ToUniversalTime();
        }
    }
}