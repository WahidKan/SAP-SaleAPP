CREATE PROCEDURE SP_SalesOrder @Date Date, @IsSync int
AS
SELECT SO.CustomerName,SO.EmployeeName,SO.OrderCode,SO.DocNum,SO.DocType,SO.CreatedDate,SO.IsSync,OT.OrderCode,OT.UnitPrice,OT.Quantity,OT.Discount
FROM salesOrders SO
INNER JOIN OrderItem OT ON SO.OrderCode = OT.OrderCode
WHERE SO.CreatedDate = @Date AND SO.IsSync=@IsSync
GO