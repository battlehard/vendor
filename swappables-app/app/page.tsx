'use client'

import AdminPage from '@/components/pages/admin'
import NftPoolPage from '@/components/pages/nft-pool'
import OwnerPage from '@/components/pages/owner'
import TabPanel, { ITabPage } from '@/components/tab-panel'

const pages: ITabPage[] = [
  { label: 'Owner', component: <OwnerPage /> },
  { label: 'Admin', component: <AdminPage /> },
  { label: 'NFT Pool', component: <NftPoolPage /> },
]

export default function Home() {
  return (
    <main className="flex min-h-screen flex-col">
      <TabPanel pages={pages} />
    </main>
  )
}
