namespace WEBDULICH.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public string PaymentMethod { get; set; }

        public string PaymentStatus { get; set; }

        public int Amount { get; set; }

        public DateTime PaymentDate { get; set; }  

        public int OrdersId { get; set; }
        public Orders Orders { get; set; }
    }

}
