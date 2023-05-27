namespace FustOnline.Models
{
    public class TransactionModel
    {
        public int Id { get; set; }
        public Guid CorrelationId { get; set; }
        public string? CustomerLocation { get; set; }
        public string? CustomerNumber { get; set; }
        public string? SupplierNumber { get; set; }
        public string? Location { get; set; }
        public int Status { get; set; }
        public int TransactionType { get; set; }
        public Fust[]? Fusts { get; set; }
    }

    public class Fust
    {
        public int Code { get; set; }
        public int Stw { get; set; }
        public int Stpl { get; set; }
        public int Stk { get; set; }
    }
}