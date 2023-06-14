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
  }
}
