using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Services;
using System.Numerics;
using static Vendor.Helpers;

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
    public static Map<string, object> ListTrade(BigInteger pageNumber, BigInteger pageSize)
    {
      Assert(pageNumber > 0 && pageSize > 0, "Pagination data must be provided, pageNumber and pageSize must have at least 1");
      Assert(pageSize <= MAX_PAGE_LIMIT, $"Input page limit exceed the max limit of {MAX_PAGE_LIMIT}");

      BigInteger totalTrades = TradePoolStorage.Count();
      // Calculate the total number of pages based on the total trades and page size
      BigInteger totalPages = totalTrades / pageSize;
      if (totalTrades % pageSize > 0)
      {
        totalPages += 1;
      }
      Assert(pageNumber <= totalPages, $"Input page number exceed the totalPages of {totalPages}");

      // Calculate the number of items to skip based on the requested page and page size
      BigInteger skipCount = (pageNumber - 1) * pageSize;

      // Initialize return variable
      Map<string, object> tradePaginationData = new();
      tradePaginationData["totalPages"] = totalPages;
      tradePaginationData["totalTrades"] = totalTrades;
      // Get list of active trades with pagination parameters
      tradePaginationData["tradeList"] = TradePoolStorage.List(skipCount, pageSize);
      return tradePaginationData;
    }

    [Safe]
    public static List<Map<string, object>> ListOfferTokenWhiteList()
    {
      return OfferTokenWhiteListStorage.List();
    }
  }
}