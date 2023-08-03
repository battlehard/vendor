using FluentAssertions;
using Neo;
using Neo.Assertions;
using Neo.BlockchainToolkit;
using Neo.BlockchainToolkit.Models;
using Neo.BlockchainToolkit.SmartContract;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.SmartContract;
using Neo.SmartContract.Native;
using Neo.VM;
using Neo.VM.Types;
using NeoTestHarness;
using System;
using System.Collections.Generic;
using System.Numerics;
using TestVendor; // For reference contract method interface e.g. Vendor
using Xunit;

namespace test
{
  // This checkpoint already add admin wallet to whitelist
  [CheckpointPath("checkpoints/vendor-deployed.neoxp-checkpoint")]
  public class VendorTradeTests : IClassFixture<CheckpointFixture<VendorTradeTests>>
  {
    readonly CheckpointFixture fixture;
    readonly ExpressChain chain;

    public VendorTradeTests(CheckpointFixture<VendorTradeTests> fixture)
    {
      this.fixture = fixture;
      this.chain = fixture.FindChain();
    }

    [Fact]
    public void Create_trade()
    {
      ProtocolSettings settings = chain.GetProtocolSettings();
      UInt160 owner = chain.GetDefaultAccount("owner").ToScriptHash(settings.AddressVersion);

      using SnapshotCache snapshot = fixture.GetSnapshot();
      // Set WitnessScope as Global here to only test login, not permission.
      using TestApplicationEngine engine = new(snapshot, settings, owner, WitnessScope.Global);
      UInt160 gasHash = NativeContract.GAS.Hash;
      BigInteger hundredGas = Common.HUNDRED_GAS;
      BigInteger tenGas = Common.TEN_GAS;
      BigInteger offerPackages = 10;
      engine.ExecuteScript<Vendor>(c => c.createTrade(gasHash, hundredGas, offerPackages, gasHash, tenGas));
      engine.State.Should().Be(VMState.HALT);
      engine.ResultStack.Should().HaveCount(1);
      var storages = snapshot.GetContractStorages<Vendor>();
      var tradeMap = storages.StorageMap(Common.Prefix_Trade_Pool);
      BigInteger tradeId = 1;
      tradeMap.TryGetValue(tradeId.ToByteArray(), out StorageItem item).Should().BeTrue();
      StackItem storedTrade = BinarySerializer.Deserialize(item.Value, engine.Limits, engine.ReferenceCounter);
      // Convert stored data into contract parameter so that they can be could compared
      ContractParameter storedOwner = ((Struct)storedTrade)[0].ToParameter();
      ContractParameter storedOfferTokenHash = ((Struct)storedTrade)[1].ToParameter();
      ContractParameter storedOfferTokenAmount = ((Struct)storedTrade)[2].ToParameter();
      ContractParameter storedOfferPackages = ((Struct)storedTrade)[3].ToParameter();
      ContractParameter storedAmountPerPackage = ((Struct)storedTrade)[4].ToParameter();
      ContractParameter storedPurchaseTokenHash = ((Struct)storedTrade)[5].ToParameter();
      ContractParameter storedPurchasePrice = ((Struct)storedTrade)[6].ToParameter();
      ContractParameter storedSoldPackages = ((Struct)storedTrade)[7].ToParameter();
      // Setup expected value
      Common.Trade expectedTrade = new()
      {
        owner = owner,
        offerTokenHash = gasHash,
        offerTokenAmount = hundredGas,
        offerPackages = offerPackages,
        amountPerPackage = hundredGas / offerPackages,
        purchaseTokenHash = gasHash,
        purchasePrice = tenGas,
        soldPackages = 0
      };
      // Compare stored value against expected value
      storedOwner.ToString().Should().Be(ContractParameterParser.ConvertObject(expectedTrade.owner).ToString());
      storedOfferTokenHash.ToString().Should().Be(ContractParameterParser.ConvertObject(expectedTrade.offerTokenHash).ToString());
      storedOfferTokenAmount.ToString().Should().Be(ContractParameterParser.ConvertObject(expectedTrade.offerTokenAmount).ToString());
      storedOfferPackages.ToString().Should().Be(ContractParameterParser.ConvertObject(expectedTrade.offerPackages).ToString());
      storedAmountPerPackage.ToString().Should().Be(ContractParameterParser.ConvertObject(expectedTrade.amountPerPackage).ToString());
      storedPurchaseTokenHash.ToString().Should().Be(ContractParameterParser.ConvertObject(expectedTrade.purchaseTokenHash).ToString());
      storedPurchasePrice.ToString().Should().Be(ContractParameterParser.ConvertObject(expectedTrade.purchasePrice).ToString());
      storedSoldPackages.ToString().Should().Be(ContractParameterParser.ConvertObject(expectedTrade.soldPackages).ToString());
    }
  }
}
