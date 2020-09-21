using ElectronicFundTransfer.Enums;
using System;

namespace ElectronicFundTransfer
{
   /// <summary>
   /// Model for common payment messages.
   /// </summary>
   public class BankResultMessage
   {
      public Guid Id { get; set; }
      public string Specversion { get; set; }
      public BankResultType Type { get; set; }
      public string Version { get; set; }
      public string Subject { get; set; }
      public DateTime Time { get; set; }
      public SourceSystem Source { get; set; }
      public DataContentType DataContentType { get; set; }
      public Guid CorrelationId { get; set; }
      public BankResultTopic Topic { get; set; }
      public EnvType Env { get; set; }
      public BankResultMessageBody data { get; set; }

   }
}
