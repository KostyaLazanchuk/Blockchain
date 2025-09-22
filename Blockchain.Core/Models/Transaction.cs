using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Blockchain.Core.Models
{
    public sealed class Transaction
    {
        public string Sender { get; }
        public string Recipient { get; }
        public int Amount { get; }

        public Transaction(string sender, string recipient, int amount) 
        {
            Sender = sender;
            Recipient = recipient;
            Amount = amount;
        }

        public string ToCanonical() => $"{Sender}|{Recipient}|{Amount}";
    }
}
