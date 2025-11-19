interface LoaderProps {
  label?: string;
}

export function Loader({ label = 'Загрузка...' }: LoaderProps) {
  return (
    <div className="flex items-center gap-2 text-slate-600">
      <span className="inline-block h-3 w-3 animate-ping rounded-full bg-indigo-500" />
      {label}
    </div>
  );
}


