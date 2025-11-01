using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WareHouse.Domain.Enums;

public enum OrderStatus
{
    Received = 1,    // Заказ получен
    Picking = 2,     // В процессе сборки
    //Picked = 3,      // Собран
    Completed = 3,   // Завершен (бывший Packed)
    Cancelled = 4    // Отменен
}

public enum PickingTaskStatus
{
    Created = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4
}

public enum StorageUnitType
{
    Shelf = 1,       // Полка
    Pallet = 2,      // Паллета
    Box = 3,         // Коробка
    Refrigerated = 4 // Холодильник
}