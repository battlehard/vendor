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
    public delegate void OnTradeCreatedDelegate(BigInteger tradeId);
  }
}
