import { MAINNET, TESTNET } from '../constant'
import {
  IConnectedWallet,
  IInvokeScriptJson,
  INetworkType,
} from '../interfaces'

import { wallet as NeonWallet } from '@cityofzion/neon-core'
import { WalletAPI } from '../wallet'

export enum AdminWhiteListAction {
  ADD = 'Add',
  REMOVE = 'Remove',
}

export const LEGENDS_SCRIPT_HASH = {
  [TESTNET]: '0xea74fb81d9fdfe3a44eed09c9435d329aa049a1f',
  [MAINNET]: '',
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
    const senderHash = NeonWallet.getScriptHashFromAddress(
      connectedWallet.account.address
    )
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
}
