using Neo;
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
  }
}