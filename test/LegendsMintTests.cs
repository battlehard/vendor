using FluentAssertions;
using Neo;
using Neo.Assertions;
using Neo.BlockchainToolkit;
using Neo.BlockchainToolkit.Models;
using Neo.BlockchainToolkit.SmartContract;
using Neo.Persistence;
using Neo.SmartContract;
using Neo.VM;
using NeoTestHarness;
using TestSwappables; // For reference contract method interface e.g. Legends
using Xunit;

namespace test
{
  // This checkpoint already add admin wallet to whitelist
  [CheckpointPath("checkpoints/legends-deployed.neoxp-checkpoint")]
  public class LegendsMintTests : IClassFixture<CheckpointFixture<LegendsMintTests>>
  {
    readonly CheckpointFixture fixture;
    readonly ExpressChain chain;

    public LegendsMintTests(CheckpointFixture<LegendsMintTests> fixture)
    {
      this.fixture = fixture;
      this.chain = fixture.FindChain();
    }

    [Fact]
    public void owner_can_mint()
    {
      ProtocolSettings settings = chain.GetProtocolSettings();
      UInt160 owner = chain.GetDefaultAccount("owner").ToScriptHash(settings.AddressVersion);

      using SnapshotCache snapshot = fixture.GetSnapshot();
      using TestApplicationEngine engine = new TestApplicationEngine(snapshot, settings, owner);
      engine.ExecuteScript<Legends>(c => c.mint("https://host.com/owner.jpg", "LegendsOwner", null));
      engine.State.Should().Be(VMState.HALT);
      engine.ResultStack.Should().HaveCount(1);
    }

    [Fact]
    public void admin_can_mint()
    {
      ProtocolSettings settings = chain.GetProtocolSettings();
      UInt160 admin = chain.GetDefaultAccount("admin").ToScriptHash(settings.AddressVersion);

      using SnapshotCache snapshot = fixture.GetSnapshot();
      using TestApplicationEngine engine = new TestApplicationEngine(snapshot, settings, admin);
      engine.ExecuteScript<Legends>(c => c.mint("https://host.com/admin.jpg", "LegendsAdmin", admin));
      engine.State.Should().Be(VMState.HALT);
      engine.ResultStack.Should().HaveCount(1);
    }

    [Fact]
    public void user_cannot_mint()
    {
      ProtocolSettings settings = chain.GetProtocolSettings();
      UInt160 user = chain.GetDefaultAccount("user").ToScriptHash(settings.AddressVersion);

      using SnapshotCache snapshot = fixture.GetSnapshot();
      using TestApplicationEngine engine = new TestApplicationEngine(snapshot, settings, user);
      engine.ExecuteScript<Legends>(c => c.mint("https://host.com/user.jpg", "LegendsUser", user));
      engine.State.Should().Be(VMState.FAULT);
      engine.UncaughtException.GetString().Should().Contain("No admin authorization");
    }

    [Fact]
    public void mint_same_name_error() // This name has been once mint using express batch.
    {
      ProtocolSettings settings = chain.GetProtocolSettings();
      UInt160 owner = chain.GetDefaultAccount("owner").ToScriptHash(settings.AddressVersion);

      using SnapshotCache snapshot = fixture.GetSnapshot();
      using TestApplicationEngine engine = new TestApplicationEngine(snapshot, settings, owner);
      engine.ExecuteScript<Legends>(c => c.mint("https://host.com/LegendsOne.jpg", Common.LEGENDS_NAME, owner));
      engine.State.Should().Be(VMState.FAULT);
      engine.UncaughtException.GetString().Should().Contain("This token already minted");
    }

    [Fact]
    public void mint_long_name_error()
    {
      ProtocolSettings settings = chain.GetProtocolSettings();
      UInt160 owner = chain.GetDefaultAccount("owner").ToScriptHash(settings.AddressVersion);

      using SnapshotCache snapshot = fixture.GetSnapshot();
      using TestApplicationEngine engine = new TestApplicationEngine(snapshot, settings, owner);
      string nameWithLength33 = "012345678901234567890123456789012345678901234567890123456789_33";
      engine.ExecuteScript<Legends>(c => c.mint("https://host.com/LegendsOne.jpg", nameWithLength33, owner));
      engine.State.Should().Be(VMState.FAULT);
      engine.UncaughtException.GetString().Should().Contain("Name must not longer than 32 characters");
    }
  }
}
