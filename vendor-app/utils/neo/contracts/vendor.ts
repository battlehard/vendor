import { MAINNET, TESTNET } from '../constant'
import {
  IConnectedWallet,
  IInvokeScriptJson,
  INetworkType,
} from '../interfaces'

import { WalletAPI } from '../wallet'
import { stackJsonToObject } from '../helpers'
import { Network } from '../network'
import { wallet as NeonWallet, tx } from '@cityofzion/neon-core'

export enum AdminWhiteListAction {
  ADD = 'Add',
  REMOVE = 'Remove',
}

export const VENDOR_SCRIPT_HASH = {
  [TESTNET]: '0x2792f6c37e165bbe7ca2a210689dc73136446e2c',
  [MAINNET]: '',
}

export interface ITradeProperties {
  id: number
  owner: string
  offerTokenHash: string
  offerTokenAmount: number
  offerPackages: number
  amountPerPackage: number
  purchaseTokenHash: string
  purchasePrice: number
  soldPackages: number
}

export class VendorContract {
  network: INetworkType
  contractHash: string

  constructor(networkType: INetworkType) {
    this.network = networkType
    this.contractHash = VENDOR_SCRIPT_HASH[networkType]
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

  AdminCancelTrade = async (
    connectedWallet: IConnectedWallet,
    tradeId: number
  ): Promise<string> => {
    const invokeScript: IInvokeScriptJson = {
      operation: 'adminCancelTrade',
      scriptHash: this.contractHash,
      args: [
        {
          type: 'Integer',
          value: tradeId,
        },
      ],
    }

    return new WalletAPI(connectedWallet.key).invoke(
      connectedWallet.account.address,
      invokeScript
    )
  }

  CreateTrade = async (
    connectedWallet: IConnectedWallet,
    offerTokenHash: string,
    offerTokenAmount: number,
    offerPackages: number,
    purchaseTokenHash: string,
    purchasePrice: number
  ): Promise<string> => {
    const invokeScript: IInvokeScriptJson = {
      operation: 'createTrade',
      scriptHash: this.contractHash,
      args: [
        {
          type: 'Hash160',
          value: offerTokenHash,
        },
        {
          type: 'Integer',
          value: offerTokenAmount,
        },
        {
          type: 'Integer',
          value: offerPackages,
        },
        {
          type: 'Hash160',
          value: purchaseTokenHash,
        },
        {
          type: 'Integer',
          value: purchasePrice,
        },
      ],
      signers: [
        {
          account: NeonWallet.getScriptHashFromAddress(
            connectedWallet.account.address
          ),
          scopes: tx.WitnessScope.CustomContracts,
          allowedContracts: [this.contractHash], // This scope allow the contract transfer tokens out from user's wallet
        },
      ],
    }

    return new WalletAPI(connectedWallet.key).invoke(
      connectedWallet.account.address,
      invokeScript
    )
  }
}
