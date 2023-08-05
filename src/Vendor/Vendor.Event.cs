using Neo;
using System.ComponentModel;
using System.Numerics;

#pragma warning disable CS8618 // Suppress warning nullable
namespace Vendor
{
  public partial class Vendor
  {
    [DisplayName("TradeCreated")]
    public static event OnTradeCreatedDelegate OnTradeCreated;
    public delegate void OnTradeCreatedDelegate(BigInteger tradeId, Trade creatingTrade);

    [DisplayName("TradeExecuted")]
    public static event OnTradeExecutedDelegate OnTradeExecuted;
    public delegate void OnTradeExecutedDelegate(BigInteger tradeId, BigInteger purchasePackages,
                                                  UInt160 payTokenHash, BigInteger payTokenAmount,
                                                  UInt160 receiveTokenHash, BigInteger receiveTokenAmount);

    [DisplayName("TradeCancelled")]
    public static event OnTradeCancelledDelegate OnTradeCancelled;
    public delegate void OnTradeCancelledDelegate(BigInteger tradeId, UInt160 owner,
                                                  UInt160 offerTokenHash, BigInteger returnOfferTokenAmount,
                                                  UInt160 purchaseTokenHash, BigInteger returnPurchaseTokenAmount);
  }
}
