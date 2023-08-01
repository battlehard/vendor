using Neo;
using Neo.SmartContract;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using System;
using System.Numerics;

#pragma warning disable CS8625 // Suppress known warning
namespace Vendor
{
  public partial class Vendor
  {
    public static void Debug_TradePoolStorage()
    {
      IsOwner();
      Trade mockTrade = new Trade
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
      try
      {
        Trade notFoundTrade = TradePoolStorage.Get(1);
      }
      catch (Exception e)
      {
        Runtime.Notify("Expected error", new object[] { e });
      }
      TradePoolStorage.Put(1, mockTrade);
      Trade queriedTrade = TradePoolStorage.Get(1);
      Runtime.Notify("TradeID=1", new object[] { queriedTrade });
      TradePoolStorage.Delete(1);
      try
      {
        Trade notFoundTrade = TradePoolStorage.Get(1);
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

  }
}