'use client'

import { AlertColor, Button, TextField, styled } from '@mui/material'
import TabPanel, { ITabPage } from '../tab-panel'
import { useWallet } from '@/context/wallet-provider'
import { VendorContract } from '@/utils/neo/contracts/vendor'
import React, { ChangeEvent, useState } from 'react'
import Notification from '../notification'
import { NUMBER_PATTERN } from '../constant'

const InputTextField = styled(TextField)`
  width: 450px;
  margin-top: 25px;
  margin-left: 25px;
`

export default function AdminPage() {
  // Notification
  const [open, setOpen] = useState(false)
  const [severity, setSeverity] = useState<AlertColor>('success')
  const [msg, setMsg] = useState('')
  const handleClose = (
    event?: React.SyntheticEvent | Event,
    reason?: string
  ) => {
    if (reason === 'clickaway') {
      return
    }

    setOpen(false)
  }

  const pages: ITabPage[] = [
    {
      label: 'Admin Cancel Trade',
      component: AdminSubPanel(),
    },
  ]

  function AdminSubPanel() {
    const { connectedWallet, network } = useWallet()
    const [inputTradeId, setInputTradeId] = useState('')
    const [isValidTradeId, setIsValidTradeId] = useState(true)
    const handleTradeIdChange = (event: ChangeEvent<HTMLInputElement>) => {
      const value = event.target.value
      setInputTradeId(value)
      if (value.length > 0) {
        setIsValidTradeId(NUMBER_PATTERN.test(value))
      } else {
        setIsValidTradeId(false)
      }
    }

    const isInvokeDisable = (): boolean => {
      return !connectedWallet || !isValidTradeId || inputTradeId.length == 0
    }

    const invokeAdminCancelTrade = async () => {
      if (connectedWallet) {
        try {
          const txid = await new VendorContract(network).AdminCancelTrade(
            connectedWallet,
            Number(inputTradeId)
          )
          showSuccessPopup(txid)
        } catch (e: any) {
          if (e.type !== undefined) {
            showErrorPopup(`Error: ${e.type} ${e.description}`)
          }
          console.log(e)
        }
      }
    }

    const showPopup = (severity: AlertColor, message: string) => {
      setOpen(true)
      setSeverity(severity)
      setMsg(message)
    }

    const showSuccessPopup = (txid: string) => {
      showPopup('success', `Transaction submitted: txid = ${txid}`)
    }
    const showErrorPopup = (message: string) => {
      showPopup('error', message)
    }

    return (
      <div>
        <div style={{ display: 'flex', flexDirection: 'column' }}>
          <InputTextField
            required
            label="TradeId (Required)"
            defaultValue=""
            value={inputTradeId}
            onChange={handleTradeIdChange}
            error={!isValidTradeId}
          />
        </div>
        <Button
          disabled={isInvokeDisable()}
          onClick={invokeAdminCancelTrade}
          style={{ marginTop: '25px', marginLeft: '25px', alignSelf: 'start' }}
        >
          Invoke
        </Button>
        <Notification
          open={open}
          handleClose={handleClose}
          severity={severity}
          message={msg}
        />
      </div>
    )
  }

  return <TabPanel pages={pages} />
}
