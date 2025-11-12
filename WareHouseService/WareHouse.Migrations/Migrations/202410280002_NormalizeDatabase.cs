using FluentMigrator;

namespace WareHouse.Migrations.Migrations;

[Migration(202410280002)]
public class NormalizeDatabase : Migration
{
    public override void Up()
    {
        // 1. Создаем таблицу продуктов (нормализация)
        Execute.Sql(@"
            CREATE TABLE products (
                id UUID PRIMARY KEY,
                name VARCHAR(200) NOT NULL,
                sku VARCHAR(50) UNIQUE NOT NULL,
                description TEXT NULL,
                category VARCHAR(100) NOT NULL,
                unit_price NUMERIC(18,2) NOT NULL,
                weight_kg NUMERIC(8,3) NULL,
                requires_refrigeration BOOLEAN NOT NULL DEFAULT false,
                created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
            );
        ");

        // 2. Удаляем дублирующие поля из других таблиц и добавляем внешние ключи
        Execute.Sql(@"
            -- Удаляем product_name, sku из order_lines
            ALTER TABLE order_lines DROP COLUMN IF EXISTS product_name;
            ALTER TABLE order_lines DROP COLUMN IF EXISTS sku;
            
            -- Удаляем product_name, sku из storage_units
            ALTER TABLE storage_units DROP COLUMN IF EXISTS product_name;
            ALTER TABLE storage_units DROP COLUMN IF EXISTS sku;
            
            -- Удаляем product_name, sku из picking_items
            ALTER TABLE picking_items DROP COLUMN IF EXISTS product_name;
            ALTER TABLE picking_items DROP COLUMN IF EXISTS sku;

            -- Добавляем внешние ключи
            ALTER TABLE order_lines 
            ADD CONSTRAINT fk_order_lines_products 
            FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE RESTRICT;

            ALTER TABLE storage_units 
            ADD CONSTRAINT fk_storage_units_products
            FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE RESTRICT;

            ALTER TABLE picking_items 
            ADD CONSTRAINT fk_picking_items_products
            FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE RESTRICT;

            ALTER TABLE picking_tasks 
            ADD CONSTRAINT fk_picking_tasks_orders
            FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE;

            -- Добавляем проверочные ограничения
            ALTER TABLE orders 
            ADD CONSTRAINT chk_orders_status 
            CHECK (status IN ('Received', 'Picking', 'Picked', 'Completed', 'Cancelled'));

            ALTER TABLE picking_tasks 
            ADD CONSTRAINT chk_picking_tasks_status 
            CHECK (status IN ('Created', 'InProgress', 'Completed', 'Cancelled'));

            ALTER TABLE order_lines 
            ADD CONSTRAINT chk_order_lines_quantities 
            CHECK (quantity_ordered >= 0 AND quantity_picked >= 0 AND quantity_picked <= quantity_ordered);

            ALTER TABLE storage_units 
            ADD CONSTRAINT chk_storage_units_quantities 
            CHECK (quantity >= 0 AND reserved_quantity >= 0 AND reserved_quantity <= quantity);

            ALTER TABLE picking_items 
            ADD CONSTRAINT chk_picking_items_quantity 
            CHECK (quantity > 0);
        ");

        // Дополнительные индексы для производительности
        Execute.Sql(@"
            CREATE INDEX ix_products_sku ON products(sku);
            CREATE INDEX ix_products_category ON products(category);
            CREATE INDEX ix_order_lines_product_id ON order_lines(product_id);
            CREATE INDEX ix_picking_items_product_id ON picking_items(product_id);
        ");
    }

    public override void Down()
    {
        Execute.Sql(@"
            -- Восстанавливаем удаленные колонки
            ALTER TABLE order_lines ADD COLUMN product_name VARCHAR(200);
            ALTER TABLE order_lines ADD COLUMN sku VARCHAR(50);
            
            ALTER TABLE storage_units ADD COLUMN product_name VARCHAR(200);
            ALTER TABLE storage_units ADD COLUMN sku VARCHAR(50);
            
            ALTER TABLE picking_items ADD COLUMN product_name VARCHAR(200);
            ALTER TABLE picking_items ADD COLUMN sku VARCHAR(50);

            -- Удаляем ограничения
            ALTER TABLE order_lines DROP CONSTRAINT IF EXISTS fk_order_lines_products;
            ALTER TABLE storage_units DROP CONSTRAINT IF EXISTS fk_storage_units_products;
            ALTER TABLE picking_items DROP CONSTRAINT IF EXISTS fk_picking_items_products;
            ALTER TABLE picking_tasks DROP CONSTRAINT IF EXISTS fk_picking_tasks_orders;
            
            ALTER TABLE orders DROP CONSTRAINT IF EXISTS chk_orders_status;
            ALTER TABLE picking_tasks DROP CONSTRAINT IF EXISTS chk_picking_tasks_status;
            ALTER TABLE order_lines DROP CONSTRAINT IF EXISTS chk_order_lines_quantities;
            ALTER TABLE storage_units DROP CONSTRAINT IF EXISTS chk_storage_units_quantities;
            ALTER TABLE picking_items DROP CONSTRAINT IF EXISTS chk_picking_items_quantity;

            -- Удаляем индексы
            DROP INDEX IF EXISTS ix_products_sku;
            DROP INDEX IF EXISTS ix_products_category;
            DROP INDEX IF EXISTS ix_order_lines_product_id;
            DROP INDEX IF EXISTS ix_picking_items_product_id;

            -- Удаляем таблицу продуктов
            DROP TABLE IF EXISTS products;
        ");
    }
}