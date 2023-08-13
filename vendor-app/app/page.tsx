'use client'

import AdminPage from '@/components/pages/admin'
import TradePoolPage from '@/components/pages/trade-pool'
import OwnerPage from '@/components/pages/owner'
import TabPanel, { ITabPage } from '@/components/tab-panel'

const pages: ITabPage[] = [
  { label: 'Owner', component: <OwnerPage /> },
  { label: 'Admin', component: <AdminPage /> },
  { label: 'Trade Pool', component: <TradePoolPage /> },
]

export default function Home() {
  return (
    <main className="flex min-h-screen flex-col">
      <TabPanel pages={pages} />
    </main>
  )
}
