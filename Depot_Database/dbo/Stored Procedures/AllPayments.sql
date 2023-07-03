

CREATE PROCEDURE [dbo].[AllPayments]

AS
	BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

   select distinct
   --App Details
   a.id as ApplicationID,a.reference as ReferenceNo, a.status as Status, a.fee_payable as Fee, a.service_charge, a.CreatedAt as DateSubmitted, a.date_added as Date_Added , a.CreatedAt,
   a.UpdatedAt, a.Current_Permit, a.type as Type, a.year as Year, a.PaymentDescription as PaymentBreakdown, a.DeleteStatus,
   --Company Details
    c.CompanyEmail, c.name as CompanyName, c.id as CompanyId,
	--Category and Phase Details
   ca.id as CategoryId, ca.name as CategoryName, p.id as PhaseId, p.name as Category,  p.ShortName, p.issueType as ApprovalType, p.FlowType,
     --Facility Details
   f.Id as FacilityId,  f.Name as FacilityName, f.ContactName as Contact, f.ContactNumber,
   -- Facility Address Details
   ad.address_1 as FacilityAddress, ad.city as FacilityCity, st.Name as StateName, f.Lga as FacilityLGA, f.IdentificationCode as FacIdentificationCode, f.CategoryCode,
   --Office and Zonal Office Details
  sof.FieldOffice_id as OfficeId, fo.OfficeName as Office, z.ZoneName as ZonalOffice, z.Zone_id as ZoneId,
   --Remita Payment Details
  t.RRR, t.RRR as PaymentRef, t.approved_amount as Amount, t.id as ID, t.type as Channel, t.transaction_date as Date, t.order_id, inv.receipt_no as ReceiptNo, inv.payment_type as PaymentType, inv.status as PaymentStatus
   --Current Desk
   --Tank Details
   --,COUNT(tnk.Id) as TanksCount, COUNT(pd.Id) as ProductsCount, SUM(tnk.MaxCapacity) as TanksCount

       
   from applications as a 
   inner join invoices as inv on a.id = inv.application_id
   inner join remita_transactions as t on a.reference = t.order_id
   inner join Categories as ca on a.category_id = ca.id
   inner join Phases as p on a.PhaseId = p.id
   inner join Companies as c on a.company_id = c.id
   inner join Facilities as f on a.FacilityID = f.Id
   left join addresses as ad on f.AddressId = ad.id
   left join States as st on ad.StateId = st.Id
   left join FieldOfficeStates as sof on st.Id = sof.StateId
   left join FieldOffices as fo on sof.FieldOffice_id = fo.FieldOffice_id
   left join ZoneFieldOffice as zf on fo.FieldOffice_id = zf.FieldOffice_id
   left join ZonalOffice as z on zf.Zone_id = z.Zone_id
   
   Left  join Lgas as lg on ad.LgaId = lg.Id
   
 
  --where (a.isLegacy=0 and a.company_id > 0 and a.DeleteStatus = 0)

END