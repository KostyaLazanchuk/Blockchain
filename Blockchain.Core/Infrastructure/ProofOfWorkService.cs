using Blockchain.Core.Interfaces;

namespace Blockchain.Core.Infrastructure
{
    public sealed class ProofOfWorkService : IProofOfWork
    {
        private readonly IHashService _hash;
        private readonly string _suffix;

        public ProofOfWorkService(IHashService hash, string suffix)
        {
            _hash = hash;
            _suffix = suffix;
        }

        public int FindProof(int lastProof)
        {
            var proof = 0;
            while (!IsValid(lastProof, proof)) proof++;
            return proof;
        }

        public bool IsValid(int lastProof, int proof)
        {
            var guess = $"{lastProof}{proof}";
            var hex = _hash.CoputeHex(guess);
            return hex.EndsWith(_suffix, StringComparison.Ordinal);
        }
    }
}
