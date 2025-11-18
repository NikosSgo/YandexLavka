interface AddressesPageHeaderProps {
  showAddForm: boolean;
  setShowAddForm: (show: boolean) => void;
  setEditingId: (id: string | null) => void;
  setFormData: (data: {
    street: string;
    city: string;
    state: string;
    country: string;
    zipCode: string;
    description: string;
    isPrimary: boolean;
  }) => void;
}

const AddressesPageHeader = ({ showAddForm, setShowAddForm, setEditingId, setFormData }: AddressesPageHeaderProps) => {
  const handleButtonClick = () => {
    if (showAddForm) {
      setShowAddForm(false);
      setEditingId(null);
    } else {
      setShowAddForm(true);
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
    }
  };

  return (
    <div className="mb-6 flex justify-between items-center">
      <h1 className="text-3xl font-bold text-gray-900">Мои адреса</h1>
      <button
        onClick={handleButtonClick}
        className="px-4 py-2 bg-indigo-600 text-white rounded hover:bg-indigo-700"
      >
        {showAddForm ? 'Отмена' : 'Добавить адрес'}
      </button>
    </div>
  );
};

export default AddressesPageHeader;
