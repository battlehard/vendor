using System.Numerics;
using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Services;

namespace Swappables
{
  [ManifestExtra("Author", "BATTLE STUDIO")]
  [ManifestExtra("Email", "admin@battlehard.studio")]
  [ManifestExtra("Description", "Swappable Avatars")]
  [SupportedStandards("NEP-11")]
  [ContractPermission("*", "*")]
  public partial class Legends : Nep11Token<LegendsState>
  {
    public static void OnNEP11Payment(UInt160 from, BigInteger amount, ByteString tokenId, object[] data)
    {
    }

    public static void OnNEP17Payment(UInt160 from, BigInteger amount, object data)
    {
    }
  }
}
