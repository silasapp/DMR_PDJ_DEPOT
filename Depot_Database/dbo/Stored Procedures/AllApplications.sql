
CREATE PROCEDURE [dbo].[AllApplications]

AS
	BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

   select 
   --App Details
   a.id as appId,a.reference as Reference, a.status as Status, a.fee_payable as Fee_Payable, a.service_charge as Service_Charge, a.CreatedAt as DateSubmitted, a.date_added as Date_Added , a.CreatedAt,
   a.UpdatedAt, a.Current_Permit, a.type as Type, a.year as Year, a.TransferCost, a.Migrated, a.submitted, a.AllowPush,a.AppProcessed, a.SupervisorProcessed, a.DeleteStatus, a.DeletedBy, a.PaymentDescription,
   --Company Details
    c.CompanyEmail, c.name as CompanyName, c.id as CompanyId,
	--Category and Phase Details
   ca.id as CategoryId, ca.name as CategoryName, p.id as PhaseId, p.name as PhaseName,  p.ShortName, p.issueType as ApprovalType, p.FlowType,
     --Facility Details
   f.Id as FacilityId,  f.Name as FacilityName, f.ContactName as Contact, f.ContactNumber,
   -- Facility Address Details
  ad.id as AddressId, ad.address_1 as FacilityAddress, ad.city as FacilityCity, st.Name as StateName, f.Lga as FacilityLGA, f.IdentificationCode as FacIdentificationCode, f.CategoryCode,
   --Office and Zonal Office Details
  sof.FieldOffice_id as OfficeId, fo.OfficeName, z.ZoneName, z.Zone_id as ZoneId,
   --Remita Payment Details
  t.RRR, t.approved_amount, t.transaction_date, t.order_id, 
   --Current Desk
 desk.ProcessID, desk.StaffID as currentDeskID
   --Tank Details
   --,COUNT(tnk.Id) as TanksCount, COUNT(pd.Id) as ProductsCount, SUM(tnk.MaxCapacity) as TanksCount

       
   from applications as a 
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
   
   left join Tanks as tnk on f.Id = tnk.FacilityId
   left join Products as pd on tnk.ProductId = pd.Id
   Left  join Lgas as lg on ad.LgaId = lg.Id
   Left join remita_transactions as t on a.reference = t.order_id
   left join MyDesk as desk on a.id = desk.AppId and desk.HasWork!= 1
  --where (a.isLegacy=0 and a.company_id > 0 and a.DeleteStatus = 0)

END