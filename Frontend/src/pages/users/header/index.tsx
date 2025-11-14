export default function Header() {
  return (
    <div className="flex justify-between h-20 border-b-2 shadow-sm p-[15px]">
      <div className="flex gap-[15px] items-center">
        <div>Лавка</div>
        <div>Поиск</div>
        <div>Адреса</div>
      </div>
      <div className="flex items-center">
        Аккаунт
      </div>
    </div>
  );
}

