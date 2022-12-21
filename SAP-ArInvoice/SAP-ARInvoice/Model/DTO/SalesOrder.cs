using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAP_ARInvoice.Model.DTO
{
    public class SalesOrder
    {
        public int OrderCode { get; set; }
        public string DocNum { get; set; }
        public string DocType { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<OrderItem> OrderItems { get; set; }
    }

    public class OrderItem
    {
        public string ItemCode { get; set; }
        public int UnitPrice { get; set; }
        public int Quantity { get; set; }
    }
}
