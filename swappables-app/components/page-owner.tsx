'use client'

import { AlertColor, Button, TextField } from '@mui/material'
import TabPanel, { ITabPage } from './tab-panel'
import { useWallet } from '@/context/wallet-provider'
import {
  AdminWhiteListAction,
  LegendsContract,
} from '@/utils/neo/contracts/legends'
import React, { ChangeEvent, useState } from 'react'
import Notification from './notification'

export default function OwnerPage() {
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
      label: 'Add Admin White List',
      component: AdminWhiteList(AdminWhiteListAction.ADD),
    },
    {
      label: 'Remove Admin White List',
      component: AdminWhiteList(AdminWhiteListAction.REMOVE),
    },
  ]

  function AdminWhiteList(adminWhiteListAction: AdminWhiteListAction) {
    const { connectedWallet, network } = useWallet()
    const [inputWalletHash, setInputWalletHash] = useState('')

    const handleInputChange = (event: ChangeEvent<HTMLInputElement>) => {
      setInputWalletHash(event.target.value)
    }

    const showPopup = (severity: AlertColor, message: string) => {
      setOpen(true)
      setSeverity(severity)
      setMsg(message)
    }

    const invoke = async () => {
      if (connectedWallet) {
        try {
          const txid = await new LegendsContract(network).ModifyAdminWhiteList(
            connectedWallet,
            inputWalletHash,
            adminWhiteListAction
          )
          showPopup('success', `Transaction submitted: txid = ${txid}`)
        } catch (e: any) {
          if (e.type !== undefined) {
            showPopup('error', `Error: ${e.type} ${e.description}`)
          }
          console.log(e)
        }
      }
    }

    return (
      <div style={{ display: 'flex' }}>
        <TextField
          required
          style={{
            width: '450px',
            marginTop: '25px',
            marginLeft: '25px',
          }}
          label="Wallet Hash (Required)"
          helperText="Admin Wallet Addess in Hash160 format"
          defaultValue=""
          value={inputWalletHash}
          onChange={handleInputChange}
        />
        <Button disabled={!connectedWallet} onClick={invoke}>
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
