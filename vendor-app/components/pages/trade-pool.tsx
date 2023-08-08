'use client'

import {
  AlertColor,
  Box,
  Button,
  Modal,
  Typography,
  styled,
} from '@mui/material'
import {
  ITradeProperties,
  VENDOR_SCRIPT_HASH,
  VendorContract,
} from '@/utils/neo/contracts/vendor'
import React, { useEffect, useState } from 'react'
import { useWallet } from '@/context/wallet-provider'
import { wallet as NeonWallet } from '@cityofzion/neon-core'
import Notification from '../notification'

const Container = styled(Box)`
  max-width: 900px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
`

const ContainerRowForPool = styled(Box)`
  display: grid;
  grid-template-columns: 200px 0.5fr 1fr 0.25fr;
  justify-items: center;
  margin-bottom: 10px;
  overflow-wrap: anywhere;
`

const ContainerRowForWallet = styled(Box)`
  display: grid;
  grid-template-columns: 1fr 2fr 1fr;
  justify-items: center;
  margin-bottom: 10px;
  overflow-wrap: anywhere;
`

const Div = styled('div')(({ theme }) => ({
  ...theme.typography.button,
  padding: theme.spacing(1),
  textTransform: 'none',
}))

const modalStyle = {
  position: 'absolute' as 'absolute',
  top: '50%',
  left: '50%',
  transform: 'translate(-50%, -50%)',
  width: 'auto',
  bgcolor: 'background.paper',
  border: '2px solid #000',
  boxShadow: 24,
  p: 4,
}

interface MessagePanelProps {
  message: string
}
const MessagePanel = ({ message }: MessagePanelProps) => {
  return (
    <Container>
      <Div style={{ textAlign: 'center' }}>{message}</Div>
    </Container>
  )
}

export default function TradePoolPage() {
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

  const { connectedWallet, network } = useWallet()
  const [loading, setLoading] = useState(true)
  const [tradeList, setTradeList] = useState<ITradeProperties[]>([])
  const [openModal, setOpenModal] = useState(false)
  const handleModalOpen = () => {
    setOpenModal(true)
  }
  const handleModalClose = () => {
    setOpenModal(false)
  }

  const fetchListTrade = async () => {
    setLoading(true)
    // try {
    //   const result = await new VendorContract(network).ListTrade(
    //     VENDOR_SCRIPT_HASH[network]
    //   )
    //   setTradeList(result)
    // } catch (e: any) {
    //   if (e.type !== undefined) {
    //     showErrorPopup(`Error: ${e.type} ${e.description}`)
    //   }
    //   console.error(e)
    // }

    setLoading(false)
  }

  useEffect(() => {
    fetchListTrade()
  }, [])

  const handleCreateTrade = async () => {
    if (connectedWallet) {
      try {
        // const txid = await new VendorContract(network).CreateTrade(
        //   connectedWallet,
        // )
        // showSuccessPopup(txid)
      } catch (e: any) {
        if (e.type !== undefined) {
          showErrorPopup(`Error: ${e.type} ${e.description}`)
        }
        console.log(e)
      }
    }
  }

  const handleExecuteTrade = async (tradeId: number) => {
    if (connectedWallet) {
      try {
        // const txid = await new VendorContract(network).ExecuteTrade(
        //   connectedWallet,
        // )
        // showSuccessPopup(txid)
      } catch (e: any) {
        if (e.type !== undefined) {
          showErrorPopup(`Error: ${e.type} ${e.description}`)
        }
        console.log(e)
      }
    }
  }

  return (
    <Box sx={{ width: '100%' }}>
      <Container>
        <Button
          disabled={!connectedWallet}
          variant="outlined"
          onClick={handleModalOpen}
        >
          Create Trade
        </Button>
      </Container>
      {loading && <MessagePanel message="Loading" />}
      {!loading && tradeList.length == 0 && (
        <MessagePanel message="No Trade in the pool" />
      )}
      {!loading && tradeList.length > 0 && (
        <Container>
          <Button
            disabled={!connectedWallet}
            variant="outlined"
            onClick={handleModalOpen}
          >
            Create Trade
          </Button>
          <ContainerRowForPool>
            <Div>Trade ID</Div>
            <Div>Owner Address</Div>
            <Div>Offer Token Hash</Div>
            <Div>Amount Per Package</Div>
            <Div>Offer Packages</Div>
            <Div>Sold Packages</Div>
            <Div>Purchase Token Hash</Div>
            <Div>Purchase Price</Div>
          </ContainerRowForPool>
          {tradeList.map((trade, index) => {
            return (
              <ContainerRowForPool key={index}>
                <Div>{trade.id}</Div>
                <Div>{trade.owner}</Div>
                <Div>{trade.offerTokenHash}</Div>
                <Div>{trade.amountPerPackage}</Div>
                <Div>{trade.offerPackages}</Div>
                <Div>{trade.soldPackages}</Div>
                <Div>{trade.purchaseTokenHash}</Div>
                <Div>{trade.purchasePrice}</Div>
                <Div>
                  <Button
                    disabled={!connectedWallet}
                    variant="outlined"
                    onClick={() => {
                      handleExecuteTrade(trade.id)
                    }}
                  >
                    Trade
                  </Button>
                </Div>
              </ContainerRowForPool>
            )
          })}
        </Container>
      )}
      <Modal
        open={openModal}
        onClose={handleModalClose}
        aria-labelledby="modal-modal-title"
      >
        <Box sx={modalStyle}>
          <Typography id="modal-modal-title" variant="h6" component="h2">
            Trade Creation
          </Typography>
          <Container>
            <Button
              disabled={!connectedWallet}
              variant="outlined"
              onClick={() => {
                handleCreateTrade()
              }}
            >
              Invoke
            </Button>
          </Container>
        </Box>
      </Modal>
      <Notification
        open={open}
        handleClose={handleClose}
        severity={severity}
        message={msg}
      />
    </Box>
  )
}
