using Neo;
using Neo.SmartContract.Framework.Services;
using System.Numerics;
using static Vendor.Helpers;

namespace Vendor
{
  public partial class Vendor
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

    public static void AdminCancelTrade(BigInteger tradeId)
    {
      CheckAdminAuthorization();
      InternalCancelTrade(tradeId);
    }

    public static void AddOfferTokenWhiteList(UInt160 contractHash, string symbol, string imageUrl)
    {
      CheckAdminAuthorization();
      ValidateSymbol(symbol);
      TokenContractInfo offerTokenInfo = new()
      {
        symbol = symbol,
        imageUrl = imageUrl
      };
      OfferTokenWhiteListStorage.Put(contractHash, offerTokenInfo);
    }

    public static void RemoveOfferTokenWhiteList(UInt160 contractHash)
    {
      CheckAdminAuthorization();
      OfferTokenWhiteListStorage.Delete(contractHash);
    }
  }
}
