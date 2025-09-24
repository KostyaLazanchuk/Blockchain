namespace Blockchain.Core.Models
{
    public sealed class Block
    {
        public int Index { get; set; }
        public long Timestamp { get; set; }
        public IReadOnlyCollection<Transaction> Transactions { get; init; } = Array.Empty<Transaction>();
        public int Proof { get; set; }
        public string PreviousHash { get; init; } = string.Empty;
        public string? Nonce { get; init; }
        public string MerkleRoot { get; init; } = string.Empty;

        public string ToCanonical()
        {
            var parts = new List<string>(5 + Transactions.Count)
            {
                Index.ToString(), Timestamp.ToString(), Proof.ToString(), PreviousHash, Nonce ?? ""
            };

            foreach (var tx in Transactions) parts.Add(tx.ToCanonical());
            return string.Join("|", parts);
        }

    }
}
