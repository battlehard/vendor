using Neo;
using Neo.SmartContract;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using System;
using System.Numerics;

namespace Vendor
{
  public partial class Vendor
  {
    public static void Debug_TradePoolStorage()
    {
      IsOwner();
      Trade mockTrade = new()
      {
        owner = GetContractOwner(),
        offerTokenHash = GAS.Hash,
        offerTokenAmount = 100,
        offerPackages = 10,
        amountPerPackage = 10,
        purchaseTokenHash = GAS.Hash,
        purchasePrice = 10,
        soldPackages = 0
      };
      BigInteger tradeId = 1;
      try
      {
        Trade notFoundTrade = TradePoolStorage.Get(tradeId);
      }
      catch (Exception e)
      {
        Runtime.Notify("Expected error", new object[] { e });
      }
      TradePoolStorage.Put(tradeId, mockTrade);
      LogTradeData(tradeId);
      TradePoolStorage.Delete(tradeId);
      try
      {
        Trade notFoundTrade = TradePoolStorage.Get(tradeId);
      }
      catch (Exception e)
      {
        Runtime.Notify("Expected error", new object[] { e });
      }
    }

    public static void Debug_CreateTrade()
    {
      IsOwner();
      BigInteger TEN_GAS = 10_00000000;
      BigInteger HUNDRED_GAS = 100_00000000;
      BigInteger packages = 10;
      CreateTrade(GAS.Hash, HUNDRED_GAS, packages, GAS.Hash, TEN_GAS);
    }

    public static void Debug_ExecuteTrade()
    {
      IsOwner();
      BigInteger TEN_GAS = 10_00000000;
      BigInteger HUNDRED_GAS = 100_00000000;
      BigInteger packages = 20;
      BigInteger tradeId = 1;
      BigInteger overPurchasePackages = 21;
      BigInteger halfPurchasePackages = 10;
      CreateTrade(GAS.Hash, HUNDRED_GAS, packages, GAS.Hash, TEN_GAS);
      try
      {
        ExecuteTrade(tradeId, overPurchasePackages);
      }
      catch (Exception e)
      {
        Runtime.Notify("Expected error", new object[] { e });
      }
      ExecuteTrade(tradeId, halfPurchasePackages);
      LogTradeData(tradeId);
      ExecuteTrade(tradeId, halfPurchasePackages);
      LogTradeData(tradeId);
      try
      {
        ExecuteTrade(tradeId, halfPurchasePackages);
      }
      catch (Exception e)
      {
        Runtime.Notify("Expected error", new object[] { e });
      }
    }

    private static void LogTradeData(BigInteger tradeId)
    {
      Trade queriedTrade = TradePoolStorage.Get(tradeId);
      Runtime.Notify($"TradeID={tradeId}", new object[] { queriedTrade });
    }
  }
}