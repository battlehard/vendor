using Neo;
using Neo.SmartContract.Framework;
using System;

namespace Vendor
{
  public class Helpers
  {
    public static void ValidateAddress(UInt160 address)
    {
      Assert(address is not null && address.IsValid, "The address is invalid");
    }

    public static void Assert(bool condition, string errorMessage)
    {
      if (!condition)
      {
        throw new Exception(errorMessage);
      }
    }

    public static ulong ByteStringToUlong(ByteString bs)
    {
      ulong result = 0;
      byte[] data = Helper.ToByteArray(bs);
      for (int i = 0; i < data.Length; i++)
      {
        result <<= 8; // Shift left by 8 bits
        result |= data[i]; // OR with the next byte
      }
      return result;
    }
  }
}