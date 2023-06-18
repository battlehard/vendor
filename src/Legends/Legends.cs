using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Services;
using static Swappables.Helpers;
using static Swappables.Transfer;
using System.Numerics;

namespace Swappables
{
  [ManifestExtra("Author", "BATTLE STUDIO")]
  [ManifestExtra("Email", "admin@battlehard.studio")]
  [ManifestExtra("Description", "Swappable Avatars")]
  [SupportedStandards("NEP-11")]
  [ContractPermission("*", "*")]
  public partial class Legends : Nep11Token<LegendsState>
  {
    public static void Trade(string fromTokenId, string toTokenId)
    {
      LegendsState fromState = GetStateById(fromTokenId);
      UInt160 tradingWallet = fromState.Owner;
      Assert(Runtime.CheckWitness(tradingWallet), $"{CONTRACT_NAME}: No NFT ownership");

      ValidateTokenIdForTrade(toTokenId);
      // Transfer trade-in token into contract address -- into the pool
      Safe11Transfer(Runtime.ExecutingScriptHash, Runtime.ExecutingScriptHash, fromTokenId);
      TradePoolStorage.Put(fromTokenId);
      // Transfer trade-out token into trader wallet address -- out of the pool
      Safe11Transfer(Runtime.ExecutingScriptHash, tradingWallet, toTokenId);
      TradePoolStorage.Delete(toTokenId);

      OnTrade(fromTokenId, toTokenId, tradingWallet);
    }

    public static void OnNEP11Payment(UInt160 from, BigInteger amount, ByteString tokenId, object[] data)
    {
    }

    public static void OnNEP17Payment(UInt160 from, BigInteger amount, object data)
    {
    }
  }
}
