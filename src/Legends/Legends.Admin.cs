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
      bool isNoTargetWallet = true;
      CheckAdminAuthorization();
      if (walletAddress is not null)
      {
        ValidateAddress(walletAddress);
        isNoTargetWallet = false;
      }
      else
      {
        Transaction Tx = (Transaction)Runtime.ScriptContainer;
        walletAddress = Runtime.ExecutingScriptHash;
      }
      ValidateName(name);
      ValidateMintTokenId(name);

      LegendsState mintNftState = new LegendsState
      {
        Owner = walletAddress,
        Name = name,
        ImageUrl = imageUrl,
      };

      // Legends use name as tokenId
      Mint(name, mintNftState);
      // Add minted token to trade pool only when minted to the pool -- i.e. No target wallet address.
      if (isNoTargetWallet) TradePoolStorage.Put(name);
      OnMint(name, imageUrl, name, walletAddress);
    }

    public static void Burn(string tokenId)
    {
      CheckAdminAuthorization();
      ValidateBurnTokenId(tokenId);
      Neo.SmartContract.Framework.Nep11Token<LegendsState>.Burn(tokenId);
      // Remove token from trade pool
      TradePoolStorage.Delete(tokenId);
      OnBurn(tokenId);
    }
  }
}
