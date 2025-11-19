import { type FormEvent, useState } from 'react';
import { observer } from 'mobx-react-lite';
import { useProfileStore } from '@app/providers/AppStoreProvider';
import { AddressForm, type AddressFormState } from '@features/addresses/ui/AddressForm';
import { AddressesList } from '@features/addresses/ui/AddressesList';
import { Loader } from '@shared/ui/Loader';
import { ErrorState } from '@shared/ui/ErrorState';

const emptyForm: AddressFormState = {
  street: '',
  city: '',
  state: '',
  country: '',
  zipCode: '',
  description: '',
  isPrimary: false,
};

export const AddressesPage = observer(() => {
  const profileStore = useProfileStore();
  const [formState, setFormState] = useState<AddressFormState>(emptyForm);
  const [mode, setMode] = useState<'create' | 'edit'>('create');
  const [editingId, setEditingId] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [localError, setLocalError] = useState<string | null>(null);

  if (!profileStore.user) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <ErrorState message="Сначала создайте профиль" />
      </div>
    );
  }

  const resetForm = () => {
    setFormState(emptyForm);
    setEditingId(null);
    setMode('create');
  };

  const handleCreate = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setLocalError(null);
    try {
      await profileStore.addAddress(formState);
      resetForm();
      setShowForm(false);
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Ошибка добавления адреса';
      setLocalError(message);
    }
  };

  const handleUpdate = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!editingId) {
      return;
    }
    setLocalError(null);
    try {
      await profileStore.updateAddress(editingId, formState);
      resetForm();
      setShowForm(false);
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Ошибка обновления адреса';
      setLocalError(message);
    }
  };

  const handleEditClick = (addressId: string, form: AddressFormState) => {
    setFormState(form);
    setEditingId(addressId);
    setMode('edit');
    setShowForm(true);
  };

  const startCreate = () => {
    resetForm();
    setShowForm(true);
    setMode('create');
  };

  const handleDelete = async (addressId: string) => {
    if (!confirm('Вы уверены, что хотите удалить этот адрес?')) {
      return;
    }
    try {
      await profileStore.deleteAddress(addressId);
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Ошибка удаления адреса';
      setLocalError(message);
    }
  };

  const handleSetPrimary = async (addressId: string) => {
    try {
      await profileStore.setPrimaryAddress(addressId);
    } catch (error) {
      const message =
        error instanceof Error ? error.message : 'Ошибка установки основного адреса';
      setLocalError(message);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-4xl mx-auto px-4 space-y-6">
        <div className="flex justify-between items-center">
          <h1 className="text-2xl font-bold">Мои адреса</h1>
          <button
            onClick={startCreate}
            className="px-4 py-2 bg-indigo-600 text-white rounded hover:bg-indigo-700"
          >
            Добавить адрес
          </button>
        </div>
        {localError && (
          <div>
            <ErrorState message={localError} />
          </div>
        )}
        {showForm && (
          <AddressForm
            mode={mode}
            formData={formState}
            onChange={setFormState}
            onSubmit={mode === 'edit' ? handleUpdate : handleCreate}
            onCancel={() => {
              setShowForm(false);
              resetForm();
            }}
            isLoading={profileStore.isLoading}
          />
        )}
        {profileStore.isLoading && !showForm ? (
          <Loader label="Загружаем адреса..." />
        ) : (
          <AddressesList
            addresses={profileStore.addresses}
            onEdit={(address) =>
              handleEditClick(address.id, {
                street: address.street,
                city: address.city,
                state: address.state,
                country: address.country,
                zipCode: address.zipCode,
                description: address.description,
                isPrimary: address.isPrimary,
              })
            }
            onDelete={handleDelete}
            onSetPrimary={handleSetPrimary}
          />
        )}
      </div>
    </div>
  );
});


