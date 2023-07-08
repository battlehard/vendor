'use client'

import AdminPage from '@/components/page-admin'
import OwnerPage from '@/components/page-owner'
import TabPanel, { ITabPage } from '@/components/tab-panel'

const pages: ITabPage[] = [
  { label: 'Owner', component: <OwnerPage /> },
  { label: 'Admin', component: <AdminPage /> },
]

export default function Home() {
  return (
    <main className="flex min-h-screen flex-col">
      <TabPanel pages={pages} />
    </main>
  )
}
