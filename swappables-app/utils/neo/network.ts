import { rpc, sc, u, wallet } from '@cityofzion/neon-core'
import { MAINNET, MAINNET_CONFIG, TESTNET, TESTNET_CONFIG } from './constant'
import { InvokeResult, Query } from '@cityofzion/neon-core/lib/rpc'
import { MAX_RPC_RESULT } from './constant'
import { INetworkType } from './interfaces'

export class Network {
  static getRPCClient = (networkType: INetworkType) => {
    let config
    switch (networkType) {
      case TESTNET:
        config = TESTNET_CONFIG
        break
      case MAINNET:
        config = MAINNET_CONFIG
        break
    }
    return new rpc.RPCClient(config.url)
  }

  static read = async (
    network: INetworkType,
    scripts: sc.ContractCallJson[]
  ): Promise<InvokeResult> => {
    const rpcClient = Network.getRPCClient(network)
    const sb = new sc.ScriptBuilder()
    scripts.map((script) => {
      let params: unknown[] = []
      if (script.args) {
        params = script.args.map((arg) => Network.convertContractCallParam(arg))
      }
      sb.emitAppCall(script.scriptHash, script.operation, params)
    })
    return await rpcClient.invokeScript(u.HexString.fromHex(sb.build()))
  }

  // traverseIterator is using only for get result of tokensOf (it return iterator)
  // therefore, return result as string[] which contain tokenId
  static traverseIterator = async (
    network: INetworkType,
    sessionId: string,
    id: string
  ): Promise<string[]> => {
    const rpcClient = Network.getRPCClient(network)
    const traverseIteratorQuery = {
      method: 'traverseiterator',
      params: [sessionId, id, MAX_RPC_RESULT],
    }
    let rpcRes: any[] = []
    try {
      // neon-core 5.2.0 don't have support for traverseiterator.
      // used execute with custom method instead
      rpcRes = await rpcClient.execute(new Query(traverseIteratorQuery))
    } catch (e: any) {
      // in case that node support lower than MAX_RPC_RESULT, it will throw error.
      // set count to default of 100 then re-execute.
      if (e.message.includes('count')) {
        traverseIteratorQuery.params[2] = 100
        rpcRes = await rpcClient.execute(new Query(traverseIteratorQuery))
      }
    }

    const tokenIdList: string[] = []
    if (rpcRes.length > 0) {
      // @ts-ignore
      for (const item of rpcRes) {
        const tokenId = u.HexString.fromBase64(item.value as string).toAscii()
        tokenIdList.push(tokenId)
      }
    }
    return tokenIdList
  }

  static queryTraverseIterator = async (
    network: INetworkType,
    sessionId: string,
    id: string
  ): Promise<any[]> => {
    const rpcClient = Network.getRPCClient(network)
    const traverseIteratorQuery = {
      method: 'traverseiterator',
      params: [sessionId, id, MAX_RPC_RESULT],
    }
    let rpcRes: any[] = []
    try {
      rpcRes = await rpcClient.execute(new Query(traverseIteratorQuery))
    } catch (e: any) {
      if (e.message.includes('count')) {
        traverseIteratorQuery.params[2] = 100
        rpcRes = await rpcClient.execute(new Query(traverseIteratorQuery))
      }
    }

    return rpcRes
  }

  private static convertContractCallParam = (param: any) => {
    switch (param.type) {
      case 'Address':
        return sc.ContractParam.hash160(
          wallet.getScriptHashFromAddress(param.value)
        )
      case 'Hash160':
        return sc.ContractParam.hash160(param.value)
      case 'String':
        return sc.ContractParam.string(param.value)
      case 'Integer':
        return sc.ContractParam.integer(param.value)
      case 'Array':
        return sc.ContractParam.array(
          ...param.value.map((i: any) => Network.convertContractCallParam(i))
        )
      case 'ByteArray':
        return sc.ContractParam.byteArray(
          u.hex2base64(u.str2hexstring(param.value))
        )
      default:
        throw new Error('No support param')
    }
  }
}
