namespace ElectronicFundTransfer
{
    /// <summary>
    /// Model for EFT details in a common payment message.
    /// </summary>
    public class PaymentEftDetails
    {
        public string BankNumber { get; set; }
        public string Branch { get; set; }
        public string BankAccount { get; set; }
        public string BeneficiaryName { get; set; }
    }
}
