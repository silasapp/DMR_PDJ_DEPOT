namespace NewDepot.Models
{
    public class ZoneFieldMapping
    {
        public int Id { get; set; }
        public int ZoneFieldId { get; set; }

        public int FieldLocationId { get; set; }
        public bool Status { get; set; }
    }
}