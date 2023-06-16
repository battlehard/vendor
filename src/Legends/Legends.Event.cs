using Neo;
using System.ComponentModel;

#pragma warning disable CS8618 // Suppress warning nullable
namespace Swappables
{
  public partial class Legends
  {
    [DisplayName("LegendsMint")]
    public static event OnMintDelegate OnMint;
    public delegate void OnMintDelegate(string tokenId, string imageUrl, string name, UInt160 walletAddress);

    [DisplayName("LegendsBurn")]
    public static event OnBurnDelegate OnBurn;
    public delegate void OnBurnDelegate(string tokenId);
  }
}
