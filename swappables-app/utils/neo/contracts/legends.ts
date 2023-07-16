import { MAINNET, TESTNET } from '../constant'
import {
  IConnectedWallet,
  IInvokeScriptJson,
  INetworkType,
} from '../interfaces'

import { WalletAPI } from '../wallet'
import { getTokensOf, stackJsonToObject } from '../helpers'
import { Network } from '../network'
import { wallet as NeonWallet, tx } from '@cityofzion/neon-core'

export enum AdminWhiteListAction {
  ADD = 'Add',
  REMOVE = 'Remove',
}

export const LEGENDS_SCRIPT_HASH = {
  [TESTNET]: '0xea74fb81d9fdfe3a44eed09c9435d329aa049a1f',
  [MAINNET]: '',
}

export interface ILegendsProperties {
  owner: string
  name: string
  image: string
}

export interface IMintInput {
  imageUrl: string
  name: string
  walletHash: string | null
}

export interface IBurnInput {
  tokenId: string
}

export class LegendsContract {
  network: INetworkType
  contractHash: string

  constructor(networkType: INetworkType) {
    this.network = networkType
    this.contractHash = LEGENDS_SCRIPT_HASH[networkType]
  }

  ModifyAdminWhiteList = async (
    connectedWallet: IConnectedWallet,
    adminWalletHash: string,
    action: AdminWhiteListAction
  ): Promise<string> => {
    const invokeScript: IInvokeScriptJson = {
      operation: `${action.toLowerCase()}AdminWhiteList`,
      scriptHash: this.contractHash,
      args: [
        {
          type: 'Hash160',
          value: adminWalletHash,
        },
      ],
    }

    return new WalletAPI(connectedWallet.key).invoke(
      connectedWallet.account.address,
      invokeScript
    )
  }

  Mint = async (
    connectedWallet: IConnectedWallet,
    mintData: IMintInput
  ): Promise<string> => {
    const invokeScript = this.BuildMintScript([mintData])[0]

    return new WalletAPI(connectedWallet.key).invoke(
      connectedWallet.account.address,
      invokeScript
    )
  }

  BulkMint = async (
    connectedWallet: IConnectedWallet,
    bulkMintData: IMintInput[]
  ): Promise<string> => {
    const invokeScripts = this.BuildMintScript(bulkMintData)

    return new WalletAPI(connectedWallet.key).invokeMulti(
      connectedWallet.account.address,
      invokeScripts
    )
  }

  private BuildMintScript = (
    bulkMintData: IMintInput[]
  ): IInvokeScriptJson[] => {
    const invokeScripts: IInvokeScriptJson[] = []
    for (let i = 0; i < bulkMintData.length; i++) {
      const { imageUrl, name, walletHash } = bulkMintData[i]
      const invokeScript: IInvokeScriptJson = {
        operation: 'mint',
        scriptHash: this.contractHash,
        args: [
          {
            type: 'String',
            value: imageUrl,
          },
          {
            type: 'String',
            value: name,
          },
        ],
      }
      if (walletHash) {
        invokeScript.args?.push({
          type: 'Hash160',
          value: walletHash,
        })
      } else {
        invokeScript.args?.push({
          type: 'Any',
          value: null,
        })
      }
      invokeScripts.push(invokeScript)
    }
    return invokeScripts
  }

  Burn = async (
    connectedWallet: IConnectedWallet,
    burnData: IBurnInput
  ): Promise<string> => {
    const invokeScript = this.BuildBurnScript([burnData])[0]

    return new WalletAPI(connectedWallet.key).invoke(
      connectedWallet.account.address,
      invokeScript
    )
  }

  BulkBurn = async (
    connectedWallet: IConnectedWallet,
    bulkBurnData: IBurnInput[]
  ): Promise<string> => {
    const invokeScripts = this.BuildBurnScript(bulkBurnData)

    return new WalletAPI(connectedWallet.key).invokeMulti(
      connectedWallet.account.address,
      invokeScripts
    )
  }

  private BuildBurnScript = (
    bulkBurnData: IBurnInput[]
  ): IInvokeScriptJson[] => {
    const invokeScripts: IInvokeScriptJson[] = []
    for (let i = 0; i < bulkBurnData.length; i++) {
      const { tokenId } = bulkBurnData[i]
      const invokeScript: IInvokeScriptJson = {
        operation: 'burn',
        scriptHash: this.contractHash,
        args: [
          {
            type: 'String',
            value: tokenId,
          },
        ],
      }
      invokeScripts.push(invokeScript)
    }
    return invokeScripts
  }

  Trade = async (
    connectedWallet: IConnectedWallet,
    fromTokenId: string,
    toTokenId: string
  ): Promise<string> => {
    const invokeScript: IInvokeScriptJson = {
      operation: 'trade',
      scriptHash: this.contractHash,
      args: [
        {
          type: 'String',
          value: fromTokenId,
        },
        {
          type: 'String',
          value: toTokenId,
        },
      ],
      signers: [
        {
          account: NeonWallet.getScriptHashFromAddress(
            connectedWallet.account.address
          ),
          scopes: tx.WitnessScope.CustomContracts,
          allowedContracts: [this.contractHash], // This scope allow the contract transfer NFT out from user's wallet
        },
      ],
    }

    return new WalletAPI(connectedWallet.key).invoke(
      connectedWallet.account.address,
      invokeScript
    )
  }

  getProperties = async (tokenId: string): Promise<ILegendsProperties> => {
    const script = {
      scriptHash: this.contractHash,
      operation: 'properties',
      args: [
        {
          type: 'String',
          value: tokenId,
        },
      ],
    }
    const res = await Network.read(this.network, [script])
    return stackJsonToObject(res.stack[0]) as ILegendsProperties
  }

  getTokensOf = async (ownerHash: string): Promise<ILegendsProperties[]> => {
    const tokenIdList = await this.getTokenIdsOfHash(ownerHash)

    return await this.getBatchProperties(tokenIdList)
  }

  getTokenIdsOfHash = async (ownerHash: string): Promise<string[]> => {
    const tokenIdList = await getTokensOf(
      this.network,
      this.contractHash,
      ownerHash
    )
    return tokenIdList
  }

  getBatchProperties = async (
    tokenIdList: string[]
  ): Promise<ILegendsProperties[]> => {
    const propertiesList: ILegendsProperties[] = []
    let promises = []
    // Increase efficiency by request all properties at once
    for (const tokenId of tokenIdList) {
      promises.push(this.getProperties(tokenId))
    }
    const propertiesData = await Promise.allSettled(promises)
    propertiesData.forEach((data) => {
      if (data.status === 'fulfilled') propertiesList.push(data.value)
    })
    return propertiesList
  }
}
