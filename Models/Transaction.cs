using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace ChoreMgr.Models
{
    public class Transaction
    {
        public Transaction()
        {
            Name = String.Empty;
        }
        public Transaction(Transaction? Transaction)
        {
            if (Transaction == null)
                throw new ArgumentNullException(nameof(Transaction));
            Id = Transaction.Id;
            Name = Transaction.Name;
            Category = Transaction.Category;
            Amount = Transaction.Amount;
        }

        public string? Id { get; set; }
        public string Name { get; set; }
        public string? Category { get; set; }
        public decimal Amount { get; set; }
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
