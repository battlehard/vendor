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
using System;
using System.Collections.Generic;
using TestSwappables; // For reference contract method interface e.g. Legends
using Xunit;

namespace test
{
  // This checkpoint already add admin wallet to whitelist
  [CheckpointPath("checkpoints/legends-deployed.neoxp-checkpoint")]
  public class LegendsBurnTests : IClassFixture<CheckpointFixture<LegendsBurnTests>>
  {
    readonly CheckpointFixture fixture;
    readonly ExpressChain chain;

    public LegendsBurnTests(CheckpointFixture<LegendsBurnTests> fixture)
    {
      this.fixture = fixture;
      this.chain = fixture.FindChain();
    }

    [Fact]
    public void owner_can_burn()
    {
      ProtocolSettings settings = chain.GetProtocolSettings();
      UInt160 owner = chain.GetDefaultAccount("owner").ToScriptHash(settings.AddressVersion);

      using SnapshotCache snapshot = fixture.GetSnapshot();
      using TestApplicationEngine engine = new TestApplicationEngine(snapshot, settings, owner);
      engine.ExecuteScript<Legends>(c => c.burn(Common.LEGENDS_NAME));
      engine.State.Should().Be(VMState.HALT);
      engine.ResultStack.Should().HaveCount(1);
      IReadOnlyDictionary<ReadOnlyMemory<byte>, StorageItem> storages = snapshot.GetContractStorages<Legends>();
      storages.TryGetValue(Neo.Helper.Concat(Common.Prefix_Trade_Pool, Neo.Utility.StrictUTF8.GetBytes(Common.LEGENDS_NAME)), out StorageItem item).Should().BeFalse();
    }

    [Fact]
    public void admin_can_burn()
    {
      ProtocolSettings settings = chain.GetProtocolSettings();
      UInt160 admin = chain.GetDefaultAccount("admin").ToScriptHash(settings.AddressVersion);

      using SnapshotCache snapshot = fixture.GetSnapshot();
      using TestApplicationEngine engine = new TestApplicationEngine(snapshot, settings, admin);
      engine.ExecuteScript<Legends>(c => c.burn(Common.LEGENDS_NAME));
      engine.State.Should().Be(VMState.HALT);
      engine.ResultStack.Should().HaveCount(1);
      IReadOnlyDictionary<ReadOnlyMemory<byte>, StorageItem> storages = snapshot.GetContractStorages<Legends>();
      storages.TryGetValue(Neo.Helper.Concat(Common.Prefix_Trade_Pool, Neo.Utility.StrictUTF8.GetBytes(Common.LEGENDS_NAME)), out StorageItem item).Should().BeFalse();
    }

    [Fact]
    public void user_cannot_burn()
    {
      ProtocolSettings settings = chain.GetProtocolSettings();
      UInt160 user = chain.GetDefaultAccount("user").ToScriptHash(settings.AddressVersion);

      using SnapshotCache snapshot = fixture.GetSnapshot();
      using TestApplicationEngine engine = new TestApplicationEngine(snapshot, settings, user);
      engine.ExecuteScript<Legends>(c => c.burn(Common.LEGENDS_NAME));
      engine.State.Should().Be(VMState.FAULT);
      engine.UncaughtException.GetString().Should().Contain("No admin authorization");
    }

    [Fact]
    public void cannot_burn_unexisting()
    {
      ProtocolSettings settings = chain.GetProtocolSettings();
      UInt160 owner = chain.GetDefaultAccount("owner").ToScriptHash(settings.AddressVersion);

      using SnapshotCache snapshot = fixture.GetSnapshot();
      using TestApplicationEngine engine = new TestApplicationEngine(snapshot, settings, owner);
      engine.ExecuteScript<Legends>(c => c.burn("DeletedLegends"));
      engine.State.Should().Be(VMState.FAULT);
      engine.UncaughtException.GetString().Should().Contain("This token not existing");
    }
  }
}
