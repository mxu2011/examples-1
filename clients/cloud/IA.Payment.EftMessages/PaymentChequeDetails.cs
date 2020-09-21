namespace ElectronicFundTransfer
{
    /// <summary>
    /// Model for cheque details in a common payment message.
    /// </summary>
    public class PaymentChequeDetails
    {
        public string ChequeNumber { get; set; }
        public string PayeeName { get; set; }
        public PaymentAddress Address { get; set; }
    }
}
