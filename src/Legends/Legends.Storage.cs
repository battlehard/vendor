
using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;

namespace Swappables
{
  partial class Legends
  {
    private static readonly byte[] Prefix_Owner = new byte[] { 0x01, 0x00 };
    private static readonly byte[] Prefix_Admin_White_List = new byte[] { 0x01, 0x01 };
    private static readonly byte[] Prefix_Trade_Pool = new byte[] { 0x01, 0x02 };

    /// <summary>
    /// Class <c>AdminWhiteListStorage</c>
    /// Storage of address that can perform admin tasks.
    /// </summary>
    public static class AdminWhiteListStorage
    {
      internal static void Put(UInt160 contractHash)
      {
        StorageMap adminWhiteListMap = new(Storage.CurrentContext, Prefix_Admin_White_List);
        adminWhiteListMap.Put(contractHash, 1);
      }

      internal static ByteString Get(UInt160 contractHash)
      {
        StorageMap adminWhiteListMap = new(Storage.CurrentContext, Prefix_Admin_White_List);
        return adminWhiteListMap.Get(contractHash);
      }

      public static void Delete(UInt160 contractHash)
      {
        StorageMap adminWhiteListMap = new(Storage.CurrentContext, Prefix_Admin_White_List);
        adminWhiteListMap.Delete(contractHash);
      }
    }

    /// <summary>
    /// Class <c>TradePoolStorage</c>
    /// Storage of NFT in the pool that available for trade.
    /// </summary>
    public static class TradePoolStorage
    {
      internal static void Put(string tokenId)
      {
        StorageMap tradePoolMap = new(Storage.CurrentContext, Prefix_Trade_Pool);
        tradePoolMap.Put(tokenId, 1);
      }

      public static void Delete(string tokenId)
      {
        StorageMap tradePoolMap = new(Storage.CurrentContext, Prefix_Trade_Pool);
        tradePoolMap.Delete(tokenId);
      }

      public static bool IsAvailable(string tokenId)
      {
        StorageMap tradePoolMap = new(Storage.CurrentContext, Prefix_Trade_Pool);
        if (tradePoolMap[tokenId] is not null) return true;
        else return false;
      }
    }
  }
}