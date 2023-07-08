'use client'

import { createContext, useContext, useEffect, useState } from 'react'
import {
  IConnectedWallet,
  IWalletOptions,
  IWalletStates,
  IWalletType,
} from '@/utils/neo/interfaces'
import { sc } from '@cityofzion/neon-core'
import { WalletAPI } from '@/utils/neo/wallet'
import { NEO_LINE } from '@/utils/neo/constant'

export const WalletContext = createContext({} as IWalletStates)

export default function WalletProvider(props: {
  options: IWalletOptions
  children: any
}) {
  const [network] = useState(props.options.network)

  const [isWalletModalActive, setWalletModalActive] = useState(false)
  const [isNeoLineAvailable, setNeoLineAvailability] = useState(false)
  const [isOneGateAvailable, setOneGateAvailability] = useState(false)

  const [connectedWallet, setConnectedWallet] = useState<
    IConnectedWallet | undefined
  >(undefined)

  const [invokeScript, setInvokeScript] = useState<
    sc.ContractCallJson | undefined
  >()

  const openWalletModal = () => {
    setWalletModalActive(true)
    // Check wallet provider availability during open wallet modal
    checkNeoLineAvailability()
    checkOneGateAvailability()
  }

  const closeWalletModal = () => setWalletModalActive(false)

  const connectWallet = async (walletType: IWalletType) => {
    try {
      const connectedWallet = await new WalletAPI(walletType).init()
      setConnectedWallet(connectedWallet)
      setWalletModalActive(false)
      console.log(`Wallet connected successfully`)
    } catch (e) {
      setWalletModalActive(false)
      console.error(e)
    }
  }

  const disConnectWallet = () => {
    setConnectedWallet(undefined)
  }

  const doInvoke = (invokeScript: sc.ContractCallJson) => {
    if (isWalletModalActive) setWalletModalActive(false)
    setInvokeScript(invokeScript)
  }

  const closeInvoke = () => setInvokeScript(undefined)

  const contextValue: IWalletStates = {
    network,
    invokeScript,
    list: WalletAPI.list,
    connectedWallet,
    isWalletModalActive,
    openWalletModal,
    closeWalletModal,
    connectWallet,
    disConnectWallet,
    doInvoke,
    closeInvoke,
    isNeoLineAvailable,
    isOneGateAvailable,
  }

  useEffect(() => {
    if (connectedWallet) {
      const refresh = () => {
        connectWallet(connectedWallet.key)
      }
      const disconnected = () => {
        disConnectWallet()
      }

      if (connectedWallet.key === NEO_LINE) {
        window.addEventListener('NEOLine.NEO.EVENT.ACCOUNT_CHANGED', refresh)
        window.addEventListener('NEOLine.NEO.EVENT.NETWORK_CHANGED', refresh)
        window.addEventListener('NEOLine.NEO.EVENT.DISCONNECTED', disconnected)
        return () => {
          window.removeEventListener(
            'NEOLine.NEO.EVENT.ACCOUNT_CHANGED',
            refresh
          )
          window.removeEventListener(
            'NEOLine.NEO.EVENT.NETWORK_CHANGED',
            refresh
          )
          window.removeEventListener(
            'NEOLine.NEO.EVENT.DISCONNECTED',
            disconnected
          )
        }
      }
    }
  }, [])

  // Check wallet provider availability
  function checkNeoLineAvailability() {
    try {
      // @ts-ignore
      const neoLineInstance = new NEOLineN3.Init()
      setNeoLineAvailability(true)
      if (process.env.IS_DEBUG) console.log('NeoLine AVAILABLE')
    } catch (e) {
      setNeoLineAvailability(false)
      if (process.env.IS_DEBUG) console.log('NeoLine UN-AVAILABLE')
    }
  }

  function checkOneGateAvailability() {
    // @ts-ignore
    if (window.OneGate && !window.NeoLineMobile) {
      setOneGateAvailability(true)
      if (process.env.IS_DEBUG)
        console.log('OneGate AVAILABLE, Neon UN-AVAILABLE')
    } else {
      setOneGateAvailability(false)
      if (process.env.IS_DEBUG) console.log('OneGate UN-AVAILABLE')
    }
  }

  return (
    <WalletContext.Provider value={contextValue}>
      {props.children}
    </WalletContext.Provider>
  )
}

export const useWallet = () => useContext(WalletContext)
