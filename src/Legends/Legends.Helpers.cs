using Neo.SmartContract.Framework.Services;
using static Swappables.Helpers;

#pragma warning disable CS8618 // Suppress warning nullable
namespace Swappables
{
  public partial class Legends
  {
    private static void ValidateName(string name)
    {
      Assert(name.Length <= 32, $"{CONTRACT_NAME}: Name must not longer than 32 characters");
    }

    private static void ValidateMintTokenId(string tokenId)
    {
      StorageMap tokenMap = new(Storage.CurrentContext, Prefix_Token);
      Assert(tokenMap.Get(tokenId) == null, $"{CONTRACT_NAME}: This token already minted");
    }

    private static void ValidateBurnTokenId(string tokenId)
    {
      StorageMap tokenMap = new(Storage.CurrentContext, Prefix_Token);
      Assert(tokenMap.Get(tokenId) != null, $"{CONTRACT_NAME}: This token not existing");
    }
  }
}
