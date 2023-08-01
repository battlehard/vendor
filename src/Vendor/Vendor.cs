using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Services;
using static Vendor.Helpers;
using static Vendor.Transfer;
using System.Numerics;

namespace Vendor
{
  [ManifestExtra("Author", "BATTLE HARD")]
  [ManifestExtra("Email", "admin@battlehard.studio")]
  [ManifestExtra("Description", "Trade House")]
  [ContractPermission("*", "*")]
  public partial class Vendor : SmartContract
  {
    public static void CreateTrade()
    {

    }

    public static void ExecuteTrade()
    {

    }

    public static void CancelTrade()
    {

    }
  }
}
