using ElectronicFundTransfer.Enums;
using System;

namespace ElectronicFundTransfer
{
   /// <summary>
   /// Model for common payment messages.
   /// </summary>
   public class BankResultMessageBody
   {
      public string Context { get; set; }
      public DateTime BankReportDate { get; set; }
      public string BankReportName { get; set; }
      public PaymentMethod PaymentMethod { get; set; }
      public string LanguageCode { get; set; }
      public bool BankPaymentSuccessful { get; set; }
      public string Currency { get; set; }
      public string CompanyCode { get; set; }
      public string CustomerAccount { get; set; }
      public string AuthorizationNumber { get; set; }
      public string ContractDealerNumber { get; set; }      
      public DateTime DepositDate { get; set; }
      public Double Amount { get; set; }
      public PaymentEftDetails EftDetails { get; set; }
   }
}
