﻿using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using static Vendor.Helpers;
using static Vendor.Transfer;
using System;
using System.Numerics;

namespace Vendor
{
  [ManifestExtra("Author", "BATTLE HARD")]
  [ManifestExtra("Email", "admin@battlehard.studio")]
  [ManifestExtra("Description", "Trade House")]
  [ContractPermission("*", "*")]
  public partial class Vendor : SmartContract
  {
    /// <summary>
    /// Create a trade with any choice of tokens for offer and purchase in a vending machine style where tokens bundle in packages for sell with set price and currency
    /// </summary>
    /// <param name="offerTokenHash">Script hash of token for sell</param>
    /// <param name="offerTokenAmount">Total amount for sell with no decimal format. If the token use 8 decimal, then 1 token must input as 100000000</param>
    /// <param name="offerPackages">Number of packages for sell, must be evenly divisible</param>
    /// <param name="purchaseTokenHash">Script Hash of token that use for purchase</param>
    /// <param name="offerTokpurchasePriceenHash">Price per package with no decimal format</param>
    public static void CreateTrade(UInt160 offerTokenHash, BigInteger offerTokenAmount, BigInteger offerPackages, UInt160 purchaseTokenHash, BigInteger purchasePrice)
    {
      // Initialize variables
      var tx = (Transaction)Runtime.ScriptContainer;
      UInt160 sender = tx.Sender;
      UInt160 vendor = Runtime.ExecutingScriptHash;
      // Calculate number of tokens per package
      Assert(offerTokenAmount % offerPackages == 0, $"Unable to allocate same amount of token per package");
      BigInteger amountPerPackage = offerTokenAmount / offerPackages;
      // Generate tradeId using running number.
      BigInteger tradeId = TradeHeightStorage.IncreaseByOne();
      // Collect GAS fee from user
      Safe17Transfer(GAS.Hash, sender, vendor, CreateTradeFeeStorage.Get());
      // Lock offer token from user into vendor contract
      Safe17Transfer(offerTokenHash, sender, vendor, offerTokenAmount);
      // Create a trade object
      Trade creatingTrade = new Trade
      {
        owner = sender,
        offerTokenHash = offerTokenHash,
        offerTokenAmount = offerTokenAmount,
        offerPackages = offerPackages,
        amountPerPackage = amountPerPackage,
        purchaseTokenHash = purchaseTokenHash,
        purchasePrice = purchasePrice,
        soldPackages = 0
      };
      TradePoolStorage.Put(tradeId, creatingTrade);
      OnTradeCreated(tradeId, creatingTrade);
    }

    public static void ExecuteTrade()
    {

    }

    public static void CancelTrade()
    {

    }

    public static void OnNEP11Payment(UInt160 from, BigInteger amount, ByteString tokenId, object[] data)
    {
      throw new Exception($"{CONTRACT_NAME} reject NFT transfer");
    }

    public static void OnNEP17Payment(UInt160 from, BigInteger amount, object data)
    {
    }
  }
}