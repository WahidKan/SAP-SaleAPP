Alter PROCEDURE SP_SalesOrder @Date Date
AS
SELECT SO.Id , SO.CustomerName,SO.EmployeeName,SO.OrderCode,SO.DocNum,SO.DocType,SO.CreatedDate,OT.OrderCode,OT.ItemCode,OT.UnitPrice,OT.Quantity,OT.Discount
FROM salesOrders SO
INNER JOIN OrderItem OT ON SO.OrderCode = OT.OrderCode
WHERE SO.CreatedDate = @Date
