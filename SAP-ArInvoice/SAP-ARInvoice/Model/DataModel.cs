using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAP_ARInvoice.Model
{
    public class DataModel
    {
        public string CustomerName { get; set; }
        public string EmployeeName { get; set; }
        public string OrderCode { get; set; }
        public string DocNum { get; set; }
        public string DocType { get; set; }
        public string CreatedDate { get; set; }
        public string ItemCode { get; set; }
        public string UnitPrice { get; set; }
        public string Quantity { get; set; }
        public string Discount { get; set; }
        public string DocDueDate { get; set; }
    }


}
