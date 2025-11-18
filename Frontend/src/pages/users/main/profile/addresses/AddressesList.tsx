import type { Address } from "../../../../../models/User";

interface AddressesListProps {
  addresses: Address[];
  handleSetPrimary: (addressId: string) => void;
  handleEdit: (address: Address) => void;
  handleDelete: (addressId: string) => void;
}

const AddressesList = ({ addresses, handleSetPrimary, handleEdit, handleDelete }: AddressesListProps) => {
  return (
    <div className="space-y-4">
      {addresses.length === 0 ? (
        <div className="bg-white rounded-lg shadow-md p-8 text-center">
          <p className="text-gray-500">У вас пока нет сохраненных адресов</p>
        </div>
      ) : (
        addresses.map((address: Address) => (
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
  )
}

export default AddressesList;
