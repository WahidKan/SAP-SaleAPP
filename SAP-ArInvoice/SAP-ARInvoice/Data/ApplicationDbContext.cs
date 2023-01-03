using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using SAP_ARInvoice.Model.DTO;

namespace AR_InvoiceTest.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
       DbSet<SalesOrder> SalesOrders { get; set; }
    }
}
