namespace FustOnline.Models
{
    internal class TransactionModel
    {
        public int Id { get; set; }
        public string? CustomerLocation { get; set; }
        public string? CustomerNumber { get; set; }
        public string? SupplierNumber { get; set; }
        public string? Location { get; set; }
        public int Status { get; set; }
        public int TransactionType { get; set; }
        public FustModel[]? Fusts { get; set; }
    }
}
