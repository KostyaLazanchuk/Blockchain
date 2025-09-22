using Blockchain.Core.Interfaces;
using Blockchain.Core.Models;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockchain.Core.Services
{
    public sealed class BlockchainService
    {
        private readonly IHashService _hash;
        private readonly IProofOfWork _pow;
        private readonly IBlockRepository _block;

        public BlockchainService(IHashService hash, IProofOfWork pow, IBlockRepository block)
        {
            _hash = hash;
            _pow = pow;
            _block = block;
        }

        public IReadOnlyList<Block> Chain => _block.GetChain();
        public Block Last => _block.GetLast();

        public void CreateGenesis(string surname, string dayMonthYear)
        {
            if (_block.GetChain().Count > 0) return;

            var genesis = new Block
            {
                Index = 1,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Transactions = Array.Empty<Transaction>(),
                Proof = 0,
                PreviousHash = surname,
                Nonce = dayMonthYear,
            };

            _block.Append(genesis);
        }

        public int AddTransaction(string sender, string recipient, int amount, List<Transaction> mempool)
        {
            mempool.Add(new Transaction(sender, recipient, amount));
            return _block.GetLast().Index + 1;
        }

        public int MineNextProof() => _pow.FindProof(_block.GetLast().Proof);

        public Block CreateBlock(int proof, List<Transaction> mempool)
        {
            var last = _block.GetLast();
            var block = new Block
            {
                Index = last.Index + 1,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Transactions = mempool.ToList(),
                Proof = proof,
                PreviousHash= _hash.CoputeHex(last.ToCanonical()),
                Nonce = null
            };

            _block.Append(block);
            mempool.Clear();
            return block;
        }

        public string Hash(Block block) => _hash.CoputeHex(block.ToCanonical());
    }
}
