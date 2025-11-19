interface CategoryListProps {
  className?: string;
}

const categories = [
  'Фрукты и овощи',
  'Молочные продукты',
  'Напитки',
  'Бакалея',
  'Здоровое питание',
];

export function CategoryList({ className }: CategoryListProps) {
  return (
    <aside className={`p-4 space-y-2 ${className ?? ''}`}>
      <h3 className="text-lg font-semibold mb-3">Категории</h3>
      <ul className="space-y-2 text-sm">
        {categories.map((category) => (
          <li
            key={category}
            className="py-2 border-b border-zinc-200 last:border-b-0 cursor-pointer hover:text-indigo-600"
          >
            {category}
          </li>
        ))}
      </ul>
    </aside>
  );
}


