namespace SocietyApp.Models
{
    public class ReceiptLineItem
    {
        public int Id { get; set; }
        public int ReceiptId { get; set; }
        public int ParticularId { get; set; }
        public decimal Amount { get; set; }

        // Navigation
        public Receipt? Receipt { get; set; }
        public Particular? Particular { get; set; }
    }
}
