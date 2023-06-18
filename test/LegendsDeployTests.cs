using FluentAssertions;
using Neo;
using Neo.Assertions;
using Neo.BlockchainToolkit;
using Neo.BlockchainToolkit.Models;
using Neo.BlockchainToolkit.SmartContract;
using Neo.Persistence;
using Neo.SmartContract;
using Neo.VM;
using Neo.VM.Types;
using NeoTestHarness;
using System;
using System.Collections.Generic;
using TestSwappables; // For reference contract method interface e.g. Legends
using Xunit;

namespace test
{
  // This checkpoint already add admin wallet to whitelist
  [CheckpointPath("checkpoints/legends-deployed.neoxp-checkpoint")]
  public class LegendsDeployTests : IClassFixture<CheckpointFixture<LegendsDeployTests>>
  {
    readonly CheckpointFixture fixture;
    readonly ExpressChain chain;

    public LegendsDeployTests(CheckpointFixture<LegendsDeployTests> fixture)
    {
      this.fixture = fixture;
      this.chain = fixture.FindChain();
    }

    [Fact]
    public void contract_owner_in_storage()
    {
      ProtocolSettings settings = chain.GetProtocolSettings();
      UInt160 owner = chain.GetDefaultAccount("owner").ToScriptHash(settings.AddressVersion);

      using SnapshotCache snapshot = fixture.GetSnapshot();
      // check to make sure contract owner stored in contract storage
      IReadOnlyDictionary<ReadOnlyMemory<byte>, StorageItem> storages = snapshot.GetContractStorages<Legends>();
      storages.TryGetValue(Common.Prefix_Owner, out var item).Should().BeTrue();
      item!.Should().Be(owner);
    }

    [Fact]
    public void can_get_correct_properties()
    {
      ProtocolSettings settings = chain.GetProtocolSettings();
      UInt160 owner = chain.GetDefaultAccount("owner").ToScriptHash(settings.AddressVersion);

      using SnapshotCache snapshot = fixture.GetSnapshot();
      using TestApplicationEngine engine = new TestApplicationEngine(snapshot, settings, owner);
      engine.ExecuteScript<Legends>(c => c.properties(Neo.Utility.StrictUTF8.GetBytes(Common.LEGENDS_NAME)));
      engine.State.Should().Be(VMState.HALT);
      engine.ResultStack.Should().HaveCount(1);
      StackItem legendsState = engine.ResultStack.Pop();
      Common.GetStringValueFromMapKey(legendsState, "name").Should().Be(Common.LEGENDS_NAME);
    }


  }
}
