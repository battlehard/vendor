using Neo;
using Neo.SmartContract.Framework.Services;
using System.Numerics;
using static Vendor.Transfer;

namespace Vendor
{
  public partial class Vendor
  {
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
  }
}
