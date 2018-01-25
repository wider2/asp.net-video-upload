using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VircaVideoCore.Models
{
    public class Virca_Orders_Video
    {
        [Key]
        public Int64 Id { get; set; }

        public string No_account { get; set; }

        public Int16 Video_id { get; set; }

        public string Filename { get; set; }

        public DateTime Date_pub { get; set; }

        public string Add_info { get; set; }

        public byte? Status { get; set; }

        public bool Suspicious { get; set; }

    }
}
