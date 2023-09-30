
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
    private static readonly byte[] Prefix_Trade_Count = new byte[] { 0x01, 0x03 };
    private static readonly byte[] Prefix_Trade_Pool = new byte[] { 0x01, 0x04 };
    private static readonly byte[] Prefix_Create_Trade_Fee = new byte[] { 0x01, 0x05 };
    private static readonly byte[] Prefix_Offer_Token_White_List = new byte[] { 0x01, 0x06 };

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
    /// Class <c>OfferTokenWhiteListStorage</c>
    /// Storage of hash of token that can put offer.
    /// </summary>
    public static class OfferTokenWhiteListStorage
    {
      internal static void Put(UInt160 contractHash, TokenContractInfo contractInfo)
      {
        StorageMap offerTokenWhiteListMap = new(Storage.CurrentContext, Prefix_Offer_Token_White_List);
        offerTokenWhiteListMap.Put(contractHash, StdLib.Serialize(contractInfo));
      }

      internal static TokenContractInfo Get(UInt160 contractHash)
      {
        StorageMap offerTokenWhiteListMap = new(Storage.CurrentContext, Prefix_Offer_Token_White_List);
        var offerTokenContractInfo = offerTokenWhiteListMap.Get(contractHash);
        string contractAddress = contractHash.ToAddress();
        Assert(offerTokenContractInfo != null, $"Provide token contract hash is not allowed: {contractAddress}");
        return (TokenContractInfo)StdLib.Deserialize(offerTokenContractInfo);
      }

      internal static string[] GetSymbolAndImageUrl(UInt160 contractHash)
      {
        StorageMap offerTokenWhiteListMap = new(Storage.CurrentContext, Prefix_Offer_Token_White_List);
        var offerTokenContractInfo = offerTokenWhiteListMap.Get(contractHash);
        if (offerTokenContractInfo != null)
        {
          TokenContractInfo tokenInfo = (TokenContractInfo)StdLib.Deserialize(offerTokenContractInfo);
          return new string[] { tokenInfo.symbol, tokenInfo.imageUrl };
        }
        else
        {
          return new string[] { "", "" };
        }
      }

      public static void Delete(UInt160 contractHash)
      {
        StorageMap offerTokenWhiteListMap = new(Storage.CurrentContext, Prefix_Offer_Token_White_List);
        offerTokenWhiteListMap.Delete(contractHash);
      }

      internal static List<Map<string, object>> List()
      {
        StorageMap offerTokenWhiteListMap = new(Storage.CurrentContext, Prefix_Offer_Token_White_List);
        Iterator keys = offerTokenWhiteListMap.Find(FindOptions.KeysOnly | FindOptions.RemovePrefix);
        List<Map<string, object>> returnListData = new();
        while (keys.Next())
        {
          UInt160 tokenHash = (UInt160)keys.Value;
          TokenContractInfo tokenInfo = Get(tokenHash);
          Map<string, object> tokenMapData = new();
          tokenMapData["offerTokenHash"] = tokenHash;
          tokenMapData["symbol"] = tokenInfo.symbol;
          tokenMapData["imageUrl"] = tokenInfo.imageUrl;
          returnListData.Add(tokenMapData);
        }
        return returnListData;
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
      internal static void Create(BigInteger tradeId, Trade trade)
      {
        Put(tradeId, trade);
        // Increase count
        Storage.Put(Storage.CurrentContext, Prefix_Trade_Count, Count() + 1);
      }

      internal static void Put(BigInteger tradeId, Trade trade)
      {
        StorageMap tradesMap = new(Storage.CurrentContext, Prefix_Trade_Pool);
        tradesMap.Put(tradeId.ToByteArray(), StdLib.Serialize(trade));
      }

      internal static void Delete(BigInteger tradeId)
      {
        StorageMap tradesMap = new(Storage.CurrentContext, Prefix_Trade_Pool);
        tradesMap.Delete(tradeId.ToByteArray());
        // Decrease count
        Storage.Put(Storage.CurrentContext, Prefix_Trade_Count, Count() - 1);
      }

      internal static Trade Get(BigInteger tradeId)
      {
        StorageMap tradesMap = new(Storage.CurrentContext, Prefix_Trade_Pool);
        byte[] id = tradeId.ToByteArray();
        Assert(tradesMap[id] != null, $"Cannot find the tradeId: {tradeId}");
        return (Trade)StdLib.Deserialize(tradesMap[id]);
      }

      internal static BigInteger Count()
      {
        return (BigInteger)Storage.Get(Storage.CurrentContext, Prefix_Trade_Count);
      }

      internal static List<Map<string, object>> List(BigInteger skipCount, BigInteger pageSize)
      {
        StorageMap tradesMap = new(Storage.CurrentContext, Prefix_Trade_Pool);
        Iterator keys = tradesMap.Find(FindOptions.KeysOnly | FindOptions.RemovePrefix);
        List<Map<string, object>> returnListData = new();
        BigInteger foundKeySeq = 0;
        while (keys.Next())
        {
          if (foundKeySeq >= skipCount && foundKeySeq < (skipCount + pageSize))
          {
            BigInteger tradeId = (BigInteger)keys.Value;
            Trade trade = Get(tradeId);
            string[] symbolAndImageUrl = OfferTokenWhiteListStorage.GetSymbolAndImageUrl(trade.offerTokenHash);

            Map<string, object> tradeMapData = new();
            tradeMapData["id"] = ByteStringToUlong((ByteString)tradeId);
            tradeMapData["owner"] = trade.owner;
            tradeMapData["offerTokenHash"] = trade.offerTokenHash;
            tradeMapData["offerTokenSymbol"] = (ByteString)symbolAndImageUrl[0];
            tradeMapData["offerTokenImageUrl"] = (ByteString)symbolAndImageUrl[1];
            tradeMapData["offerTokenAmount"] = trade.offerTokenAmount;
            tradeMapData["offerPackages"] = trade.offerPackages;
            tradeMapData["amountPerPackage"] = trade.amountPerPackage;
            tradeMapData["purchaseTokenHash"] = trade.purchaseTokenHash;
            tradeMapData["purchasePrice"] = trade.purchasePrice;
            tradeMapData["soldPackages"] = trade.soldPackages;
            returnListData.Add(tradeMapData);
          }
          if (returnListData.Count >= pageSize)
            break;
          foundKeySeq++;
        }
        return returnListData;
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