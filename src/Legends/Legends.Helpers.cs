using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using static Swappables.Helpers;

#pragma warning disable CS8618 // Suppress warning nullable
namespace Swappables
{
  public partial class Legends
  {
    private static LegendsState GetStateById(string tokenId)
    {
      ValidateExistingTokenId(tokenId);
      StorageMap tokenMap = new(Storage.CurrentContext, Prefix_Token);
      return (LegendsState)StdLib.Deserialize(tokenMap[tokenId]);
    }

    private static void ValidateName(string name)
    {
      Assert(name.Length <= 32, $"{CONTRACT_NAME}: Name must not longer than 32 characters");
    }

    private static void ValidateMintTokenId(string tokenId)
    {
      StorageMap tokenMap = new(Storage.CurrentContext, Prefix_Token);
      Assert(tokenMap.Get(tokenId) == null, $"{CONTRACT_NAME}: This token already minted");
    }

    private static void ValidateExistingTokenId(string tokenId)
    {
      StorageMap tokenMap = new(Storage.CurrentContext, Prefix_Token);
      Assert(tokenMap.Get(tokenId) != null, $"{CONTRACT_NAME}: This token not existing");
    }

    private static void ValidateTokenIdForTrade(string tokenId)
    {
      ValidateExistingTokenId(tokenId);
      Assert(TradePoolStorage.IsAvailable(tokenId), $"{CONTRACT_NAME}: {tokenId} in not available in the pool");
    }
  }
}
