import { observer } from 'mobx-react-lite';
import { Link } from 'react-router-dom';
import { useAuthStore, useProfileStore } from '@app/providers/AppStoreProvider';
import { Loader } from '@shared/ui/Loader';
import { ErrorState } from '@shared/ui/ErrorState';

export const ProfilePage = observer(() => {
  const authStore = useAuthStore();
  const profileStore = useProfileStore();

  if (profileStore.isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <Loader label="Загрузка профиля..." />
      </div>
    );
  }

  if (profileStore.error && profileStore.error !== 'Профиль не найден') {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <ErrorState message={profileStore.error} />
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-4xl mx-auto px-4 space-y-6">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Мой профиль</h1>
        </div>
        {!profileStore.user ? (
          <div className="bg-white rounded-lg shadow-md p-6">
            <div className="text-center py-8">
              <p className="text-lg text-gray-600 mb-4">У вас еще нет профиля пользователя</p>
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
            <section className="bg-white rounded-lg shadow-md p-6">
              <h2 className="text-xl font-semibold mb-4">Аккаунт</h2>
              <div className="space-y-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700">Email</label>
                  <div className="mt-1 text-sm text-gray-900">{authStore.account?.email}</div>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">ID аккаунта</label>
                  <div className="mt-1 text-sm text-gray-500 font-mono">{authStore.account?.id}</div>
                </div>
              </div>
            </section>
            <section className="bg-white rounded-lg shadow-md p-6">
              <div className="flex justify-between items-center mb-4">
                <h2 className="text-xl font-semibold">Профиль пользователя</h2>
                <Link to="/profile/edit" className="text-sm text-indigo-600 hover:text-indigo-700">
                  Редактировать
                </Link>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <ProfileField label="Имя" value={profileStore.user.firstName} />
                <ProfileField label="Фамилия" value={profileStore.user.lastName} />
                <ProfileField label="Телефон" value={profileStore.user.phone} />
                <ProfileField label="Email" value={profileStore.user.email} />
              </div>
            </section>
            <section className="bg-white rounded-lg shadow-md p-6">
              <div className="flex justify-between items-center mb-4">
                <h2 className="text-xl font-semibold">Адреса</h2>
                <Link
                  to="/profile/addresses"
                  className="text-sm text-indigo-600 hover:text-indigo-700"
                >
                  Управление адресами
                </Link>
              </div>
              {profileStore.addresses.length > 0 ? (
                <div className="space-y-3">
                  {profileStore.addresses.map((address) => (
                    <div
                      key={address.id}
                      className={`p-4 border rounded-lg ${
                        address.isPrimary ? 'border-indigo-500 bg-indigo-50' : 'border-gray-200'
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
                          {address.street}, {address.city}, {address.state}, {address.country}{' '}
                          {address.zipCode}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <p className="text-sm text-gray-500">Адреса не добавлены</p>
              )}
            </section>
          </div>
        )}
      </div>
    </div>
  );
});

function ProfileField({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <label className="block text-sm font-medium text-gray-700">{label}</label>
      <div className="mt-1 text-sm text-gray-900">{value}</div>
    </div>
  );
}


