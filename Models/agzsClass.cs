using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsSetPrice.Models
{
    public class agzsClass
    {
        public int id { get; set; }
        public string externalCode { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public decimal latitude { get; set; }
        public decimal longitude { get; set; }
        public string services { get; set; }
        public decimal price { get; set; }
        public string workSchedule { get; set; }
        public string phoneNumber { get; set; }
        public bool hasSto { get; set; }
        public string agzsid { get; set; }
    }
}
