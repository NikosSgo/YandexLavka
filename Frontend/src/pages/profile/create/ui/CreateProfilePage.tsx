import { type FormEvent, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { observer } from 'mobx-react-lite';
import { useAuthStore, useProfileStore } from '@app/providers/AppStoreProvider';
import { Loader } from '@shared/ui/Loader';
import { ErrorState } from '@shared/ui/ErrorState';

const emptyForm = {
  firstName: '',
  lastName: '',
  phone: '',
  email: '',
};

export const CreateProfilePage = observer(() => {
  const authStore = useAuthStore();
  const profileStore = useProfileStore();
  const navigate = useNavigate();

  const [formData, setFormData] = useState(emptyForm);
  const [localError, setLocalError] = useState<string | null>(null);
  const accountEmail = authStore.account?.email ?? '';

  useEffect(() => {
    if (accountEmail) {
      setFormData((prev) => ({ ...prev, email: accountEmail }));
    }
  }, [accountEmail]);

  if (profileStore.isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <Loader label="Загрузка профиля..." />
      </div>
    );
  }

  if (!authStore.account) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <ErrorState message="Вы не авторизованы" />
      </div>
    );
  }

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setLocalError(null);

    if (!formData.firstName.trim() || !formData.lastName.trim() || !formData.phone.trim()) {
      setLocalError('Пожалуйста, заполните все обязательные поля.');
      return;
    }

    if (!formData.email.trim()) {
      setLocalError('Email не найден. Попробуйте выйти и войти снова.');
      return;
    }

    try {
      await profileStore.createProfile({
        firstName: formData.firstName.trim(),
        lastName: formData.lastName.trim(),
        phone: formData.phone.trim(),
        email: formData.email.trim(),
      });
      navigate('/profile');
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Ошибка создания профиля';
      setLocalError(message);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-2xl mx-auto px-4">
        <div className="bg-white rounded-lg shadow-md p-6">
          <h1 className="text-2xl font-bold mb-6">Создание профиля</h1>
          {localError && (
            <div className="mb-4">
              <ErrorState message={localError} />
            </div>
          )}
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <TextField
                label="Имя *"
                value={formData.firstName}
                onChange={(value) => setFormData((prev) => ({ ...prev, firstName: value }))}
              />
              <TextField
                label="Фамилия *"
                value={formData.lastName}
                onChange={(value) => setFormData((prev) => ({ ...prev, lastName: value }))}
              />
            </div>
            <TextField
              label="Телефон *"
              value={formData.phone}
              onChange={(value) => setFormData((prev) => ({ ...prev, phone: value }))}
              placeholder="+79991234567"
            />
            <div>
              <label className="block text-sm font-medium text-gray-700">Email *</label>
              <input
                id="email"
                type="email"
                required
                readOnly
                value={formData.email}
                className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm bg-gray-50 text-gray-500 cursor-not-allowed"
                title="Email берется из вашего аккаунта"
              />
            </div>
            <div className="flex gap-4">
              <button
                type="submit"
                disabled={profileStore.isLoading}
                className="flex-1 px-4 py-2 bg-indigo-600 text-white rounded hover:bg-indigo-700 disabled:opacity-50"
              >
                {profileStore.isLoading ? 'Создание...' : 'Создать профиль'}
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
});

function TextField({
  label,
  value,
  onChange,
  placeholder,
}: {
  label: string;
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
}) {
  return (
    <div>
      <label className="block text-sm font-medium text-gray-700">{label}</label>
      <input
        type="text"
        required
        value={value}
        onChange={(event) => onChange(event.target.value)}
        placeholder={placeholder}
        className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
      />
    </div>
  );
}


