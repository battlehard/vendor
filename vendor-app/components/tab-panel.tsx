'use client'

import { useState } from 'react'
import { Box, Tab, Tabs } from '@mui/material'

export interface ITabPage {
  label: string
  component: JSX.Element
}

interface Props {
  pages: ITabPage[]
}

export default function TabPanel({ pages }: Props) {
  const [value, setValue] = useState(0)

  const handleChange = (event: React.SyntheticEvent, newValue: number) => {
    setValue(newValue)
  }

  return (
    <Box sx={{ width: '100%' }}>
      <Tabs
        value={value}
        onChange={handleChange}
        aria-label="Tab component"
        sx={{
          '& .MuiTabs-indicator': {
            backgroundColor: '#90caf9', // Accent color for the active tab indicator
          },
        }}
      >
        {pages.map((page, index) => (
          <Tab
            key={index}
            label={page.label}
            sx={{
              textTransform: 'none',
              color: 'rgba(255, 255, 255, 0.6)', // Default text color for inactive tabs
              '&.Mui-selected': {
                color: '#90caf9', // Active tab text color
              },
              '&:hover': {
                color: '#b3e5fc', // Hover color
              },
            }}
          />
        ))}
      </Tabs>
      <Box sx={{ mt: 2, color: 'rgba(255, 255, 255, 0.87)' }}>
        {pages[value].component}
      </Box>
    </Box>
  )
}
