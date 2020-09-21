using ElectronicFundTransfer.Enums;
using System;

namespace ElectronicFundTransfer
{
    /// <summary>
    /// Model for common payment messages.
    /// </summary>
   public class PaymentMessage
   {
       public string Version { get; set; }
       public string Context { get; set; }
       public Guid Id { get; set; }
       public string PaymentId { get; set; }
       public PaymentMethod PaymentMethod { get; set; }
       public PaymentType PaymentType { get; set; }
       public TransationType TransactionType { get; set; }
       public string PaymentNote { get; set; }
       public string SystemCode { get; set; }
       public string Currency { get; set; }
       public string CompanyCode { get; set; }
       public string AuthorizationNumber { get; set; }
       public string ContractDealerNumber { get; set; }
       public string FilingDate { get; set; }
       public string LanguageCode { get; set; }
       public string Amount { get; set; }
       public PaymentEftDetails EftDetails { get; set; }
       public PaymentChequeDetails ChequeDetails { get; set; }

       /// <summary>
       /// Reference back to the original Kafka message. Required for committing.
       /// </summary>
       public object MessageReference { get; set; }
   }
}
