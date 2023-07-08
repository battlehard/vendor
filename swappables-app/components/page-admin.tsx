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

  return <div>Admin Page</div>
}
