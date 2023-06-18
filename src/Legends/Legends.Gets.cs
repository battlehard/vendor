using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
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

    [Safe]
    public override Map<string, object> Properties(ByteString tokenId)
    {
      StorageMap tokenMap = new(Storage.CurrentContext, Prefix_Token);
      LegendsState state = (LegendsState)StdLib.Deserialize(tokenMap[tokenId]);
      Map<string, object> map = new();
      map["owner"] = state.Owner;
      map["name"] = state.Name;
      map["image"] = state.ImageUrl;
      return map;
    }
  }
}