import Basket from "./basket";
import CategoryList from "./CategoryList";

export default function Assortment() {
  return (
    <div className="flex">
      <CategoryList className="w-1/5 border-r border-zinc-300 " />
      <div className="w-3/6" ></div>
      <Basket className="flex-1 border-l border-zinc-300 " />
    </div>
  )
}
