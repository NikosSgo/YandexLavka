import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../../hooks/useAuth';
import { useUser } from '../../../hooks/useUser';

export default function CreateProfilePage() {
  const { account, isLoading: authLoading } = useAuth();
  const { createProfile } = useUser();
  const navigate = useNavigate();

  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    phone: '',
    email: '',
  });
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  // Обновляем email когда account загружается
  useEffect(() => {
    if (account?.email) {
      const email = account.email.trim();
      if (email) {
        setFormData(prev => ({ ...prev, email }));
      }
    }
  }, [account?.email]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    // Проверка наличия email - сначала из formData, потом из account
    let email = formData.email?.trim();

    if (!email && account?.email) {
      email = account.email.trim();
      setFormData(prev => ({ ...prev, email }));
    }

    if (!email) {
      setError('Email не найден. Пожалуйста, обновите страницу или войдите в систему заново.');
      return;
    }

    // Проверка всех обязательных полей
    if (!formData.firstName.trim() || !formData.lastName.trim() || !formData.phone.trim()) {
      setError('Пожалуйста, заполните все обязательные поля.');
      return;
    }

    setIsLoading(true);

    try {
      // Убеждаемся, что email передается
      const profileData = {
        firstName: formData.firstName.trim(),
        lastName: formData.lastName.trim(),
        phone: formData.phone.trim(),
        email: email,
      };

      await createProfile(profileData);
      navigate('/profile');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Ошибка создания профиля');
    } finally {
      setIsLoading(false);
    }
  };

  // Показываем загрузку если account еще не загружен
  if (authLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-lg">Загрузка...</div>
      </div>
    );
  }

  // Проверяем, что пользователь аутентифицирован
  if (!account) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <div className="text-lg text-gray-600 mb-4">Вы не авторизованы</div>
          <button
            onClick={() => navigate('/login')}
            className="px-4 py-2 bg-indigo-600 text-white rounded hover:bg-indigo-700"
          >
            Войти
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-2xl mx-auto px-4">
        <div className="bg-white rounded-lg shadow-md p-6">
          <h1 className="text-2xl font-bold mb-6">Создание профиля</h1>

          <form onSubmit={handleSubmit} className="space-y-4">
            {error && (
              <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
                {error}
              </div>
            )}

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label htmlFor="firstName" className="block text-sm font-medium text-gray-700">
                  Имя *
                </label>
                <input
                  id="firstName"
                  type="text"
                  required
                  value={formData.firstName}
                  onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
                  className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                />
              </div>

              <div>
                <label htmlFor="lastName" className="block text-sm font-medium text-gray-700">
                  Фамилия *
                </label>
                <input
                  id="lastName"
                  type="text"
                  required
                  value={formData.lastName}
                  onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
                  className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                />
              </div>
            </div>

            <div>
              <label htmlFor="phone" className="block text-sm font-medium text-gray-700">
                Телефон *
              </label>
              <input
                id="phone"
                type="tel"
                required
                value={formData.phone}
                onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
                placeholder="+79991234567"
                className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
              />
            </div>

            <div>
              <label htmlFor="email" className="block text-sm font-medium text-gray-700">
                Email *
              </label>
              <input
                id="email"
                type="email"
                required
                readOnly
                value={formData.email || account?.email || ''}
                className={`mt-1 block w-full px-3 py-2 border rounded-md shadow-sm bg-gray-50 text-gray-500 cursor-not-allowed ${!formData.email && !account?.email
                  ? 'border-red-300 bg-red-50'
                  : 'border-gray-300'
                  }`}
                title="Email берется из вашего аккаунта"
              />
              {!formData.email && !account?.email ? (
                <p className="mt-1 text-xs text-red-600">
                  ⚠️ Email не найден. Пожалуйста, обновите страницу или войдите заново.
                </p>
              ) : (
                <p className="mt-1 text-xs text-gray-500">
                  Email берется из вашего аккаунта и не может быть изменен
                </p>
              )}
            </div>

            <div className="flex gap-4">
              <button
                type="submit"
                disabled={isLoading}
                className="flex-1 px-4 py-2 bg-indigo-600 text-white rounded hover:bg-indigo-700 disabled:opacity-50"
              >
                {isLoading ? 'Создание...' : 'Создать профиль'}
              </button>
              <button
                type="button"
                onClick={() => navigate('/profile')}
                className="px-4 py-2 border border-gray-300 rounded hover:bg-gray-50"
              >
                Отмена
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}

