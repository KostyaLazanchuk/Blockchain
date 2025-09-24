using Blockchain.Core.Interfaces;
using Blockchain.Core.Models;
using System.Net.Http.Json;

namespace Blockchain.Core.Services
{
    public sealed class BlockchainService
    {
        private readonly IHashService _hash;
        private readonly IProofOfWork _pow;
        private readonly IBlockRepository _block;
        private readonly IMerkleService _merkle;

        public BlockchainService(IHashService hash, IProofOfWork pow,
            IBlockRepository block, IMerkleService merkle)
        {
            _hash = hash;
            _pow = pow;
            _block = block;
            _merkle = merkle;
        }

        private readonly HashSet<string> _nodes = new(StringComparer.OrdinalIgnoreCase);
        public IReadOnlyCollection<string> Nodes => _nodes;

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
            var markleRoot = _merkle.ComputeMerkleRoot(mempool);

            var block = new Block
            {
                Index = last.Index + 1,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Transactions = mempool.ToList(),
                Proof = proof,
                PreviousHash= _hash.ComputeHex(last.ToCanonical()),
                Nonce = null,
                MerkleRoot = markleRoot,
            };

            _block.Append(block);
            mempool.Clear();
            return block;
        }

        public string Hash(Block block) => _hash.ComputeHex(block.ToCanonical());

        public void RegisterNode(string address)
        {
            if (string.IsNullOrWhiteSpace(address)) return;

            if (Uri.TryCreate(address, UriKind.Absolute, out var abs))
            {
                if (abs.Port <= 0) return;
                _nodes.Add($"{abs.Host}:{abs.Port}");
            }
            else
            {
                var s = address.Trim().TrimEnd('/');
                if (!s.Contains(":")) return;
                _nodes.Add(s);
            }
        }

        public bool ValidChain(IList<Block> chain)
        {
            if (chain == null || chain.Count == 0) return false;

            for (int i = 1; i < chain.Count; i++)
            {
                var prev = chain[i - 1];
                var curr = chain[i];

                var prevHash = _hash.ComputeHex(prev.ToCanonical());
                if (!string.Equals(curr.PreviousHash, prevHash, StringComparison.OrdinalIgnoreCase))
                    return false;

                if (!_pow.IsValid(prev.Proof, curr.Proof))
                    return false;
            }
            return true;
        }

        public async Task<bool> ResolveConflictsAsync(IHttpClientFactory httpClientFactory)
        {
            if (_nodes.Count == 0) return false;

            var client = httpClientFactory.CreateClient();
            var maxLength = Chain.Count;
            List<Block>? newChain = null;

            foreach (var node in _nodes)
            {
                try
                {
                    var url = $"http://{node}/blockchain/chain";
                    using var resp = await client.GetAsync(url);
                    if (!resp.IsSuccessStatusCode) continue;

                    var dto = await resp.Content.ReadFromJsonAsync<Chain>();
                    if (dto is null || dto.chain is null) continue;

                    var foreignChain = dto.chain;
                    var foreignLen = dto.length;

                    if (foreignLen > maxLength && ValidChain(foreignChain))
                    {
                        maxLength = foreignLen;
                        newChain = foreignChain;
                    }
                }
                catch
                {
                }
            }

            if (newChain != null)
            {
                var current = _block.GetChain();
                if (current is List<Block> list)
                {
                    list.Clear();
                    list.AddRange(newChain);
                }
                else
                {
                    foreach (var b in newChain.Skip(current.Count))
                        _block.Append(b);
                }
                return true;
            }

            return false;
        }
    }
}
