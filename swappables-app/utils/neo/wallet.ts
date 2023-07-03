import { Argument, BaseDapi, Dapi } from '@neongd/neo-dapi'
import { Provider } from '@neongd/neo-provider'
import {
  IBalance,
  IInvokeScriptJson,
  INetworkType,
  ITransaction,
  IWalletType,
} from './interfaces'
import {
  GAS_SCRIPT_HASH,
  NEO_LINE,
  NEO_SCRIPT_HASH,
  ONEGATE,
  WALLET_LIST,
} from './constant'
import { wallet as NeonWallet, u } from '@cityofzion/neon-core'
import moment from 'moment'
import { Signer, WitnessScope } from '@cityofzion/neon-core/lib/tx'

export class WalletAPI {
  walletType: IWalletType

  constructor(walletType: IWalletType) {
    this.walletType = walletType
  }

  static list = WALLET_LIST

  private getInstance = async (walletType: IWalletType): Promise<any> => {
    let instance: any
    switch (walletType) {
      case NEO_LINE:
        // @ts-ignore
        instance = new NEOLineN3.Init()
        break
      case ONEGATE:
        instance = await this.getOneGateInstance()
        break
      default:
        throw new Error('No support wallet!')
    }
    return instance
  }

  private NeoLine = async () => {
    // @ts-ignore
    const instance = new NEOLineN3.Init()
    // @ts-ignore
    // NEOLineN3 doesn't have getNetworks function
    const instance2 = new NEOLine.Init()
    const network = await instance2.getNetworks()
    const provider = await instance.getProvider()
    const account = await instance.getAccount()
    return { provider, account, network, balances: {} }
  }

  private getOneGateInstance(): Dapi {
    // Check available provider following NGD demo app.
    let provider = null
    // @ts-ignore
    if (window.neo) provider = window.neo as Provider
    // @ts-ignore
    else if (window.OneGate) provider = window.OneGate as Provider

    if (process.env.IS_DEBUG)
      console.log(`Window Provider Type: ${JSON.stringify(provider)}`)

    if (provider) {
      const instance = new BaseDapi(provider)
      return instance
    } else {
      throw {
        type: 'NO_PROVIDER',
        description: 'OneGate dapi provider not found.',
      }
    }
  }

  private OneGate = async () => {
    const instance = this.getOneGateInstance()
    const provider = await instance.getProvider()
    const account = await instance.getAccount()
    const network = await instance.getNetworks()
    const balances: IBalance[] = []
    const walletBalance = await instance.getNep17Balances({
      address: account.address,
      network: network.defaultNetwork,
    })
    walletBalance.forEach((item: any) => {
      let symbol = ''
      let amount = ''
      // TODO: refactor when possible
      // assetHash from @neongd/neo-dapi and assethash from @cityofzion/neon-core are different case (H/h).
      if (item.assetHash.includes(GAS_SCRIPT_HASH)) {
        symbol = 'GAS'
        amount = u.BigInteger.fromNumber(item.amount).toDecimal(8).toString()
      } else if (item.assetHash.includes(NEO_SCRIPT_HASH)) {
        symbol = 'NEO'
        amount = item.amount // NEO is indivisible
      } else {
        // Any non-support tokens will be skipped.
        return
      }
      balances.push({
        contract: item.assetHash,
        amount,
        symbol,
      })
    })

    if (process.env.IS_DEBUG) {
      console.log(`Provider: ${JSON.stringify(provider)}`)
      console.log(`Account: ${JSON.stringify(account)}`)
      console.log(`Network: ${JSON.stringify(network)}`)
      console.log(`Balances: ${JSON.stringify(balances)}`)
    }
    return { provider, account, network, balances }
  }

  init = async (): Promise<any> => {
    let wallet
    try {
      switch (this.walletType) {
        case NEO_LINE:
          wallet = await this.NeoLine()
          break
        case ONEGATE:
          wallet = await this.OneGate()
          break
      }
      return {
        key: this.walletType,
        ...wallet,
      }
    } catch (e) {
      console.error(e)
      throw e
    }
  }

  /* Control signing and send transaction. TODO:Need to improve type hardcoding later */
  invoke = async (
    network: INetworkType,
    senderAddress: string,
    invokeScript: IInvokeScriptJson | IInvokeScriptJson[]
  ): Promise<string> => {
    const isMulti = Array.isArray(invokeScript)
    const wallet = await this.getInstance(this.walletType)
    let res
    let signers: Signer[] = [
      {
        //@ts-ignore
        account: NeonWallet.getScriptHashFromAddress(senderAddress),
        scopes: WitnessScope.CalledByEntry,
      },
    ]
    if (this.walletType === ONEGATE) {
      if (!isMulti) {
        if (!invokeScript.signers) invokeScript.signers = signers
        // @ts-ignore
        invokeScript.args = this.buildOneGateArgs(invokeScript.args)
        invokeScript.signers = this.buildStringScopes(invokeScript.signers)
        if (process.env.IS_DEBUG) console.log(JSON.stringify(invokeScript))
        res = await wallet.invoke(invokeScript)
      } else {
        const invocations = invokeScript.map((script) => {
          if (script.signers) signers = script.signers
          // @ts-ignore
          const args = this.buildOneGateArgs(script.args)
          return {
            scriptHash: script.scriptHash,
            operation: script.operation,
            args,
          }
        })
        signers = this.buildStringScopes(signers)
        if (process.env.IS_DEBUG)
          console.log(JSON.stringify({ invocations, signers }))
        res = await wallet.invokeMulti({ invocations, signers })
      }
    } else {
      if (!isMulti) {
        if (!invokeScript.signers) invokeScript.signers = signers
        if (process.env.IS_DEBUG) console.log(JSON.stringify(invokeScript))
        res = await wallet.invoke(invokeScript)
      } else {
        const invokeArgs = invokeScript.map((script) => {
          if (script.signers) signers = script.signers
          return {
            scriptHash: script.scriptHash,
            operation: script.operation,
            args: script.args,
          }
        })
        console.log(JSON.stringify({ invokeArgs, signers }))
        res = await wallet.invokeMultiple({ invokeArgs, signers })
      }
    }
    // explicitly declare txid to support NEON
    const txid = res.txid
    if (process.env.IS_DEBUG) console.log(`txid: ${txid}`)
    if (!isMulti) {
      this.buildSubmittedTx(network, txid, invokeScript)
    } else {
      invokeScript.forEach((script) => {
        this.buildSubmittedTx(network, txid, script)
      })
    }
    return txid
  }

  private buildOneGateArgs = (args: Argument[]): Argument[] => {
    // OneGate not support Address type, need to convert to Hash160
    return args.map((param: any) => {
      if (param.type === 'Address') {
        return {
          type: 'Hash160',
          value: NeonWallet.getScriptHashFromAddress(param.value),
        }
      } else {
        return param
      }
    })
  }

  private buildStringScopes = (signers: Signer[]): any[] => {
    return signers.map((signer: any) => {
      switch (signer.scopes) {
        case WitnessScope.None:
          signer.scopes = 'None'
          break
        case WitnessScope.CalledByEntry:
          signer.scopes = 'CalledByEntry'
          break
        case WitnessScope.CustomContracts:
          signer.scopes = 'CustomContracts'
          break
        case WitnessScope.CustomGroups:
          signer.scopes = 'CustomGroups'
          break
        case WitnessScope.Global:
          signer.scopes = 'Global'
          break
        default:
          return signer
      }
      return signer
    })
  }

  private buildSubmittedTx = (
    network: INetworkType,
    txid: string,
    script: IInvokeScriptJson
  ): ITransaction => {
    return {
      network,
      wallet: this.walletType,
      status: 'PENDING',
      txid,
      contractHash: script.scriptHash,
      method: script.operation,
      args: script.args,
      createdAt: moment().format('MMMM Do YYYY, h:mm:ss a'),
    }
  }
}
