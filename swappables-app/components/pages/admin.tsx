'use client'

import { AlertColor, Button, TextField, styled } from '@mui/material'
import TabPanel, { ITabPage } from '../tab-panel'
import { useWallet } from '@/context/wallet-provider'
import { LegendsContract } from '@/utils/neo/contracts/legends'
import React, { ChangeEvent, useState } from 'react'
import Notification from '../notification'
import { HASH160_PATTERN } from '../constant'

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
      label: 'Mint',
      component: AdminSubPanel(AdminMethod.MINT),
    },
    {
      label: 'Burn',
      component: AdminSubPanel(AdminMethod.BURN),
    },
  ]

  const enum AdminMethod {
    MINT = 'Mint',
    BURN = 'Burn',
  }

  function AdminSubPanel(method: AdminMethod) {
    const { connectedWallet, network } = useWallet()
    const [isValidHash, setIsValidHash] = useState(true)
    const [isValidUrl, setIsValidUrl] = useState(true)
    const [inputImageUrl, setInputImageUrl] = useState('')
    const [inputName, setInputName] = useState('')
    const [inputWalletHash, setInputWalletHash] = useState('')
    const [inputTokenId, setInputTokenId] = useState('')
    const handleImageUrlChange = (event: ChangeEvent<HTMLInputElement>) => {
      const value = event.target.value
      setInputImageUrl(value)
      if (value.length > 0) {
        setIsValidUrl(ValidateURL(value))
      } else {
        setIsValidUrl(true)
      }
    }
    const handleNameChange = (event: ChangeEvent<HTMLInputElement>) => {
      setInputName(event.target.value)
    }
    const handleWalletChange = (event: ChangeEvent<HTMLInputElement>) => {
      const value = event.target.value
      setInputWalletHash(value)
      if (value.length > 0) {
        setIsValidHash(HASH160_PATTERN.test(value))
      } else {
        setIsValidHash(true)
      }
    }
    const handleTokenIdChange = (event: ChangeEvent<HTMLInputElement>) => {
      setInputTokenId(event.target.value)
    }

    const ValidateURL = (url: string): boolean => {
      try {
        new URL(url)
        return true
      } catch (_) {
        return false
      }
    }

    const isDisable = () => {
      if (method == AdminMethod.MINT) {
        return (
          !connectedWallet ||
          !isValidUrl ||
          !isValidHash ||
          inputImageUrl.length == 0 ||
          inputName.length == 0
        )
      } else {
        return !connectedWallet || inputTokenId.length == 0
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

    const invokeMint = async () => {
      if (connectedWallet) {
        try {
          const txid = await new LegendsContract(network).Mint(
            connectedWallet,
            inputImageUrl,
            inputName,
            inputWalletHash.length == 0 ? null : inputWalletHash
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

    const invokeBurn = async () => {
      if (connectedWallet) {
        try {
          const txid = await new LegendsContract(network).Burn(
            connectedWallet,
            inputTokenId
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

    return (
      <div style={{ display: 'flex', flexDirection: 'column' }}>
        {method == AdminMethod.MINT && (
          <div style={{ display: 'flex', flexDirection: 'column' }}>
            <InputTextField
              required
              label="Image URL (Required)"
              defaultValue=""
              value={inputImageUrl}
              onChange={handleImageUrlChange}
              error={!isValidUrl}
            />
            <InputTextField
              required
              label="Token name (Required)"
              helperText="Token name no more than 32 characters"
              defaultValue=""
              value={inputName}
              onChange={handleNameChange}
              inputProps={{ maxLength: 32 }}
            />
            <InputTextField
              label="Wallet Hash (Optional)"
              helperText={
                isValidHash
                  ? 'Mint destination: Wallet addess in Hash160 format. Leave blank to store in contract'
                  : 'Invalid hash'
              }
              defaultValue=""
              value={inputWalletHash}
              onChange={handleWalletChange}
              error={!isValidHash}
            />
          </div>
        )}
        {method == AdminMethod.BURN && (
          <InputTextField
            required
            label="Token ID (Required)"
            helperText="Token ID is the same as token name"
            defaultValue=""
            value={inputTokenId}
            onChange={handleTokenIdChange}
            inputProps={{ maxLength: 32 }}
          />
        )}
        <Button
          disabled={isDisable()}
          onClick={method == AdminMethod.MINT ? invokeMint : invokeBurn}
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
