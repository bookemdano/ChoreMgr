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

        internal static Transaction Duplicate(Transaction other)
        {
            var rv = new Transaction(other);
            rv.Id = null;
            rv.Timestamp = DateTime.Now;
            return rv;
        }

        internal string ToCsv()
        {
            return $"{Id},{Timestamp},{Name},{Amount},{Category},{Notes}";
        }

        internal bool Same(Transaction other)
        {
            if (other == null)
                return false;
            if (Name != other.Name)
                return false;
            if (Amount != other.Amount)
                return false;
            if (Category != other.Category)
                return false;
            if (Notes != other.Notes)
                return false;
            return true;
        }
        public override string ToString()
        {
            return $"{Name}({Id?.Substring(0,4)}) {Amount} {Category}";
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
