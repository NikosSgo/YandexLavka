import { useState } from 'react';
import type { Address } from '../../../models/User';
import { useUser } from '../../../hooks/useUser';
import { userService } from '../../../services/userService';

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
        <div className="mb-6 flex justify-between items-center">
          <h1 className="text-3xl font-bold text-gray-900">Мои адреса</h1>
          <button
            onClick={() => {
              setShowAddForm(!showAddForm);
              setEditingId(null);
              setFormData({
                street: '',
                city: '',
                state: '',
                country: '',
                zipCode: '',
                description: '',
                isPrimary: false,
              });
            }}
            className="px-4 py-2 bg-indigo-600 text-white rounded hover:bg-indigo-700"
          >
            {showAddForm ? 'Отмена' : 'Добавить адрес'}
          </button>
        </div>

        {error && (
          <div className="mb-4 bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
            {error}
          </div>
        )}

        {showAddForm && (
          <div className="bg-white rounded-lg shadow-md p-6 mb-6">
            <h2 className="text-xl font-semibold mb-4">
              {editingId ? 'Редактировать адрес' : 'Новый адрес'}
            </h2>
            <form onSubmit={editingId ? handleUpdate : handleAdd} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700">Описание *</label>
                <input
                  type="text"
                  required
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  placeholder="Дом, Работа и т.д."
                  className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700">Улица *</label>
                <input
                  type="text"
                  required
                  value={formData.street}
                  onChange={(e) => setFormData({ ...formData, street: e.target.value })}
                  className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700">Город *</label>
                  <input
                    type="text"
                    required
                    value={formData.city}
                    onChange={(e) => setFormData({ ...formData, city: e.target.value })}
                    className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Область/Регион *</label>
                  <input
                    type="text"
                    required
                    value={formData.state}
                    onChange={(e) => setFormData({ ...formData, state: e.target.value })}
                    className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700">Страна *</label>
                  <input
                    type="text"
                    required
                    value={formData.country}
                    onChange={(e) => setFormData({ ...formData, country: e.target.value })}
                    className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Индекс *</label>
                  <input
                    type="text"
                    required
                    value={formData.zipCode}
                    onChange={(e) => setFormData({ ...formData, zipCode: e.target.value })}
                    className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                  />
                </div>
              </div>

              <div className="flex items-center">
                <input
                  type="checkbox"
                  id="isPrimary"
                  checked={formData.isPrimary}
                  onChange={(e) => setFormData({ ...formData, isPrimary: e.target.checked })}
                  className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
                />
                <label htmlFor="isPrimary" className="ml-2 block text-sm text-gray-700">
                  Сделать основным адресом
                </label>
              </div>

              <div className="flex gap-4">
                <button
                  type="submit"
                  disabled={isLoading}
                  className="px-4 py-2 bg-indigo-600 text-white rounded hover:bg-indigo-700 disabled:opacity-50"
                >
                  {isLoading ? 'Сохранение...' : editingId ? 'Сохранить' : 'Добавить'}
                </button>
                <button
                  type="button"
                  onClick={() => {
                    setShowAddForm(false);
                    setEditingId(null);
                  }}
                  className="px-4 py-2 border border-gray-300 rounded hover:bg-gray-50"
                >
                  Отмена
                </button>
              </div>
            </form>
          </div>
        )}

        <div className="space-y-4">
          {addresses.length === 0 ? (
            <div className="bg-white rounded-lg shadow-md p-8 text-center">
              <p className="text-gray-500">У вас пока нет сохраненных адресов</p>
            </div>
          ) : (
            addresses.map((address) => (
              <div
                key={address.id}
                className={`bg-white rounded-lg shadow-md p-6 ${address.isPrimary ? 'border-2 border-indigo-500' : ''
                  }`}
              >
                <div className="flex justify-between items-start">
                  <div className="flex-1">
                    {address.isPrimary && (
                      <span className="inline-block mb-2 px-2 py-1 text-xs font-semibold text-indigo-800 bg-indigo-100 rounded">
                        Основной
                      </span>
                    )}
                    <h3 className="text-lg font-semibold mb-2">{address.description}</h3>
                    <p className="text-sm text-gray-600">
                      {address.street}, {address.city}, {address.state}
                    </p>
                    <p className="text-sm text-gray-600">
                      {address.country}, {address.zipCode}
                    </p>
                  </div>
                  <div className="flex gap-2">
                    {!address.isPrimary && (
                      <button
                        onClick={() => handleSetPrimary(address.id)}
                        className="px-3 py-1 text-sm text-green-600 hover:text-green-700"
                      >
                        Сделать основным
                      </button>
                    )}
                    <button
                      onClick={() => handleEdit(address)}
                      className="px-3 py-1 text-sm text-indigo-600 hover:text-indigo-700"
                    >
                      Редактировать
                    </button>
                    <button
                      onClick={() => handleDelete(address.id)}
                      className="px-3 py-1 text-sm text-red-600 hover:text-red-700"
                    >
                      Удалить
                    </button>
                  </div>
                </div>
              </div>
            ))
          )}
        </div>
      </div>
    </div>
  );
}

