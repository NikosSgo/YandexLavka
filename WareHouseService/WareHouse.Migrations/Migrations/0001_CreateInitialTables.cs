using FluentMigrator;

namespace WareHouse.Migrations.Migrations;

[Migration(202410280001)]
public class CreateInitialTables : Migration
{
    public override void Up()
    {
        Execute.Sql(@"
            CREATE TABLE orders (
                id UUID PRIMARY KEY,
                customer_id VARCHAR(100) NOT NULL,
                status TEXT NOT NULL,
                created_at TIMESTAMP WITH TIME ZONE NOT NULL,
                picking_started_at TIMESTAMP WITH TIME ZONE NULL,
                picking_completed_at TIMESTAMP WITH TIME ZONE NULL
            );

            CREATE TABLE order_lines (
                order_id UUID NOT NULL,
                product_id UUID NOT NULL,
                product_name VARCHAR(200) NOT NULL,
                sku VARCHAR(50) NOT NULL,
                quantity_ordered INTEGER NOT NULL,
                quantity_picked INTEGER NOT NULL,
                unit_price NUMERIC(18,2) NOT NULL,
                PRIMARY KEY (order_id, product_id),
                CONSTRAINT fk_order_lines_orders FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE
            );

            CREATE TABLE picking_tasks (
                id UUID PRIMARY KEY,
                order_id UUID NOT NULL,
                assigned_picker VARCHAR(50) NULL,
                status TEXT NOT NULL,
                zone VARCHAR(20) NOT NULL,
                created_at TIMESTAMP WITH TIME ZONE NOT NULL,
                completed_at TIMESTAMP WITH TIME ZONE NULL
            );

            CREATE TABLE picking_items (
                picking_task_id UUID NOT NULL,
                product_id UUID NOT NULL,
                product_name VARCHAR(200) NOT NULL,
                sku VARCHAR(50) NOT NULL,
                quantity INTEGER NOT NULL,
                storage_location VARCHAR(20) NOT NULL,
                barcode VARCHAR(100) NULL,
                PRIMARY KEY (picking_task_id, product_id),
                CONSTRAINT fk_picking_items_picking_tasks FOREIGN KEY (picking_task_id) REFERENCES picking_tasks(id) ON DELETE CASCADE
            );

            CREATE TABLE storage_units (
                id UUID PRIMARY KEY,
                product_id UUID NOT NULL,
                product_name VARCHAR(200) NOT NULL,
                sku VARCHAR(50) NOT NULL,
                quantity INTEGER NOT NULL,
                reserved_quantity INTEGER NOT NULL,
                location VARCHAR(20) NOT NULL,
                zone VARCHAR(20) NOT NULL,
                last_restocked TIMESTAMP WITH TIME ZONE NOT NULL
            );
        ");

        // Создаем индексы
        Execute.Sql(@"
            CREATE INDEX ix_orders_customer_id ON orders (customer_id);
            
            CREATE INDEX ix_picking_tasks_order_id ON picking_tasks (order_id);
            CREATE INDEX ix_picking_tasks_status ON picking_tasks (status);
            CREATE INDEX ix_picking_tasks_zone ON picking_tasks (zone);
            CREATE INDEX ix_picking_tasks_assigned_picker ON picking_tasks (assigned_picker);
            
            CREATE INDEX ix_storage_units_product_id ON storage_units (product_id);
            CREATE INDEX ix_storage_units_location ON storage_units (location);
            CREATE INDEX ix_storage_units_zone ON storage_units (zone);
            CREATE UNIQUE INDEX ix_storage_units_product_id_location ON storage_units (product_id, location);
        ");
    }

    public override void Down()
    {
        Execute.Sql(@"
            DROP TABLE IF EXISTS storage_units;
            DROP TABLE IF EXISTS picking_items;
            DROP TABLE IF EXISTS picking_tasks;
            DROP TABLE IF EXISTS order_lines;
            DROP TABLE IF EXISTS orders;
        ");
    }
}