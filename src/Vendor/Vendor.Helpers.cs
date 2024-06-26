﻿using Neo;
using Neo.SmartContract.Framework.Services;
using System.Numerics;
using static Vendor.Helpers;
using static Vendor.Transfer;

namespace Vendor
{
  public partial class Vendor
  {
    private static void CheckOfferTokenWhiteList(UInt160 offerTokenHash)
    {
      // Error will be thrown when provided offerTokenHash is not whitelisted.
      OfferTokenWhiteListStorage.Get(offerTokenHash);
    }

    private static void InternalCancelTrade(BigInteger tradeId)
    {
      Trade activeTrade = TradePoolStorage.Get(tradeId);
      // Initialize variables
      UInt160 vendor = Runtime.ExecutingScriptHash;
      BigInteger returnOfferAmount = (activeTrade.offerPackages - activeTrade.soldPackages) * activeTrade.amountPerPackage;
      BigInteger returnPurchasedAmount = activeTrade.soldPackages * activeTrade.purchasePrice;
      // return token back to the owner
      if (returnOfferAmount > 0)
        Safe17Transfer(activeTrade.offerTokenHash, vendor, activeTrade.owner, returnOfferAmount);
      if (returnPurchasedAmount > 0)
        Safe17Transfer(activeTrade.purchaseTokenHash, vendor, activeTrade.owner, returnPurchasedAmount);
      // Remove trade
      TradePoolStorage.Delete(tradeId);
      OnTradeCancelled(tradeId, activeTrade.owner, activeTrade.offerTokenHash, returnOfferAmount, activeTrade.purchaseTokenHash, returnPurchasedAmount);
    }

    private static void ValidateSymbol(string symbol)
    {
      Assert(symbol.Length >= 1 && symbol.Length <= 10, $"{CONTRACT_NAME}: Symbol length must be in range of 1 to 10 characters");
    }
  }
}
