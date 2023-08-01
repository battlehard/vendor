
using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using System.Numerics;
using static Vendor.Helpers;

namespace Vendor
{
  partial class Vendor
  {
    private static readonly byte[] Prefix_Owner = new byte[] { 0x01, 0x00 };
    private static readonly byte[] Prefix_Admin_White_List = new byte[] { 0x01, 0x01 };
    private static readonly byte[] Prefix_Trade_Height = new byte[] { 0x01, 0x02 };
    private static readonly byte[] Prefix_Trade_Pool = new byte[] { 0x01, 0x03 };
    private static readonly byte[] Prefix_Create_Trade_Fee = new byte[] { 0x01, 0x04 };

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

    public static class TradeHeightStorage
    {
      internal static BigInteger IncreaseByOne()
      {
        BigInteger tradeHeight = TradeHeightStorage.Get() + 1;
        Storage.Put(Storage.CurrentContext, Prefix_Trade_Height, tradeHeight);
        return tradeHeight;
      }

      internal static BigInteger Get()
      {
        return (BigInteger)Storage.Get(Storage.CurrentContext, Prefix_Trade_Height);
      }
    }

    public static class TradePoolStorage
    {
      internal static void Put(BigInteger tradeId, Trade trade)
      {
        StorageMap tradesMap = new(Storage.CurrentContext, Prefix_Trade_Pool);
        tradesMap.Put(tradeId.ToByteArray(), StdLib.Serialize(trade));
      }

      internal static void Delete(BigInteger tradeId)
      {
        StorageMap tradesMap = new(Storage.CurrentContext, Prefix_Trade_Pool);
        tradesMap.Delete(tradeId.ToByteArray());
      }

      internal static Trade Get(BigInteger tradeId)
      {
        StorageMap tradesMap = new(Storage.CurrentContext, Prefix_Trade_Pool);
        byte[] id = tradeId.ToByteArray();
        Assert(tradesMap[id] != null, $"Cannot find the tradeId: {tradeId}");
        return (Trade)StdLib.Deserialize(tradesMap[id]);
      }
    }

    // Store create trade fee
    public static class CreateTradeFeeStorage
    {
      internal static void Put(BigInteger feeAmount)
      {
        Storage.Put(Storage.CurrentContext, Prefix_Create_Trade_Fee, feeAmount);
      }

      internal static BigInteger Get()
      {
        BigInteger createTradeFee = (BigInteger)Storage.Get(Storage.CurrentContext, Prefix_Create_Trade_Fee);
        return createTradeFee > 0 ? createTradeFee : DEFAULT_CREATE_TRADE_FEE;
      }
    }
  }
}