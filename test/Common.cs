using Neo;
using System.Numerics;

namespace test
{
  static class Common
  {
    public static readonly byte[] Prefix_Owner = new byte[] { 0x01, 0x00 };
    public static readonly byte[] Prefix_Trade_Pool = new byte[] { 0x01, 0x03 };

    public static readonly BigInteger TEN_GAS = 10_00000000;
    public static readonly BigInteger HUNDRED_GAS = 100_00000000;

    public static readonly BigInteger TEST_TRADE_ID = 1;
    public static readonly BigInteger TEST_OFFER_PACKAGES = 20;
    public static readonly BigInteger TEST_PURCHASE_PACKAGES = 10;
    public static readonly BigInteger TEST_OVER_PURCHASE_PACKAGES = 21;

    public struct Trade
    {
      public UInt160 owner;
      public UInt160 offerTokenHash;
      public BigInteger offerTokenAmount;
      public BigInteger offerPackages;
      public BigInteger amountPerPackage;
      public UInt160 purchaseTokenHash;
      public BigInteger purchasePrice;
      public BigInteger soldPackages;
    }
  }
}
