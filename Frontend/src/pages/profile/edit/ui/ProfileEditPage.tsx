import { type FormEvent, useEffect, useMemo, useState } from 'react';
import { observer } from 'mobx-react-lite';
import { useNavigate } from 'react-router-dom';
import { useAuthStore, useProfileStore } from '@app/providers/AppStoreProvider';
import { Loader } from '@shared/ui/Loader';
import { ErrorState } from '@shared/ui/ErrorState';

interface ProfileFormState {
  firstName: string;
  lastName: string;
  phone: string;
}

export const ProfileEditPage = observer(() => {
  const navigate = useNavigate();
  const authStore = useAuthStore();
  const profileStore = useProfileStore();
  const user = profileStore.user;

  const initialState = useMemo<ProfileFormState>(
    () => ({
      firstName: user?.firstName ?? '',
      lastName: user?.lastName ?? '',
      phone: user?.phone ?? '',
    }),
    [user],
  );

  const [formState, setFormState] = useState<ProfileFormState>(initialState);
  const [localError, setLocalError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    setFormState(initialState);
  }, [initialState]);

  if (profileStore.isLoading && !user) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <Loader label="Загрузка профиля..." />
      </div>
    );
  }

  if (!user) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <ErrorState message="Профиль не найден. Создайте профиль прежде чем редактировать." />
      </div>
    );
  }

  const accountEmail = authStore.account?.email ?? user.email;

  const handleChange = (field: keyof ProfileFormState, value: string) => {
    setFormState((prev) => ({ ...prev, [field]: value }));
  };

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setLocalError(null);

    if (!formState.firstName.trim() || !formState.lastName.trim() || !formState.phone.trim()) {
      setLocalError('Пожалуйста, заполните все обязательные поля.');
      return;
    }

    setIsSubmitting(true);
    try {
      await profileStore.updateProfile({
        firstName: formState.firstName.trim(),
        lastName: formState.lastName.trim(),
        phone: formState.phone.trim(),
      });
      navigate('/profile');
    } catch (error) {
      const message =
        error instanceof Error ? error.message : 'Не удалось обновить профиль. Попробуйте позже.';
      setLocalError(message);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-2xl mx-auto px-4">
        <div className="bg-white rounded-lg shadow-md p-6">
          <div className="mb-6">
            <h1 className="text-2xl font-bold">Редактирование профиля</h1>
            <p className="text-sm text-gray-500 mt-1">
              Обновите информацию о себе. Email берется из учетной записи и не редактируется.
            </p>
          </div>
          {localError && (
            <div className="mb-4">
              <ErrorState message={localError} />
            </div>
          )}
          <form className="space-y-4" onSubmit={handleSubmit}>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <TextField
                label="Имя *"
                value={formState.firstName}
                onChange={(value) => handleChange('firstName', value)}
              />
              <TextField
                label="Фамилия *"
                value={formState.lastName}
                onChange={(value) => handleChange('lastName', value)}
              />
            </div>
            <TextField
              label="Телефон *"
              value={formState.phone}
              onChange={(value) => handleChange('phone', value)}
              type="tel"
              placeholder="+79991234567"
            />
            <div>
              <label className="block text-sm font-medium text-gray-700">Email</label>
              <input
                type="email"
                value={accountEmail}
                readOnly
                className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm bg-gray-50 text-gray-500 cursor-not-allowed"
              />
            </div>
            <div className="flex gap-4">
              <button
                type="submit"
                disabled={isSubmitting}
                className="flex-1 px-4 py-2 bg-indigo-600 text-white rounded hover:bg-indigo-700 disabled:opacity-50"
              >
                {isSubmitting ? 'Сохранение...' : 'Сохранить изменения'}
              </button>
              <button
                type="button"
                onClick={() => navigate('/profile')}
                className="px-4 py-2 border border-gray-300 rounded hover:bg-gray-50"
                disabled={isSubmitting}
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
  type = 'text',
  placeholder,
}: {
  label: string;
  value: string;
  onChange: (value: string) => void;
  type?: string;
  placeholder?: string;
}) {
  return (
    <div>
      <label className="block text-sm font-medium text-gray-700">{label}</label>
      <input
        type={type}
        value={value}
        required
        onChange={(event) => onChange(event.target.value)}
        placeholder={placeholder}
        className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
      />
    </div>
  );
}

