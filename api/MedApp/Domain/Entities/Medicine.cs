namespace MedApp.Domain.Entities
{
    public class Medicine
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Dosage { get; set; }
        public string? Manufacturer { get; set; }
    }
}
