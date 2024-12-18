'use client'

import { AlertColor, Button, TextField } from '@mui/material'
import TabPanel, { ITabPage } from '../tab-panel'
import { useWallet } from '@/context/wallet-provider'
import { WhiteListAction, VendorContract } from '@/utils/neo/contracts/vendor'
import React, { ChangeEvent, useState } from 'react'
import Notification from '../notification'
import { HASH160_PATTERN } from '../constant'

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
      label: 'Add admin white list',
      component: AdminWhiteList(WhiteListAction.ADD),
    },
    {
      label: 'Remove admin white list',
      component: AdminWhiteList(WhiteListAction.REMOVE),
    },
  ]

  function AdminWhiteList(adminWhiteListAction: WhiteListAction) {
    const { connectedWallet, network } = useWallet()
    console.log("Use Wallet Context:", useWallet())
    const [inputWalletHash, setInputWalletHash] = useState('')
    const [isValidHash, setIsValidHash] = useState(true)
    const isDisable = () => {
      return !connectedWallet || !isValidHash || inputWalletHash.length == 0
    }

    const handleWalletHashChange = (event: ChangeEvent<HTMLInputElement>) => {
      const value = event.target.value
      setInputWalletHash(value)
      if (value.length > 0) {
        setIsValidHash(HASH160_PATTERN.test(value))
      } else {
        setIsValidHash(true)
      }
    }

    const showPopup = (severity: AlertColor, message: string) => {
      setOpen(true)
      setSeverity(severity)
      setMsg(message)
    }

    const invoke = async () => {
      if (connectedWallet) {
        try {
          const txid = await new VendorContract(network).ModifyAdminWhiteList(
            connectedWallet,
            inputWalletHash,
            adminWhiteListAction
          )
          showPopup('success', `Transaction submitted: txid = ${txid}`)
        } catch (e: any) {
          if (e.type !== undefined) {
            showPopup('error', `${network}Error: ${e.type} ${e.description}`)
          }
          console.log(e)
        }
      }
    }

    return (
      <div style={{ display: 'flex', flexDirection: 'column' }}>
        <TextField
          required
          sx={{
            width: '450px',
            marginTop: '25px',
            marginLeft: '25px',
            '& .MuiInputBase-input': {
              color: 'rgba(255, 255, 255, 0.87)', // Input text color
            },
            '& .MuiInputLabel-root': {
              color: 'rgba(255, 255, 255, 0.6)', // Label color
            },
            '& .MuiFormHelperText-root': {
              color: 'rgba(255, 255, 255, 0.6)', // Helper text color
            },
            '& .MuiOutlinedInput-root': {
              '& fieldset': {
                borderColor: 'rgba(255, 255, 255, 0.6)', // Border color
              },
              '&:hover fieldset': {
                borderColor: '#90caf9', // Hover border color
              },
              '&.Mui-focused fieldset': {
                borderColor: '#90caf9', // Focus border color
              },
            },
          }}
          label="Wallet Hash (Required)"
          helperText={
            isValidHash
              ? 'Admin wallet in Hash160 format start in 0x'
              : 'Invalid hash'
          }
          defaultValue=""
          value={inputWalletHash}
          onChange={handleWalletHashChange}
          error={!isValidHash}
          inputProps={{ maxLength: 42 }}
        />
        <Button
          disabled={isDisable()}
          onClick={invoke}
          sx={{
            marginTop: '25px',
            marginLeft: '25px',
            alignSelf: 'start',
            color: 'rgba(255, 255, 255, 0.87)', // Button text color
            backgroundColor: 'rgba(144, 202, 249, 0.2)', // Button background color
            '&:hover': {
              backgroundColor: '#90caf9', // Hover background
            },
            '&.Mui-disabled': {
              color: 'rgba(255, 255, 255, 0.3)', // Disabled text color
              backgroundColor: 'rgba(255, 255, 255, 0.1)', // Disabled background
            },
          }}
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
