import type { FormEvent } from 'react';

export interface AddressFormState {
  street: string;
  city: string;
  state: string;
  country: string;
  zipCode: string;
  description: string;
  isPrimary: boolean;
}

interface AddressFormProps {
  mode: 'create' | 'edit';
  formData: AddressFormState;
  onChange: (value: AddressFormState) => void;
  onSubmit: (event: FormEvent<HTMLFormElement>) => void;
  onCancel: () => void;
  isLoading: boolean;
}

export function AddressForm({
  mode,
  formData,
  onChange,
  onSubmit,
  onCancel,
  isLoading,
}: AddressFormProps) {
  return (
    <div className="bg-white rounded-lg shadow-md p-6 mb-6">
      <h2 className="text-xl font-semibold mb-4">
        {mode === 'edit' ? 'Редактировать адрес' : 'Новый адрес'}
      </h2>
      <form onSubmit={onSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700">Описание *</label>
          <input
            type="text"
            required
            value={formData.description}
            onChange={(event) => onChange({ ...formData, description: event.target.value })}
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
            onChange={(event) => onChange({ ...formData, street: event.target.value })}
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
              onChange={(event) => onChange({ ...formData, city: event.target.value })}
              className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700">Область/Регион *</label>
            <input
              type="text"
              required
              value={formData.state}
              onChange={(event) => onChange({ ...formData, state: event.target.value })}
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
              onChange={(event) => onChange({ ...formData, country: event.target.value })}
              className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700">Индекс *</label>
            <input
              type="text"
              required
              value={formData.zipCode}
              onChange={(event) => onChange({ ...formData, zipCode: event.target.value })}
              className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
            />
          </div>
        </div>
        <div className="flex items-center">
          <input
            type="checkbox"
            id="isPrimary"
            checked={formData.isPrimary}
            onChange={(event) => onChange({ ...formData, isPrimary: event.target.checked })}
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
            {isLoading ? 'Сохранение...' : mode === 'edit' ? 'Сохранить' : 'Добавить'}
          </button>
          <button
            type="button"
            onClick={onCancel}
            className="px-4 py-2 border border-gray-300 rounded hover:bg-gray-50"
          >
            Отмена
          </button>
        </div>
      </form>
    </div>
  );
}


