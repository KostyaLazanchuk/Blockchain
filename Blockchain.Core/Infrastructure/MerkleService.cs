using Blockchain.Core.Interfaces;
using Blockchain.Core.Models;

namespace Blockchain.Core.Infrastructure
{
    public sealed class MerkleService : IMerkleService
    {
        private readonly IHashService _hash;
        public MerkleService(IHashService hash) => _hash = hash;

        public string ComputeMerkleRoot(IReadOnlyList<Transaction> transaction)
        {
            if (transaction is null || transaction.Count == 0) return _hash.ComputeHex("[]");

            var level = transaction.Select(t => _hash.ComputeHex(t.ToCanonical())).ToList();
            while (level.Count > 1)
            {
                var next = new List<string>((level.Count + 1) / 2);
                for (int i = 0; i < level.Count; i += 2)
                {
                    var left = level[i];
                    var right = (i + 1 < level.Count) ? level[i + 1] : left;
                    next.Add(_hash.ComputeHex(left + right));
                }
                level = next;
            }
            return level[0];
        }
    }
}
