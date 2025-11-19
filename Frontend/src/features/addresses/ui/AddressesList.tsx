import type { Address } from '@entities/user/model/types';

interface AddressesListProps {
  addresses: Address[];
  onEdit: (address: Address) => void;
  onDelete: (addressId: string) => void;
  onSetPrimary: (addressId: string) => void;
}

export function AddressesList({
  addresses,
  onEdit,
  onDelete,
  onSetPrimary,
}: AddressesListProps) {
  if (addresses.length === 0) {
    return <p className="text-sm text-gray-500">Адреса не добавлены</p>;
  }

  return (
    <div className="space-y-3">
      {addresses.map((address) => (
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
              {address.street}, {address.city}, {address.state}, {address.country} {address.zipCode}
            </div>
          </div>
          <div className="mt-3 flex gap-2 text-sm">
            <button
              className="text-indigo-600 hover:text-indigo-800"
              onClick={() => onEdit(address)}
            >
              Редактировать
            </button>
            {!address.isPrimary && (
              <button
                className="text-green-600 hover:text-green-800"
                onClick={() => onSetPrimary(address.id)}
              >
                Сделать основным
              </button>
            )}
            <button
              className="text-red-600 hover:text-red-800"
              onClick={() => onDelete(address.id)}
            >
              Удалить
            </button>
          </div>
        </div>
      ))}
    </div>
  );
}


