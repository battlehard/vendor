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
    readonly ProtocolSettings settings;
    readonly UInt160 owner;
    readonly UInt160 user;



    public VendorTradeTests(CheckpointFixture<VendorTradeTests> fixture)
    {
      this.fixture = fixture;
      this.chain = fixture.FindChain();
      this.settings = chain.GetProtocolSettings();
      this.owner = chain.GetDefaultAccount("owner").ToScriptHash(settings.AddressVersion);
      this.user = chain.GetDefaultAccount("user").ToScriptHash(settings.AddressVersion);
    }

    [Fact]
    public void Create_trade()
    {
      using SnapshotCache snapshot = fixture.GetSnapshot();
      // Set WitnessScope as Global here to only test login, not permission.
      using TestApplicationEngine engine = new(snapshot, settings, owner, WitnessScope.Global);

      Common.Trade expectedTrade = CreateTrade(engine, owner);
      engine.State.Should().Be(VMState.HALT);
      engine.ResultStack.Should().HaveCount(1);
      var storages = snapshot.GetContractStorages<Vendor>();
      var tradeMap = storages.StorageMap(Common.Prefix_Trade_Pool);
      tradeMap.TryGetValue(Common.TEST_TRADE_ID.ToByteArray(), out StorageItem item).Should().BeTrue();
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

    [Fact]
    public void Execute_not_found_trade()
    {
      using SnapshotCache snapshot = fixture.GetSnapshot();
      // Set WitnessScope as Global here to only test login, not permission.
      using TestApplicationEngine engine = new(snapshot, settings, user, WitnessScope.Global);

      engine.ExecuteScript<Vendor>(c => c.executeTrade(Common.TEST_TRADE_ID, Common.TEST_PURCHASE_PACKAGES));
      engine.State.Should().Be(VMState.FAULT);
      engine.UncaughtException.GetString().Should().Contain($"Cannot find the tradeId: {Common.TEST_TRADE_ID}");
    }

    [Fact]
    public void Execute_insufficient_trade()
    {
      using SnapshotCache snapshot = fixture.GetSnapshot();
      // Set WitnessScope as Global here to only test login, not permission.
      using TestApplicationEngine engineStep1 = new(snapshot, settings, owner, WitnessScope.Global);
      CreateTrade(engineStep1, owner);
      engineStep1.State.Should().Be(VMState.HALT);

      using TestApplicationEngine engineStep2 = new(snapshot, settings, user, WitnessScope.Global);
      engineStep2.ExecuteScript<Vendor>(c => c.executeTrade(Common.TEST_TRADE_ID, Common.TEST_OVER_PURCHASE_PACKAGES));
      engineStep2.State.Should().Be(VMState.FAULT);
      engineStep2.UncaughtException.GetString().Should().Contain($"Insufficient packages: {Common.TEST_OVER_PURCHASE_PACKAGES} purchasing BUT {Common.TEST_OFFER_PACKAGES} available");
    }

    [Fact]
    public void Execute_success_trade()
    {
      using SnapshotCache snapshot = fixture.GetSnapshot();
      // Set WitnessScope as Global here to only test login, not permission.
      using TestApplicationEngine engineStep1 = new(snapshot, settings, owner, WitnessScope.Global);
      CreateTrade(engineStep1, owner);
      engineStep1.State.Should().Be(VMState.HALT);
      engineStep1.ResultStack.Should().HaveCount(1);

      using TestApplicationEngine engineStep2 = new(snapshot, settings, user, WitnessScope.Global);
      engineStep2.ExecuteScript<Vendor>(c => c.executeTrade(Common.TEST_TRADE_ID, Common.TEST_PURCHASE_PACKAGES));
      engineStep2.State.Should().Be(VMState.HALT);
      engineStep2.ResultStack.Should().HaveCount(1);
      var storages = snapshot.GetContractStorages<Vendor>();
      var tradeMap = storages.StorageMap(Common.Prefix_Trade_Pool);
      tradeMap.TryGetValue(Common.TEST_TRADE_ID.ToByteArray(), out StorageItem item).Should().BeTrue();
      StackItem storedTrade = BinarySerializer.Deserialize(item.Value, engineStep2.Limits, engineStep2.ReferenceCounter);
      // Convert stored data into contract parameter so that they can be could compared
      ContractParameter storedSoldPackages = ((Struct)storedTrade)[7].ToParameter();
      // Setup expected value after execute trade, purchase 5 sold should be 5.
      Common.Trade expectedTrade = new()
      {
        soldPackages = Common.TEST_PURCHASE_PACKAGES
      };
      // Compare stored value against expected value
      storedSoldPackages.ToString().Should().Be(ContractParameterParser.ConvertObject(expectedTrade.soldPackages).ToString());
    }

    [Fact]
    public void Cancel_trade_with_no_permission()
    {
      using SnapshotCache snapshot = fixture.GetSnapshot();
      // Set WitnessScope as Global here to only test login, not permission.
      using TestApplicationEngine engineStep1 = new(snapshot, settings, user, WitnessScope.Global);
      CreateTrade(engineStep1, user);
      engineStep1.State.Should().Be(VMState.HALT);
      engineStep1.ResultStack.Should().HaveCount(1);

      using TestApplicationEngine engineStep2 = new(snapshot, settings, owner, WitnessScope.Global);
      engineStep2.ExecuteScript<Vendor>(c => c.cancelTrade(Common.TEST_TRADE_ID));
      engineStep2.State.Should().Be(VMState.FAULT);
      engineStep2.UncaughtException.GetString().Should().Contain($"No permission to cancel trade");
    }

    [Fact]
    public void Cancel_user_trade_with_admin()
    {
      using SnapshotCache snapshot = fixture.GetSnapshot();
      // Set WitnessScope as Global here to only test login, not permission.
      using TestApplicationEngine engineStep1 = new(snapshot, settings, user, WitnessScope.Global);
      CreateTrade(engineStep1, user);
      engineStep1.State.Should().Be(VMState.HALT);
      engineStep1.ResultStack.Should().HaveCount(1);

      using TestApplicationEngine engineStep2 = new(snapshot, settings, owner, WitnessScope.Global);
      engineStep2.ExecuteScript<Vendor>(c => c.adminCancelTrade(Common.TEST_TRADE_ID));
      engineStep2.State.Should().Be(VMState.HALT);
    }

    [Fact]
    public void Cancel_trade_success()
    {
      using SnapshotCache snapshot = fixture.GetSnapshot();
      // Set WitnessScope as Global here to only test login, not permission.
      using TestApplicationEngine engineStep1 = new(snapshot, settings, user, WitnessScope.Global);
      CreateTrade(engineStep1, user);
      engineStep1.State.Should().Be(VMState.HALT);
      engineStep1.ResultStack.Should().HaveCount(1);

      using TestApplicationEngine engineStep2 = new(snapshot, settings, user, WitnessScope.Global);
      engineStep2.ExecuteScript<Vendor>(c => c.cancelTrade(Common.TEST_TRADE_ID));
      engineStep2.State.Should().Be(VMState.HALT);
      engineStep1.ResultStack.Should().HaveCount(1);
    }

    [Fact]
    public void List_page_with_failed_cases()
    {
      using SnapshotCache snapshot = fixture.GetSnapshot();
      TestApplicationEngine engineStep1 = new(snapshot, settings, user, WitnessScope.Global);
      BigInteger tradeQuantity = Common.MAX_PAGE_LIMIT + 1;
      for (BigInteger i = 0; i < tradeQuantity; i++)
      {
        engineStep1 = new(snapshot, settings, user, WitnessScope.Global);
        CreateTrade(engineStep1, user);
      }

      // Check created trade quantity
      var storages = snapshot.GetContractStorages<Vendor>();
      storages.TryGetValue(Common.Prefix_Trade_Count, out StorageItem tradeCount).Should().BeTrue();
      tradeCount.Should().Be(tradeQuantity);

      using TestApplicationEngine engineStep2 = new(snapshot, settings, user, WitnessScope.Global);
      engineStep2.ExecuteScript<Vendor>(c => c.listTrade(1, tradeQuantity)); // List more than max page limit.
      engineStep2.State.Should().Be(VMState.FAULT);
      engineStep2.UncaughtException.GetString().Should().Contain($"Input page limit exceed the max limit of {Common.MAX_PAGE_LIMIT}");

      using TestApplicationEngine engineStep3 = new(snapshot, settings, user, WitnessScope.Global);
      engineStep3.ExecuteScript<Vendor>(c => c.listTrade(0, tradeQuantity)); // List from page 0
      engineStep3.State.Should().Be(VMState.FAULT);
      engineStep3.UncaughtException.GetString().Should().Contain("Pagination data must be provided, pageNumber and pageSize must have at least 1");

      using TestApplicationEngine engineStep4 = new(snapshot, settings, user, WitnessScope.Global);
      engineStep4.ExecuteScript<Vendor>(c => c.listTrade(Common.MAX_PAGE_LIMIT, Common.MAX_PAGE_LIMIT)); // List page over the total pages
      engineStep4.State.Should().Be(VMState.FAULT);
      engineStep4.UncaughtException.GetString().Should().Contain($"Input page number exceed the totalPages of 2");
    }

    // Create Trade and return expected trade object
    private static Common.Trade CreateTrade(TestApplicationEngine engine, UInt160 tradeCreator)
    {
      UInt160 gasHash = NativeContract.GAS.Hash;
      BigInteger hundredGas = Common.HUNDRED_GAS;
      BigInteger tenGas = Common.TEN_GAS;
      BigInteger offerPackages = Common.TEST_OFFER_PACKAGES;
      engine.ExecuteScript<Vendor>(c => c.createTrade(gasHash, hundredGas, offerPackages, gasHash, tenGas));

      return new()
      {
        owner = tradeCreator,
        offerTokenHash = gasHash,
        offerTokenAmount = hundredGas,
        offerPackages = offerPackages,
        amountPerPackage = hundredGas / offerPackages,
        purchaseTokenHash = gasHash,
        purchasePrice = tenGas,
        soldPackages = 0
      };
    }
  }
}
