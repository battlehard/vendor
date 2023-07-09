import { MAINNET, TESTNET } from '../constant'
import {
  IConnectedWallet,
  IInvokeScriptJson,
  INetworkType,
} from '../interfaces'

import { WalletAPI } from '../wallet'
import { getTokensOf, stackJsonToObject } from '../helpers'
import { Network } from '../network'

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
    imageUrl: string,
    tokenName: string,
    walletHash: string | null
  ): Promise<string> => {
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
          value: tokenName,
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

    return new WalletAPI(connectedWallet.key).invoke(
      connectedWallet.account.address,
      invokeScript
    )
  }

  Burn = async (
    connectedWallet: IConnectedWallet,
    tokenId: string
  ): Promise<string> => {
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
