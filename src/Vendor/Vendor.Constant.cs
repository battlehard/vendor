using System.Numerics;

namespace Vendor
{
  public partial class Vendor
  {
    private const string CONTRACT_NAME = "Vendor";
    private const int MAX_PAGE_LIMIT = 50;
    private static readonly BigInteger DEFAULT_CREATE_TRADE_FEE = 1_00000000; // 1 GAS
  }
}
