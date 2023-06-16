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

    // TODO: Test that pass two parameters work. Using Debugger.
    public static void Mint(string imageUrl, string name, UInt160? minter = null)
    {
      CheckAdminAuthorization();
      if (minter is not null)
      {
        ValidateAddress(minter);
      }
      else
      {
        // TODO: Make sure that when null, use contract address. Test using Debugger
        Transaction Tx = (Transaction)Runtime.ScriptContainer;
        minter = Runtime.ExecutingScriptHash;
      }

      LegendsState mintNftState = new LegendsState
      {
        Owner = minter,
        Name = name,
        ImageUrl = imageUrl,
      };

      // Legends use name as tokenId
      Mint(name, mintNftState);
      OnMint(name, imageUrl, name, minter);
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
