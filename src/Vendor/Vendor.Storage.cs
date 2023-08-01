
using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;

namespace Vendor
{
  partial class Vendor
  {
    private static readonly byte[] Prefix_Owner = new byte[] { 0x01, 0x00 };
    private static readonly byte[] Prefix_Admin_White_List = new byte[] { 0x01, 0x01 };

    /// <summary>
    /// Class <c>AdminWhiteListStorage</c>
    /// Storage of address that can perform admin tasks.
    /// </summary>
    public static class AdminWhiteListStorage
    {
      internal static void Put(UInt160 contractHash)
      {
        StorageMap adminWhiteListMap = new(Storage.CurrentContext, Prefix_Admin_White_List);
        adminWhiteListMap.Put(contractHash, 1);
      }

      internal static ByteString Get(UInt160 contractHash)
      {
        StorageMap adminWhiteListMap = new(Storage.CurrentContext, Prefix_Admin_White_List);
        return adminWhiteListMap.Get(contractHash);
      }

      public static void Delete(UInt160 contractHash)
      {
        StorageMap adminWhiteListMap = new(Storage.CurrentContext, Prefix_Admin_White_List);
        adminWhiteListMap.Delete(contractHash);
      }
    }
  }
}