using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAP_ARInvoice.Model.DTO
{
    public class AuthToken
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int status_code { get; set; }
    }
}
