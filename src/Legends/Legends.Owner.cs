using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using static Swappables.Helpers;

#pragma warning disable CS8618 // Suppress warning nullable
namespace Swappables
{
  public partial class Legends
  {
    public static void _deploy(object data, bool update)
    {
      if (update) return;
      var tx = (Transaction)Runtime.ScriptContainer;
      Storage.Put(Storage.CurrentContext, Prefix_Owner, tx.Sender);
    }

    public static void Update(ByteString nefFile, string manifest)
    {
      IsContractOwner();
      ContractManagement.Update(nefFile, manifest, null);
    }

    private static void IsContractOwner()
    {
      Assert(Runtime.CheckWitness(GetContractOwner()), $"{CONTRACT_NAME}: No owner authorization");
    }

    public static void AddAdminWhiteList(UInt160 contractHash)
    {
      IsContractOwner();
      AdminWhiteListStorage.Put(contractHash);
    }

    public static void RemoveAdminWhitelist(UInt160 contractHash)
    {
      IsContractOwner();
      AdminWhiteListStorage.Delete(contractHash);
    }
  }
}