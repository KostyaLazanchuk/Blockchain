using Blockchain.Core.Models;

namespace Blockchain.Core.Interfaces
{
    public interface IMerkleService
    {
        string ComputeMerkleRoot(IReadOnlyList<Transaction> transactions);
    }
}
