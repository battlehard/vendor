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
using TestVendor; // For reference contract method interface e.g. Vendor
using Xunit;

namespace test
{
  // This checkpoint already add admin wallet to whitelist
  [CheckpointPath("checkpoints/vendor-deployed.neoxp-checkpoint")]
  public class VendorDeployTests : IClassFixture<CheckpointFixture<VendorDeployTests>>
  {
    readonly CheckpointFixture fixture;
    readonly ExpressChain chain;

    public VendorDeployTests(CheckpointFixture<VendorDeployTests> fixture)
    {
      this.fixture = fixture;
      this.chain = fixture.FindChain();
    }

    [Fact]
    public void Contract_owner_in_storage()
    {
      ProtocolSettings settings = chain.GetProtocolSettings();
      UInt160 owner = chain.GetDefaultAccount("owner").ToScriptHash(settings.AddressVersion);

      using SnapshotCache snapshot = fixture.GetSnapshot();
      // check to make sure contract owner stored in contract storage
      IReadOnlyDictionary<ReadOnlyMemory<byte>, StorageItem> storages = snapshot.GetContractStorages<Vendor>();
      storages.TryGetValue(Common.Prefix_Owner, out var item).Should().BeTrue();
      item!.Should().Be(owner);
    }

  }
}
