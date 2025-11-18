import { useState } from 'react';
import { useUser } from '../../../../../hooks/useUser';
import type { Address } from '../../../../../models/User';
import { userService } from '../../../../../services/userService';
import AddressesList from './AddressesList';
import AddressForm from './AddressForm';
import AddressesPageHeader from './AddressesPageHeader';


export default function AddressesPage() {
  const { user, addresses, addAddress, updateAddress, deleteAddress, isLoading, error, loadUser } = useUser();
  const [showAddForm, setShowAddForm] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [formData, setFormData] = useState({
    street: '',
    city: '',
    state: '',
    country: '',
    zipCode: '',
    description: '',
    isPrimary: false,
  });

  const handleAdd = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await addAddress(formData);
      setFormData({
        street: '',
        city: '',
        state: '',
        country: '',
        zipCode: '',
        description: '',
        isPrimary: false,
      });
      setShowAddForm(false);
    } catch (err) {
      console.error('Ошибка добавления адреса:', err);
    }
  };

  const handleEdit = (address: Address) => {
    setEditingId(address.id);
    setFormData({
      street: address.street,
      city: address.city,
      state: address.state,
      country: address.country,
      zipCode: address.zipCode,
      description: address.description,
      isPrimary: address.isPrimary,
    });
    setShowAddForm(true);
  };

  const handleUpdate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editingId) return;
    try {
      await updateAddress(editingId, formData);
      setEditingId(null);
      setShowAddForm(false);
      setFormData({
        street: '',
        city: '',
        state: '',
        country: '',
        zipCode: '',
        description: '',
        isPrimary: false,
      });
    } catch (err) {
      console.error('Ошибка обновления адреса:', err);
    }
  };

  const handleDelete = async (addressId: string) => {
    if (!confirm('Вы уверены, что хотите удалить этот адрес?')) return;
    try {
      await deleteAddress(addressId);
    } catch (err) {
      console.error('Ошибка удаления адреса:', err);
    }
  };

  const handleSetPrimary = async (addressId: string) => {
    if (!user) return;
    try {
      await userService.setPrimaryAddress(user.id, addressId);
      // Перезагружаем данные пользователя
      await loadUser();
    } catch (err) {
      console.error('Ошибка установки основного адреса:', err);
      alert('Ошибка установки основного адреса');
    }
  };

  if (!user) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-lg text-gray-600">Сначала создайте профиль</div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-4xl mx-auto px-4">

        <AddressesPageHeader
          showAddForm={showAddForm}
          setShowAddForm={setShowAddForm}
          setEditingId={setEditingId}
          setFormData={setFormData}
        />

        {error && (
          <div className="mb-4 bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
            {error}
          </div>
        )}

        {showAddForm && (
          <AddressForm
            editingId={editingId}
            formData={formData}
            setFormData={setFormData}
            isLoading={isLoading}
            handleUpdate={handleUpdate}
            handleAdd={handleAdd}
            setShowAddForm={setShowAddForm}
            setEditingId={setEditingId}
          />
        )}

        <AddressesList
          addresses={addresses}
          handleSetPrimary={handleSetPrimary}
          handleEdit={handleEdit}
          handleDelete={handleDelete}
        />
      </div>
    </div>
  );
}


