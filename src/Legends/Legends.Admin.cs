using Neo;
using Neo.SmartContract.Framework.Services;
using static Swappables.Helpers;

namespace Swappables
{
  public partial class Legends
  {
    private static void CheckAdminWhiteList()
    {
      var tx = (Transaction)Runtime.ScriptContainer;
      // tx.Sender is transaction signer
      Assert(AdminWhiteListStorage.Get(tx.Sender) != null, $"{CONTRACT_NAME}: No admin authorization");
    }

    private static void CheckAdminAuthorization()
    {
      if (!IsOwner())
      {
        CheckAdminWhiteList();
      }
    }

    /// <summary>
    /// Mint Legends NFT with specified params.
    /// </summary>
    /// <param name="imageUrl">Path to NFT image.</param>
    /// <param name="name">Name of NFT, this will also use as tokenId.</param>
    /// <param name="walletAddress">Mint destination address. In case of store in contract, this param must be `null`</param>
    public static void Mint(string imageUrl, string name, UInt160? walletAddress)
    {
      CheckAdminAuthorization();
      if (walletAddress is not null)
      {
        ValidateAddress(walletAddress);
      }
      else
      {
        Transaction Tx = (Transaction)Runtime.ScriptContainer;
        walletAddress = Runtime.ExecutingScriptHash;
      }
      ValidateName(name);
      ValidateTokenId(name);

      LegendsState mintNftState = new LegendsState
      {
        Owner = walletAddress,
        Name = name,
        ImageUrl = imageUrl,
      };

      // Legends use name as tokenId
      Mint(name, mintNftState);
      OnMint(name, imageUrl, name, walletAddress);
      // TODO: Add minted token to trade pool
    }

    public static void Burn(string tokenId)
    {
      CheckAdminAuthorization();
      Burn(tokenId);
      OnBurn(tokenId);
      // TODO: Remove token from trade pool (if any)
    }
  }
}
