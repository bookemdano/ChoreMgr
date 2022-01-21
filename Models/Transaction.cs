using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace ChoreMgr.Models
{
    public class Transaction
    {
        public Transaction()
        {
            Name = String.Empty;
            Timestamp = DateTime.Now;
        }
        public Transaction(Transaction? transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));
            Id = transaction.Id;
            Name = transaction.Name;
            Category = transaction.Category;
            Notes = transaction.Notes;
            Amount = transaction.Amount;
            Timestamp = transaction.Timestamp;
        }
        internal void Update(Transaction other)
        {
            Name = other.Name;
            Category = other.Category;
            Notes = other.Notes;
            Amount = other.Amount;
            Timestamp = other.Timestamp;
        }

        public string? Id { get; set; }
        public string Name { get; set; }
        public string? Category { get; set; }
        public string? Notes { get; set; }
        [Display(Name = "Amount($)")]
        [DisplayFormat(DataFormatString = "{0:C0}")]
        public decimal Amount { get; set; }
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:g}", ApplyFormatInEditMode = true)]
        public DateTime Timestamp { get; set; }

        internal static string CsvHeader()
        {
            return "Id,Timestamp,Name,Amount,Category,Notes";
        }
        internal string ToCsv()
        {
            return $"{Id},{Timestamp},{Name},{Amount},{Category},{Notes}";
        }
    }
    public class TransactionModel : Transaction
    {
        public TransactionModel()
        {

        }
        public TransactionModel(Transaction? other) : base(other)
        {

        }

        public string AmountStyle
        {
            get
            {
                var rv = "text-align:center;";
                if (Amount < 0)
                    rv = rv + "background-color:lightgreen;";
                return rv;
            }
        }
    }
}
