import Basket from "./basket/index.tsx"
import CategoryList from "./CategoryList.tsx"
import Footer from "./footer/index.tsx"
import Header from "./header/index.tsx"

export default function UserPage() {
  return (
    <>
      <Header />
      <div className="flex">
        <CategoryList className="w-1/5 border-r border-zinc-300 " />
        <div className="w-3/6" ></div>
        <Basket className="flex-1 border-l border-zinc-300 " />
      </div>
      <Footer />
    </>
  )
}

