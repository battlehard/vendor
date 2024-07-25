using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services;
using static Vendor.Helpers;
using System;
using System.Numerics;

namespace Vendor
{
  public class Transfer
  {
#pragma warning disable CS8625 // Suppress known warning
    private const string TRANSFER_METHOD = "transfer";
    private const string NEP11_TRANSFER_FAILED = "NEP11 transfer failed";
    private const string NEP17_TRANSFER_FAILED = "NEP17 transfer failed";

    public static void Safe11Transfer(UInt160 contractHash, UInt160 to, ByteString tokenId)
    {
      string contractAddress = contractHash.ToAddress();
      string failedMessage = $"{NEP11_TRANSFER_FAILED}: {contractAddress}";
      bool result = (bool)Contract.Call(contractHash, TRANSFER_METHOD, CallFlags.All, new object[] { to, tokenId, null });
      Assert(result, failedMessage);
    }

    public static void Safe17Transfer(UInt160 contractHash, UInt160 from, UInt160 to, BigInteger amount)
    {
      string contractAddress = contractHash.ToAddress();
      string failedMessage = $"{NEP17_TRANSFER_FAILED}: {contractAddress}";
      var result = (bool)Contract.Call(contractHash, TRANSFER_METHOD, CallFlags.All, new object[] { from, to, amount, null });
      Assert(result, failedMessage);
    }
#pragma warning restore CS8625 // Suppress known warning
  }
}