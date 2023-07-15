'use client'

import {
  AlertColor,
  Button,
  Chip,
  Divider,
  TextField,
  styled,
} from '@mui/material'
import TabPanel, { ITabPage } from '../tab-panel'
import { useWallet } from '@/context/wallet-provider'
import {
  IBurnInput,
  IMintInput,
  LegendsContract,
} from '@/utils/neo/contracts/legends'
import React, { ChangeEvent, useState } from 'react'
import Notification from '../notification'
import { HASH160_PATTERN } from '../constant'

interface IDataError {
  index: number
  causes: string
}

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
    const [isValidJson, setIsValidJson] = useState(true)
    const [isBulkOp, setIsBulkOp] = useState(false)
    const [inputImageUrl, setInputImageUrl] = useState('')
    const [inputName, setInputName] = useState('')
    const [inputWalletHash, setInputWalletHash] = useState('')
    const [inputTokenId, setInputTokenId] = useState('')
    const [selectedFile, setSelectedFile] = useState('')
    const [inputBulkMint, setInputBulkMint] = useState<
      IMintInput[] | undefined
    >(undefined)
    const [inputBulkBurn, setInputBulkBurn] = useState<
      IBurnInput[] | undefined
    >(undefined)
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
    const handleFileChange = (event: ChangeEvent<HTMLInputElement>) => {
      const files = event.target.files
      if (files?.length && files.length > 0) {
        setIsBulkOp(true)
        setSelectedFile(files[0].name)
        const fileReader = new FileReader()
        fileReader.readAsText(files[0], 'UTF-8')
        if (method == AdminMethod.MINT) {
          fileReader.onload = (e: any) => {
            const bulkMintData = JSON.parse(e.target.result) as IMintInput[]
            checkBulkMintData(bulkMintData)
          }
        } else if (method == AdminMethod.BURN) {
          fileReader.onload = (e: any) => {
            const bulkBurnData = JSON.parse(e.target.result) as IBurnInput[]
            checkBulkBurnData(bulkBurnData)
          }
        }
      } else {
        setIsBulkOp(false)
        setIsValidJson(true)
        setSelectedFile('')
      }
    }

    const ValidateURL = (url: string): boolean => {
      try {
        new URL(url)
        return true
      } catch (_) {
        return false
      }
    }

    const checkBulkMintData = (bulkMintData: IMintInput[]) => {
      const errorList: IDataError[] = []
      for (let i = 0; i < bulkMintData.length; i++) {
        let validated = true
        let errorMsg = ''
        const { imageUrl, name, walletHash } = bulkMintData[i]
        // Validate URL only case that have value
        if (imageUrl && imageUrl.length > 0) {
          validated = ValidateURL(imageUrl)
          if (!validated) errorMsg += 'imageUrl,'
        } else {
          validated = false
          errorMsg += 'imageUrl,'
        }

        // Name must have value, and length between 1-32
        if (!(name && name.length > 0 && name.length <= 32)) {
          validated = false
          errorMsg += 'name,'
        }

        // Validate hash only case that have value
        // No validation need when hash = null because this parameter is optional
        if (walletHash != null && walletHash.length >= 0) {
          validated = HASH160_PATTERN.test(walletHash)
          if (!validated) errorMsg += 'walletHash,'
        } else if (walletHash != null) {
          validated = false
          errorMsg += 'walletHash,'
        }

        if (!validated) {
          const errorObj: IDataError = {
            index: i,
            causes: errorMsg.substring(0, errorMsg.length - 1), // remove trailing comma
          }
          errorList.push(errorObj)
        }
      }

      if (errorList.length == 0) {
        if (process.env.NEXT_PUBLIC_IS_DEBUG) console.log(bulkMintData)
        setInputBulkMint(bulkMintData)
        setIsValidJson(true)
      } else {
        setIsValidJson(false)
        showErrorPopup('index and causes: ' + JSON.stringify(errorList))
      }
    }

    const checkBulkBurnData = (bulkBurnData: IBurnInput[]) => {
      const errorList: IDataError[] = []
      for (let i = 0; i < bulkBurnData.length; i++) {
        let validated = true
        let errorMsg = ''
        const { tokenId } = bulkBurnData[i]
        // TokenId must have value, and length between 1-32
        if (!(tokenId && tokenId.length > 0 && tokenId.length <= 32)) {
          validated = false
          errorMsg += 'tokenId'
          const errorObj: IDataError = {
            index: i,
            causes: errorMsg,
          }
          errorList.push(errorObj)
        }
      }

      if (errorList.length == 0) {
        if (process.env.NEXT_PUBLIC_IS_DEBUG) console.log(bulkBurnData)
        setInputBulkBurn(bulkBurnData)
        setIsValidJson(true)
      } else {
        setIsValidJson(false)
        showErrorPopup('index and causes: ' + JSON.stringify(errorList))
      }
    }

    const isInvokeDisable = () => {
      if (isBulkOp) {
        return !connectedWallet || selectedFile.length == 0 || !isValidJson
      } else {
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
          let txid = ''
          if (isBulkOp && inputBulkMint) {
            txid = await new LegendsContract(network).BulkMint(
              connectedWallet,
              inputBulkMint
            )
          } else {
            txid = await new LegendsContract(network).Mint(connectedWallet, {
              imageUrl: inputImageUrl,
              name: inputName,
              walletHash: inputWalletHash.length == 0 ? null : inputWalletHash,
            } as IMintInput)
          }
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
          let txid = ''
          if (isBulkOp && inputBulkBurn) {
            txid = await new LegendsContract(network).BulkBurn(
              connectedWallet,
              inputBulkBurn
            )
          } else {
            txid = await new LegendsContract(network).Burn(connectedWallet, {
              tokenId: inputTokenId,
            } as IBurnInput)
          }
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
        <div style={{ display: 'flex', flexDirection: 'row' }}>
          <div style={{ marginTop: 25, marginLeft: 25 }}>
            <input
              id="file-upload"
              type="file"
              accept=".json"
              style={{ display: 'none' }}
              onChange={handleFileChange}
            />
            <label htmlFor="file-upload">
              <Button variant="contained" component="span">
                Upload JSON
              </Button>
            </label>
          </div>
          <TextField
            style={{ marginTop: 25, marginLeft: 25, minWidth: 300 }}
            disabled
            variant="standard"
            value={selectedFile}
          />
        </div>
        {!isBulkOp && (
          <Divider style={{ marginTop: 25 }}>
            <Chip label="OR" />
          </Divider>
        )}
        {method == AdminMethod.MINT && !isBulkOp && (
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
        {method == AdminMethod.BURN && !isBulkOp && (
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
          disabled={isInvokeDisable()}
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
