using Blockchain.Core.Infrastructure;
using Blockchain.Core.Interfaces;
using Blockchain.Core.Models;
using Blockchain.Core.Services;

var SURNAME = "Lazanchuk";
var DOB = "03032001";
var MONTH = "03";

IHashService hash = new HashService();
IProofOfWork pow = new ProofOfWorkService(hash, MONTH);
IMerkleService merkle = new MerkleService(hash);
var repo = new BlockService();
var bc = new BlockchainService(hash, pow, repo, merkle);

bc.CreateGenesis(SURNAME, DOB);

var mempool = new List<Transaction>();

bc.AddTransaction("Alice", "Bob", 10, mempool);
bc.AddTransaction("MinerReward", "Kostya", 1, mempool);

var proof = bc.MineNextProof();
var newBlock = bc.CreateBlock(proof, mempool);

PrintBlock(bc.Chain[0], bc.Hash(bc.Chain[0]), "== Genesis-block ==");
PrintBlock(newBlock, bc.Hash(newBlock), "\n== New block after PoW ==");

var lastProof = bc.Chain[0].Proof;
var checkHex = hash.ComputeHex($"{lastProof}{proof}");
Console.WriteLine($"\nPoW check: sha256(\"{lastProof}{proof}\") = {checkHex}");
Console.WriteLine($"endswith(\"{MONTH}\") = {checkHex.EndsWith(MONTH, StringComparison.Ordinal)}");

static void PrintBlock(Block b, string hash, string title)
{
    Console.WriteLine(title);
    Console.WriteLine(new string('-', 74));
    Console.WriteLine($"Index         : {b.Index}");
    Console.WriteLine($"Timestamp     : {b.Timestamp}");
    Console.WriteLine($"Proof         : {b.Proof}");
    Console.WriteLine($"PreviousHash  : {b.PreviousHash}");
    Console.WriteLine($"Nonce         : {b.Nonce}");
    Console.WriteLine($"Tx count      : {b.Transactions.Count}");
    foreach (var tx in b.Transactions)
        Console.WriteLine($"  - {tx.Sender} -> {tx.Recipient} : {tx.Amount}");
    Console.WriteLine($"Hash(block)   : {hash}");
    Console.WriteLine(new string('-', 74));
}
