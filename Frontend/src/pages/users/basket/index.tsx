export default function Basket({ className }: { className?: string }) {
  const combinedClassName = `gap-4 p-5 text-3xl font-bold ${className || ''}`.trim();
  return (
    <div className={combinedClassName}>
      Корзина
    </div>
  )
}
