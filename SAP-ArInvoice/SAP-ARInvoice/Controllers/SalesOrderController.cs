using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SAP_ARInvoice.Connection;
using SAP_ARInvoice.Model;
using SAP_ARInvoice.Model.DTO;
using SAP_ARInvoice.Model.Enum;
using SAP_ARInvoice.Model.Setting;
using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SAP_ARInvoice.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SalesOrderController
    {
        private readonly ILogger _logger;
        private readonly SAP_Connection connection;
        private Setting _setting;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SalesOrderController(IOptions<Setting> setting, ILogger<HomeController> logger, IHttpContextAccessor httpContextAccessor)
        {
            this.connection = new SAP_Connection(setting.Value);
            _logger = logger;
            _setting = setting.Value;
            _httpContextAccessor = httpContextAccessor;

        }

        [HttpGet]
        [Route("getorderbyapi")]
        public async Task<string> GetAsync()
        {
            var loginResoponse = AuthUserAsync();

            var parameters = new Dictionary<string, string>();
            parameters.Add("Keys", "Values");
            var baseURI = "https://sap.masoodapp.com/api/order/GetSyncOrder";

            if (loginResoponse == null)
            {
                _logger.LogError("user unable to login!");
                return "SAP B1 Background service";
            }

            else
            {
                if (connection.Connect() == 0)
                {
                    Documents oSO = null;
                    parameters.Add("@Date", DateTime.Now.ToString("yyyy/MM/dd"));
                    var invoices = await connection.ArInvoice_API<SalesOrder>(RequestEnum.POST, baseURI, parameters, loginResoponse.Result.access_token);

                    foreach (var singleInvoice in invoices)
                    {
                        var userResponse = await CheckOrderExist(singleInvoice.OrderCode);
                        if (!userResponse)
                        {
                            _logger.LogError("Sale Order already exists");
                            return "SAP B1 Background service";
                        }

                        oSO = connection.GetCompany().GetBusinessObject(BoObjectTypes.oOrders);

                        oSO.DocNum = Convert.ToInt32(singleInvoice.OrderCode);
                        oSO.DocDate = DateTime.Now;

                        foreach (var OrderItem in singleInvoice.OrderItems)
                        {
                            oSO.Lines.ItemCode = OrderItem.ItemCode.ToString();
                            oSO.Lines.Quantity = Convert.ToDouble(OrderItem.Quantity);
                            oSO.Lines.UnitPrice = Convert.ToDouble(OrderItem.UnitPrice);

                            if (oSO.Add() == 0)
                            {
                                Console.WriteLine("Success:Record added successfully");
                            }
                            connection.GetCompany().Disconnect();
                        }
                    }
                }

                else
                {
                    Console.WriteLine("Error " + connection.GetErrorCode() + ": " + connection.GetErrorMessage());
                }
            }

            return "";
        }

        [HttpGet]
        [Route("getorderbysp")]
        public async Task<string> GetAsyncDb()
        {
            var parameters = new Dictionary<string, string>();
            if (connection.Connect() == 0)
            {
                Documents oSO = null;
                parameters.Add("@Date", "12/28/2022");
                //var saleOrder = await connection.ArInvoice_SP<DataModel>("SP_SalesOrder", parameters);
                List<SalesOrder> salesOrders = await InvoiceMapper(await connection.ArInvoice_SP<DataModel>("SP_SalesOrder", parameters));

                foreach (var singleInvoice in salesOrders)
                {
                    //var CustomerResponse = await CheckBussinessCustomer(singleInvoice.CustomerName);
                    //if (!CustomerResponse)
                    //{
                    //    _logger.LogError("Unable to Create New User");
                    //    return "SAP B1 Background service";
                    //}
                    var userResponse = await CheckOrderExist(singleInvoice.OrderCode);
                    if (userResponse==true)
                    {
                        _logger.LogError("Sale Order already exists");
                        return "SAP B1 Background service";
                    }

                    oSO = connection.GetCompany().GetBusinessObject(BoObjectTypes.oOrders);

                    oSO.NumAtCard = singleInvoice.OrderCode;
                    oSO.DocDate = DateTime.Now;
                    oSO.CardCode = singleInvoice.CustomerName;
                    oSO.CardName = "Sajjad Khan";
                    oSO.Address = "";
                    
                    oSO.DocDueDate =DateTime.Parse(singleInvoice.DocDueDate);

                    foreach (var OrderItem in singleInvoice.OrderItems)
                    {
                        oSO.Lines.ItemCode = OrderItem.ItemCode;
                        oSO.Lines.Quantity = Convert.ToDouble(OrderItem.Quantity);
                        oSO.Lines.UnitPrice = Convert.ToDouble(OrderItem.UnitPrice);
                        oSO.Lines.Add();

                     
                    }
                    if (oSO.Add() == 0)
                    {
                        Console.WriteLine("Success:Record added successfully");
                    }
                    else
                    {
                        var errCode = connection.GetCompany().GetLastErrorCode();
                        var response = connection.GetCompany().GetLastErrorDescription();
                    }
                }
                connection.GetCompany().Disconnect();
            }

            else
            {
                Console.WriteLine("Error " + connection.GetErrorCode() + ": " + connection.GetErrorMessage());
            }
            return "";
        }
        private async Task<bool> CheckOrderExist(string OrderCode)
        {
            bool output = false;
            Recordset recordSet = connection.GetCompany().GetBusinessObject(BoObjectTypes.BoRecordset);
            Documents oSO =  connection.GetCompany().GetBusinessObject(BoObjectTypes.oOrders);
            recordSet.DoQuery($"SELECT * FROM \"ORDR\" WHERE \"NumAtCard\"='{OrderCode}'");
            if (recordSet.RecordCount == 0)
            {
                output = false;
            }
            else
            {
                output = true;
            }

            return output;
        }
        private async Task<bool> CheckBussinessCustomer(string CustomerId)
        {
            bool output = false;
            SAPbobsCOM.Recordset recordSet = null;
            BusinessPartners businessPartners = null;
            recordSet = connection.GetCompany().GetBusinessObject(BoObjectTypes.BoRecordset);
            businessPartners = connection.GetCompany().GetBusinessObject(BoObjectTypes.oBusinessPartners);

            recordSet.DoQuery($"SELECT * FROM \"OCRD\" WHERE \"CardCode\"='{CustomerId}'");
            if (recordSet.RecordCount == 0)
            {
                IDictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("@CardCode", CustomerId);

                List<Customer> customer = await connection.ArInvoice_SP<Customer>("[dbo].[GetCustomer]", parameters);
                foreach (var item in customer)
                {
                    businessPartners.CardCode = item.CardCode;
                    businessPartners.CardName = item.CustName;
                    businessPartners.Phone1 = item.Phone;
                    businessPartners.CardType = BoCardTypes.cCustomer;
                    businessPartners.SubjectToWithholdingTax = (BoYesNoNoneEnum)BoYesNoEnum.tNO;
                    var response = businessPartners.Add();
                    if (response.Equals(0))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                output = true;
            }

            return output;
        }

        public async Task<AuthToken> AuthUserAsync()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://sap.masoodapp.com/api/auth/login"))
                    {
                        request.Content = new StringContent("{\n    \"email\": \"sap@masood.com.pk\",\n    \"password\": \"sap@saleapp\"\n}");
                        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                        var response = await httpClient.SendAsync(request);
                        var token = await response.Content.ReadAsStringAsync();
                        AuthToken authToken = JsonConvert.DeserializeObject<AuthToken>(token);
                        if (authToken.status_code == 422)
                        {
                            _logger.LogError("Invalid username or password");
                            return null;
                        }
                        else
                        {
                            return authToken;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<List<SalesOrder>> InvoiceMapper(List<DataModel> data)
        {

            List<SalesOrder> SaleOrders = new List<SalesOrder>();
            List<DataModel> resp = data.Select(x => new { x.CustomerName, x.EmployeeName,x.OrderCode,x.DocDueDate,x.UnitPrice }).Distinct()
                .Select(x => data.FirstOrDefault(r => r.CustomerName == x.CustomerName && r.OrderCode == x.OrderCode && r.DocDueDate == x.DocDueDate && r.UnitPrice== x.UnitPrice)).Distinct().ToList();
            foreach (var item in resp)
            {
                var orderItems = data.Where(x => x.OrderCode == item.OrderCode && x.CustomerName == item.CustomerName && x.DocDueDate == item.DocDueDate && x.UnitPrice==item.UnitPrice)
                    .Select(x => new OrderItem { ItemCode = x.ItemCode, Quantity = x.Quantity,UnitPrice=x.UnitPrice }).Distinct().ToList();
                SaleOrders.Add(new SalesOrder() { CustomerName = item.CustomerName, OrderCode = item.OrderCode, CreatedDate = item.CreatedDate,DocDueDate=item.DocDueDate, OrderItems = orderItems });
            }

            return SaleOrders;
        }

        public async Task ShopifyAuth()
        {
    

        }

    }
}

