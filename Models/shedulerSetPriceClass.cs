using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsSetPrice.Models
{
    internal class shedulerSetPriceClass
    {
        public string agzsid { get; set; }
        public decimal new_price { get; set; }
        public TimeOnly scheduled_time { get; set; }
    }
}
