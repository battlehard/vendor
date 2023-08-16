using System.Numerics;
using Neo;

namespace Vendor
{
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

  public struct TokenContractInfo
  {
    public string symbol;
    public string imageUrl;
  }
}
