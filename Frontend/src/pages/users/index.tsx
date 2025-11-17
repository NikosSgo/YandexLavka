import { Outlet } from "react-router-dom"
import Footer from "./footer/index.tsx"
import Header from "./header/index.tsx"

export default function UserPage() {
  return (
    <>
      <Header />
      <Outlet />
      <Footer />
    </>
  )
}

