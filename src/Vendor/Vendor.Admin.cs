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
  }
}
