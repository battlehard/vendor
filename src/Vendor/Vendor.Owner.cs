using System.Numerics;
using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using static Vendor.Helpers;

#pragma warning disable CS8618 // Suppress warning nullable
namespace Vendor
{
  public partial class Vendor
  {
    private static bool IsOwner()
    {
      var contractOwner = GetContractOwner();
      var tx = (Transaction)Runtime.ScriptContainer;
      return contractOwner.Equals(tx.Sender) && Runtime.CheckWitness(contractOwner);
    }

    private static void CheckOwner()
    {
      Assert(IsOwner(), $"{CONTRACT_NAME}: No owner authorization");
    }

    public static void _deploy(object data, bool update)
    {
      if (update) return;
      var tx = (Transaction)Runtime.ScriptContainer;
      Storage.Put(Storage.CurrentContext, Prefix_Owner, tx.Sender);
      Storage.Put(Storage.CurrentContext, Prefix_Debug, 0);
    }

    public static void Update(ByteString nefFile, string manifest)
    {
      CheckOwner();
      ContractManagement.Update(nefFile, manifest, null);
      Storage.Put(Storage.CurrentContext, Prefix_Debug, 0);
    }

    public static void AddAdminWhiteList(UInt160 contractHash)
    {
      CheckOwner();
      AdminWhiteListStorage.Put(contractHash);
    }

    public static void RemoveAdminWhiteList(UInt160 contractHash)
    {
      CheckOwner();
      AdminWhiteListStorage.Delete(contractHash);
    }

    public static void ResetTradePoolCountTo(BigInteger count)
    {
      CheckOwner();
      Storage.Put(Storage.CurrentContext, Prefix_Trade_Count, count);
    }
  }
}