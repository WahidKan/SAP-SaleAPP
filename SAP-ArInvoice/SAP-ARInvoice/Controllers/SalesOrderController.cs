using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SAP_ARInvoice.Connection;
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

        public SalesOrderController(IOptions<Setting> setting, ILogger<HomeController> logger)
        {
            this.connection = new SAP_Connection(setting.Value);
            _logger = logger;

            _setting = setting.Value;
        }

        [HttpGet]
        public async Task<string> GetAsync()
        {
            var parameters = new Dictionary<string, string>();
            parameters.Add("Keys", "Values");
            var baseURI = "http://";
            var Token = "";

            if (connection.Connect() == 0)
            {
                Documents oSO = null;
                parameters.Add("@Date", DateTime.Now.ToString("yyyy/MM/dd"));
                var invoices = await connection.ArInvoice_API<SalesOrder>(RequestEnum.POST, baseURI, parameters, Token);

                foreach (var singleInvoice in invoices)
                {
                    var userResponse = await CheckOrderExist(singleInvoice.OrderCode);
                    if (!userResponse)
                    {
                        _logger.LogError("Unable to Create New User");
                        return "SAP B1 Background service";
                    }

                    oSO = connection.GetCompany().GetBusinessObject(BoObjectTypes.oOrders);

                    oSO.DocNum = singleInvoice.OrderCode;
                    oSO.DocDate = DateTime.Now;

                    foreach (var OrderItem in singleInvoice.OrderItems)
                    {
                        oSO.Lines.ItemCode = OrderItem.ItemCode;
                        oSO.Lines.Quantity = OrderItem.Quantity;
                        oSO.Lines.UnitPrice = OrderItem.UnitPrice;

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
            return "";
        }
        private async Task<bool> CheckOrderExist(int OrderCode)
        {
            bool output = false;
            Recordset recordSet = connection.GetCompany().GetBusinessObject(BoObjectTypes.BoRecordset);
            Documents oSO = await connection.GetCompany().GetBusinessObject(BoObjectTypes.oOrders);
            recordSet.DoQuery($"SELECT * FROM \"ORDR\" WHERE \"DOCNUM\"='{OrderCode}'");
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
    }
}

