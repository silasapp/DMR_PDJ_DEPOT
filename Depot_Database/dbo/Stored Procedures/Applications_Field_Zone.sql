


CREATE PROCEDURE [dbo].[Applications_Field_Zone]

AS
	BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

   select distinct
   --App Details
   a.id as id,
  f.Id as FacilityId, 
  st.Name as StateName,
   --Office and Zonal Office Details
  sof.FieldOffice_id as OfficeId, fo.OfficeName, z.ZoneName, z.Zone_id as ZoneId
   
       
   from applications as a 
   inner join Facilities as f on a.FacilityID = f.Id
   left join addresses as ad on f.AddressId = ad.id
   left join States as st on ad.StateId = st.Id
   left join FieldOfficeStates as sof on st.Id = sof.StateId
   left join FieldOffices as fo on sof.FieldOffice_id = fo.FieldOffice_id
   left join ZoneFieldOffice as zf on fo.FieldOffice_id = zf.FieldOffice_id
   left join ZonalOffice as z on zf.Zone_id = z.Zone_id
   
   
END