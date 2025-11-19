import { type FormEvent, useState } from 'react';
import { observer } from 'mobx-react-lite';
import { useAuthStore } from '@app/providers/AppStoreProvider';

interface LoginFormProps {
  onSuccess?: () => void;
}

export const LoginForm = observer(({ onSuccess }: LoginFormProps) => {
  const authStore = useAuthStore();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [localError, setLocalError] = useState<string | null>(null);

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setLocalError(null);

    try {
      await authStore.login({ email, password });
      onSuccess?.();
    } catch (error) {
      const message =
        error instanceof Error ? error.message : 'Ошибка входа. Проверьте email и пароль.';
      setLocalError(message);
    }
  };

  return (
    <form className="mt-8 space-y-6" onSubmit={handleSubmit}>
      {localError && (
        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
          {localError}
        </div>
      )}
      <div className="space-y-4">
        <div>
          <label htmlFor="email" className="block text-sm font-medium text-gray-700">
            Email
          </label>
          <input
            id="email"
            name="email"
            type="email"
            required
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
            placeholder="user@example.com"
          />
        </div>
        <div>
          <label htmlFor="password" className="block text-sm font-medium text-gray-700">
            Пароль
          </label>
          <input
            id="password"
            name="password"
            type="password"
            required
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
            placeholder="••••••••"
          />
        </div>
      </div>
      <button
        type="submit"
        disabled={authStore.isLoading}
        className="w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-50"
      >
        {authStore.isLoading ? 'Вход...' : 'Войти'}
      </button>
    </form>
  );
});


