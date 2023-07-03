CREATE VIEW [dbo].[vReceipts]
AS
SELECT        dbo.Receipts.id, dbo.Receipts.receiptno, dbo.Receipts.applicationid, dbo.Receipts.invoiceid, dbo.Receipts.companyname, dbo.Receipts.amount, dbo.Receipts.status, dbo.Receipts.applicationreference, 
                         dbo.Receipts.RRR, dbo.Receipts.date_paid, dbo.invoices.payment_type, dbo.invoices.date_added AS Invoice_open_date, dbo.invoices.paymentTransaction_Id, dbo.invoices.payment_code
FROM            dbo.Receipts INNER JOIN
                         dbo.invoices ON dbo.Receipts.invoiceid = dbo.invoices.id