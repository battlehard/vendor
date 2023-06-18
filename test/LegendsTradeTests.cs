using FluentAssertions;
using Neo;
using Neo.Assertions;
using Neo.BlockchainToolkit;
using Neo.BlockchainToolkit.Models;
using Neo.BlockchainToolkit.SmartContract;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.SmartContract;
using Neo.VM;
using NeoTestHarness;
using TestSwappables; // For reference contract method interface e.g. Legends
using Xunit;

namespace test
{
  [CheckpointPath("checkpoints/legends-minted-to-wallet.neoxp-checkpoint")]
  public class LegendsTradeTests : IClassFixture<CheckpointFixture<LegendsTradeTests>>
  {
    readonly CheckpointFixture fixture;
    readonly ExpressChain chain;

    public LegendsTradeTests(CheckpointFixture<LegendsTradeTests> fixture)
    {
      this.fixture = fixture;
      this.chain = fixture.FindChain();
    }

    [Fact]
    public void owner_can_trade_token()
    {
      ProtocolSettings settings = chain.GetProtocolSettings();
      UInt160 owner = chain.GetDefaultAccount("owner").ToScriptHash(settings.AddressVersion);

      using SnapshotCache snapshot = fixture.GetSnapshot();
      // Use global scope for test because there is no document about how to set custom contract scope
      using TestApplicationEngine engine = new TestApplicationEngine(snapshot, settings, owner, WitnessScope.Global);
      engine.ExecuteScript<Legends>(c => c.trade(Common.LEGENDS_OWNER, Common.LEGENDS_ONE));
      engine.State.Should().Be(VMState.HALT);
      engine.ResultStack.Should().HaveCount(1);
      engine.Notifications.Should().HaveCount(3); // 2 transfer events and 1 trade event
    }

    [Fact]
    public void admin_can_trade_token()
    {
      ProtocolSettings settings = chain.GetProtocolSettings();
      UInt160 admin = chain.GetDefaultAccount("admin").ToScriptHash(settings.AddressVersion);

      using SnapshotCache snapshot = fixture.GetSnapshot();
      using TestApplicationEngine engine = new TestApplicationEngine(snapshot, settings, admin, WitnessScope.Global);
      engine.ExecuteScript<Legends>(c => c.trade(Common.LEGENDS_ADMIN, Common.LEGENDS_ONE));
      engine.State.Should().Be(VMState.HALT);
      engine.ResultStack.Should().HaveCount(1);
      engine.Notifications.Should().HaveCount(3);
    }

    [Fact]
    public void user_can_trade_token()
    {
      ProtocolSettings settings = chain.GetProtocolSettings();
      UInt160 user = chain.GetDefaultAccount("user").ToScriptHash(settings.AddressVersion);

      using SnapshotCache snapshot = fixture.GetSnapshot();
      using TestApplicationEngine engine = new TestApplicationEngine(snapshot, settings, user, WitnessScope.Global);
      engine.ExecuteScript<Legends>(c => c.trade(Common.LEGENDS_USER, Common.LEGENDS_ONE));
      engine.State.Should().Be(VMState.HALT);
      engine.ResultStack.Should().HaveCount(1);
      engine.Notifications.Should().HaveCount(3);
    }

    [Fact]
    public void admin_cannot_trade_user_token()
    {
      ProtocolSettings settings = chain.GetProtocolSettings();
      UInt160 admin = chain.GetDefaultAccount("admin").ToScriptHash(settings.AddressVersion);

      using SnapshotCache snapshot = fixture.GetSnapshot();
      using TestApplicationEngine engine = new TestApplicationEngine(snapshot, settings, admin);
      engine.ExecuteScript<Legends>(c => c.trade(Common.LEGENDS_USER, Common.LEGENDS_ONE));
      engine.State.Should().Be(VMState.FAULT);
      engine.UncaughtException.GetString().Should().Contain("No NFT ownership");
    }

    [Fact]
    public void admin_cannot_trade_not_exist_token()
    {
      ProtocolSettings settings = chain.GetProtocolSettings();
      UInt160 admin = chain.GetDefaultAccount("admin").ToScriptHash(settings.AddressVersion);

      using SnapshotCache snapshot = fixture.GetSnapshot();
      using TestApplicationEngine engine = new TestApplicationEngine(snapshot, settings, admin);
      engine.ExecuteScript<Legends>(c => c.trade(Common.LEGENDS_ADMIN, Common.LEGENDS_NOT_EXIST));
      engine.State.Should().Be(VMState.FAULT);
      engine.UncaughtException.GetString().Should().Contain("This token not existing");
    }

    [Fact]
    public void admin_cannot_trade_unavailable_token()
    {
      ProtocolSettings settings = chain.GetProtocolSettings();
      UInt160 admin = chain.GetDefaultAccount("admin").ToScriptHash(settings.AddressVersion);

      using SnapshotCache snapshot = fixture.GetSnapshot();
      using TestApplicationEngine engine = new TestApplicationEngine(snapshot, settings, admin);
      engine.ExecuteScript<Legends>(c => c.trade(Common.LEGENDS_ADMIN, Common.LEGENDS_OWNER));
      engine.State.Should().Be(VMState.FAULT);
      engine.UncaughtException.GetString().Should().Contain("LegendsOwner in not available in the pool");
    }
  }
}
