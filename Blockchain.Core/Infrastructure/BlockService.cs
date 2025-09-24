using Blockchain.Core.Interfaces;
using Blockchain.Core.Models;

namespace Blockchain.Core.Infrastructure
{
    public sealed class BlockService : IBlockRepository
    {
        private readonly List<Block> _chain = new();
        public IReadOnlyList<Block> GetChain() => _chain.AsReadOnly();
        public Block GetLast() => _chain[^1];
        public void Append(Block block) => _chain.Add(block);
    }
}
