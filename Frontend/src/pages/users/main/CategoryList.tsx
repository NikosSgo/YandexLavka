export default function CategoryList({ className }: { className?: string }) {
  const categories =
    ["Аптека", "Зоотовары", "Новинки", "Придумано Лавкой", "Готовая Еда", "Овощной прилавок",
      "Молочный приловок", "Булочная", "Вода и напитки", "Сладкое и снеки",
      "Мясо, птица, рыба", "Заморозка", "Здоровый образ жизни", "Бакалея",
      "Для детей", "Дом, милый дом", "Красота и здороье", "Очень надо"
    ];

  const combinedClassName = `flex flex-col gap-4 p-5 ${className || ''}`.trim();

  return (
    <div className={combinedClassName}>
      <div className="text-3xl font-bold">Каталог</div>
      <div className="pl-[12px]">Вы покупали</div>
      <div className="pl-[12px]">Избранное</div>
      {
        categories.map((category: string) => (
          <div className="pl-[12px]"> {category} </div>
        ))
      }
    </div >
  );
}

