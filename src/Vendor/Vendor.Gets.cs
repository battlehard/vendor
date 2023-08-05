using Neo;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Services;

namespace Vendor
{
  public partial class Vendor
  {
    [Safe]
    public static UInt160 GetContractOwner()
    {
      return (UInt160)Storage.Get(Storage.CurrentContext, Prefix_Owner);
    }

    [Safe]
    public static void ListTrade()
    {

    }
  }
}