import { Link } from 'react-router-dom';
import { useUser } from '../../../hooks/useUser';
import { useAuth } from '../../../hooks/useAuth';

export default function ProfilePage() {
  const { isAuthenticated, account } = useAuth();
  const { user, isLoading, error } = useUser();

  if (!isAuthenticated) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-lg text-gray-600">Пожалуйста, войдите в систему</div>
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-lg">Загрузка...</div>
      </div>
    );
  }

  if (error && !error.includes('not found')) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-lg text-red-600">{error}</div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-4xl mx-auto px-4">
        <div className="mb-6">
          <h1 className="text-3xl font-bold text-gray-900">Мой профиль</h1>
        </div>

        {!user ? (
          <div className="bg-white rounded-lg shadow-md p-6">
            <div className="text-center py-8">
              <p className="text-lg text-gray-600 mb-4">
                У вас еще нет профиля пользователя
              </p>
              <Link
                to="/profile/create"
                className="inline-block px-6 py-2 bg-indigo-600 text-white rounded hover:bg-indigo-700"
              >
                Создать профиль
              </Link>
            </div>
          </div>
        ) : (
          <div className="space-y-6">
            {/* Информация об аккаунте */}
            <div className="bg-white rounded-lg shadow-md p-6">
              <h2 className="text-xl font-semibold mb-4">Аккаунт</h2>
              <div className="space-y-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700">Email</label>
                  <div className="mt-1 text-sm text-gray-900">{account?.email}</div>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">ID аккаунта</label>
                  <div className="mt-1 text-sm text-gray-500 font-mono">{account?.id}</div>
                </div>
              </div>
            </div>

            {/* Информация о пользователе */}
            <div className="bg-white rounded-lg shadow-md p-6">
              <div className="flex justify-between items-center mb-4">
                <h2 className="text-xl font-semibold">Профиль пользователя</h2>
                <Link
                  to="/profile/edit"
                  className="text-sm text-indigo-600 hover:text-indigo-700"
                >
                  Редактировать
                </Link>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700">Имя</label>
                  <div className="mt-1 text-sm text-gray-900">{user.firstName}</div>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Фамилия</label>
                  <div className="mt-1 text-sm text-gray-900">{user.lastName}</div>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Телефон</label>
                  <div className="mt-1 text-sm text-gray-900">{user.phone}</div>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Email</label>
                  <div className="mt-1 text-sm text-gray-900">{user.email}</div>
                </div>
              </div>
            </div>

            {/* Адреса */}
            <div className="bg-white rounded-lg shadow-md p-6">
              <div className="flex justify-between items-center mb-4">
                <h2 className="text-xl font-semibold">Адреса</h2>
                <Link
                  to="/profile/addresses"
                  className="text-sm text-indigo-600 hover:text-indigo-700"
                >
                  Управление адресами
                </Link>
              </div>
              {user.addresses && user.addresses.length > 0 ? (
                <div className="space-y-3">
                  {user.addresses.map((address) => (
                    <div
                      key={address.id}
                      className={`p-4 border rounded-lg ${address.isPrimary ? 'border-indigo-500 bg-indigo-50' : 'border-gray-200'
                        }`}
                    >
                      {address.isPrimary && (
                        <span className="inline-block mb-2 px-2 py-1 text-xs font-semibold text-indigo-800 bg-indigo-100 rounded">
                          Основной
                        </span>
                      )}
                      <div className="text-sm">
                        <div className="font-medium">{address.description}</div>
                        <div className="text-gray-600">
                          {address.street}, {address.city}, {address.state}, {address.country} {address.zipCode}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <p className="text-sm text-gray-500">Адреса не добавлены</p>
              )}
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

