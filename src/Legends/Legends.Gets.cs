using Neo;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Services;

namespace Swappables
{
  public partial class Legends
  {
    [Safe]
    public override string Symbol() => "LEGENDS";

    [Safe]
    public static UInt160 GetContractOwner()
    {
      return (UInt160)Storage.Get(Storage.CurrentContext, Prefix_Owner);
    }
  }
}