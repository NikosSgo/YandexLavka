import { CategoryList } from './CategoryList';
import { Basket } from './Basket';
import { ProductGrid } from './ProductGrid';

export function Assortment() {
  return (
    <div className="flex">
      <CategoryList className="w-1/5 border-r border-zinc-300" />
      <ProductGrid className="w-3/6 border-r border-zinc-200" />
      <Basket className="flex-1 border-l border-zinc-300" />
    </div>
  );
}


