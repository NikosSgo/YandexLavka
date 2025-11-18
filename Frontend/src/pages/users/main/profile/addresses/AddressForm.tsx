interface AddressFormProps {
  editingId: string | null;
  formData: {
    street: string;
    city: string;
    state: string;
    country: string;
    zipCode: string;
    description: string;
    isPrimary: boolean;
  };
  setFormData: (data: {
    street: string;
    city: string;
    state: string;
    country: string;
    zipCode: string;
    description: string;
    isPrimary: boolean;
  }) => void;
  isLoading: boolean;
  handleUpdate: (e: React.FormEvent) => void;
  handleAdd: (e: React.FormEvent) => void;
  setShowAddForm: (show: boolean) => void;
  setEditingId: (id: string | null) => void;
}

const AddressForm = ({
  editingId,
  formData,
  setFormData,
  isLoading,
  handleUpdate,
  handleAdd,
  setShowAddForm,
  setEditingId
}: AddressFormProps) => {
  return (
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
  );
};

export default AddressForm;
