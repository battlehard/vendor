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
  ILegendsProperties,
  LEGENDS_SCRIPT_HASH,
  LegendsContract,
} from '@/utils/neo/contracts/legends'
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

export default function NftPoolPage() {
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
  const [nftList, setNftList] = useState<ILegendsProperties[]>([])
  const [openModal, setOpenModal] = useState(false)
  const handleModalOpen = (fromTokenId: string) => {
    setSelectedPoolTokenId(fromTokenId)
    setOpenModal(true)
    fetchWalletNft()
  }
  const handleModalClose = () => {
    setSelectedPoolTokenId('')
    setOpenModal(false)
  }
  const [walletLoading, setWalletLoading] = useState(true)
  const [walletNftList, setWalletNftList] = useState<ILegendsProperties[]>([])

  const fetchContractNft = async () => {
    setLoading(true)
    try {
      const result = await new LegendsContract(network).getTokensOf(
        LEGENDS_SCRIPT_HASH[network]
      )
      setNftList(result)
    } catch (e: any) {
      if (e.type !== undefined) {
        showErrorPopup(`Error: ${e.type} ${e.description}`)
      }
      console.error(e)
    }

    setLoading(false)
  }

  const fetchWalletNft = async () => {
    setWalletLoading(true)
    const walletHash = NeonWallet.getScriptHashFromAddress(
      connectedWallet?.account.address
    )
    try {
      const result = await new LegendsContract(network).getTokensOf(walletHash)
      setWalletNftList(result)
    } catch (e: any) {
      if (e.type !== undefined) {
        showErrorPopup(`Error: ${e.type} ${e.description}`)
      }
      console.error(e)
    }

    setWalletLoading(false)
  }

  useEffect(() => {
    fetchContractNft()
  }, [])

  const [selectedPoolTokenId, setSelectedPoolTokenId] = useState('')
  const handleTrade = async (walletTokenId: string) => {
    if (connectedWallet) {
      try {
        const txid = await new LegendsContract(network).Trade(
          connectedWallet,
          walletTokenId,
          selectedPoolTokenId
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
    <Box sx={{ width: '100%' }}>
      {loading && <MessagePanel message="Loading" />}
      {!loading && nftList.length == 0 && (
        <MessagePanel message="No NFT in the pool" />
      )}
      {!loading && nftList.length > 0 && (
        <Container>
          <ContainerRowForPool>
            <Div>Image</Div>
            <Div>Name</Div>
            <Div>Owner Address</Div>
            <Div>Action</Div>
          </ContainerRowForPool>
          {nftList.map((nft, index) => {
            return (
              <ContainerRowForPool key={index}>
                <img src={nft.image} alt={nft.name} width={150} />
                <Div>{nft.name}</Div>
                <Div>{nft.owner}</Div>
                <Div>
                  <Button
                    disabled={!connectedWallet}
                    variant="outlined"
                    onClick={() => {
                      handleModalOpen(nft.name)
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
            Your Swappables NFTs
          </Typography>
          {walletLoading && <MessagePanel message="Loading" />}
          {!walletLoading && walletNftList.length == 0 && (
            <MessagePanel message="No NFT in your wallet" />
          )}
          {!walletLoading && walletNftList.length > 0 && (
            <Container>
              <ContainerRowForWallet>
                <Div>Image</Div>
                <Div>Name</Div>
                <Div>Action</Div>
              </ContainerRowForWallet>
              {walletNftList.map((nft, index) => {
                return (
                  <ContainerRowForWallet key={index}>
                    <img src={nft.image} alt={nft.name} width={75} />
                    <Div>{nft.name}</Div>
                    <Div>
                      <Button
                        disabled={!connectedWallet}
                        variant="outlined"
                        onClick={() => {
                          handleTrade(nft.name)
                        }}
                      >
                        Select
                      </Button>
                    </Div>
                  </ContainerRowForWallet>
                )
              })}
            </Container>
          )}
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
