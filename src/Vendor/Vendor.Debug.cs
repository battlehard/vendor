using Neo;
using Neo.SmartContract;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using System;
using System.Numerics;
using static Vendor.Helpers;

namespace Vendor
{
  public partial class Vendor
  {
    private static readonly UInt160 OFFER_TOKEN_HASH = NEO.Hash;
    private static readonly BigInteger OFFER_TOKEN_AMOUNT = 100; // 100_00000000 for 100 GAS because it has 8 decimal
    private static readonly BigInteger OFFER_PACKAGES = 20;
    private static readonly UInt160 PURCHASE_TOKEN_HASH = NEO.Hash;
    private static readonly BigInteger PURCHASE_PRICE = 10; // 10_00000000 for 100 GAS because it has 8 decimal
    private static readonly BigInteger TRADE_ID = 1;
    private static readonly BigInteger HALF_PURCHASE_PACKAGES = OFFER_PACKAGES / 2;
    private static readonly BigInteger OVER_PURCHASE_PACKAGES = 21;

    public static void Debug_TradePoolStorage()
    {
      IsOwner();
      Trade mockTrade = new()
      {
        owner = GetContractOwner(),
        offerTokenHash = OFFER_TOKEN_HASH,
        offerTokenAmount = OFFER_TOKEN_AMOUNT,
        offerPackages = OFFER_PACKAGES,
        amountPerPackage = OFFER_TOKEN_AMOUNT / OFFER_PACKAGES,
        purchaseTokenHash = PURCHASE_TOKEN_HASH,
        purchasePrice = PURCHASE_PRICE,
        soldPackages = 0
      };
      try
      {
        Trade notFoundTrade = TradePoolStorage.Get(TRADE_ID);
      }
      catch (Exception e)
      {
        Runtime.Notify("Expected error", new object[] { e });
      }
      TradePoolStorage.Put(TRADE_ID, mockTrade);
      LogTradeData(TRADE_ID);
      TradePoolStorage.Delete(TRADE_ID);
      try
      {
        Trade notFoundTrade = TradePoolStorage.Get(TRADE_ID);
      }
      catch (Exception e)
      {
        Runtime.Notify("Expected error", new object[] { e });
      }
    }

    public static void Debug_CreateTrade()
    {
      IsOwner();
      CreateTrade(OFFER_TOKEN_HASH, OFFER_TOKEN_AMOUNT, OFFER_PACKAGES, PURCHASE_TOKEN_HASH, PURCHASE_PRICE);
    }

    public static void Debug_ExecuteTrade()
    {
      IsOwner();
      CreateTrade(OFFER_TOKEN_HASH, OFFER_TOKEN_AMOUNT, OFFER_PACKAGES, PURCHASE_TOKEN_HASH, PURCHASE_PRICE);
      try
      {
        ExecuteTrade(TRADE_ID, OVER_PURCHASE_PACKAGES);
      }
      catch (Exception e)
      {
        Runtime.Notify("Expected error", new object[] { e });
      }
      ExecuteTrade(TRADE_ID, HALF_PURCHASE_PACKAGES);
      LogTradeData(TRADE_ID);
      ExecuteTrade(TRADE_ID, HALF_PURCHASE_PACKAGES);
      LogTradeData(TRADE_ID);
      try
      {
        ExecuteTrade(TRADE_ID, HALF_PURCHASE_PACKAGES);
      }
      catch (Exception e)
      {
        Runtime.Notify("Expected error", new object[] { e });
      }
    }

    public static void Debug_CancelTrade()
    {
      IsOwner();
      BigInteger initialBalance = GetBalance(NEO.Hash, GetContractOwner());

      BigInteger tradeId = TRADE_ID;
      // Cancel with no purchase
      CreateTrade(OFFER_TOKEN_HASH, OFFER_TOKEN_AMOUNT, OFFER_PACKAGES, PURCHASE_TOKEN_HASH, PURCHASE_PRICE);
      Assert(GetBalance(NEO.Hash, GetContractOwner()) == initialBalance - OFFER_TOKEN_AMOUNT, "Unexpected Error");
      CancelTrade(tradeId);
      Assert(GetBalance(NEO.Hash, GetContractOwner()) == initialBalance, "Unexpected Error");

      // Cancel with partial purchase
      tradeId += 1;
      CreateTrade(OFFER_TOKEN_HASH, OFFER_TOKEN_AMOUNT, OFFER_PACKAGES, PURCHASE_TOKEN_HASH, PURCHASE_PRICE);
      ExecuteTrade(tradeId, HALF_PURCHASE_PACKAGES);
      Assert(GetBalance(NEO.Hash, GetContractOwner()) == initialBalance
                                                        - OFFER_TOKEN_AMOUNT // Created Trade
                                                        - (HALF_PURCHASE_PACKAGES * PURCHASE_PRICE) // Payment
                                                        + (HALF_PURCHASE_PACKAGES * (OFFER_TOKEN_AMOUNT / OFFER_PACKAGES)) // Receive
                                                        , "Unexpected Error");
      CancelTrade(tradeId);

      // Cancel with full purchase
      tradeId += 1;
      CreateTrade(OFFER_TOKEN_HASH, OFFER_TOKEN_AMOUNT, OFFER_PACKAGES, PURCHASE_TOKEN_HASH, PURCHASE_PRICE);
      ExecuteTrade(tradeId, OFFER_PACKAGES);
      CancelTrade(tradeId);
    }

    private static void LogTradeData(BigInteger tradeId)
    {
      Trade queriedTrade = TradePoolStorage.Get(tradeId);
      Runtime.Notify($"TradeID={tradeId}", new object[] { queriedTrade });
    }

    private static BigInteger GetBalance(UInt160 tokenHash, UInt160 walletHash)
    {
      return (BigInteger)Contract.Call(tokenHash, "balanceOf", CallFlags.All, new object[] { walletHash });
    }
  }
}