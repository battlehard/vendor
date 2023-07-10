'use client'

import { Box, styled } from '@mui/material'
import {
  ILegendsProperties,
  LEGENDS_SCRIPT_HASH,
  LegendsContract,
} from '@/utils/neo/contracts/legends'
import React, { useEffect, useState } from 'react'
import { useWallet } from '@/context/wallet-provider'

const Container = styled(Box)`
  max-width: 800px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
`

const ContainerRow = styled(Box)`
  display: grid;
  grid-template-columns: 200px 1fr 1fr;
  justify-items: center;
  margin-bottom: 10px;
`

const Div = styled('div')(({ theme }) => ({
  ...theme.typography.button,
  padding: theme.spacing(1),
  textTransform: 'none',
}))

export default function NftPoolPage() {
  const { network } = useWallet()
  const [loading, setLoading] = useState(true)
  const [nftList, setNftList] = useState<ILegendsProperties[]>([])

  const fetchNft = async () => {
    setLoading(true)
    try {
      const result = await new LegendsContract(network).getTokensOf(
        LEGENDS_SCRIPT_HASH[network]
      )
      setNftList(result)
    } catch (e: any) {
      console.error(e)
    }

    setLoading(false)
  }

  useEffect(() => {
    fetchNft()
  }, [])

  return (
    <Box sx={{ width: '100%' }}>
      {loading && (
        <Container>
          <Div style={{ textAlign: 'center' }}>Loading</Div>
        </Container>
      )}
      {!loading && nftList.length == 0 && (
        <Container>
          <Div style={{ textAlign: 'center' }}>No NFT in the pool</Div>
        </Container>
      )}
      {!loading && nftList.length > 0 && (
        <Container>
          <ContainerRow>
            <Div>Image</Div>
            <Div>Name</Div>
            <Div>Owner Address</Div>
          </ContainerRow>
          {nftList.map((nft, index) => {
            return (
              <ContainerRow key={index}>
                <img src={nft.image} alt={nft.name} width={150} />
                <Div>{nft.name}</Div>
                <Div>{nft.owner}</Div>
              </ContainerRow>
            )
          })}
        </Container>
      )}
    </Box>
  )
}
